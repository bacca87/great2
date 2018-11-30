using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Utils;
using Great.Utils.Extensions;
using Great.Utils.Messages;
using Great.ViewModels.Database;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Great.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ExpenseAccountViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Properties
        private FDLManager _fdlManager;

        public int NotesMaxLength { get { return ApplicationSettings.ExpenseAccount.NotesMaxLength; } }
                
        private bool _isInputEnabled = false;
        public bool IsInputEnabled
        {
            get => _isInputEnabled;
            set
            {
                Set(ref _isInputEnabled, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollectionEx<ExpenseAccountEVM> ExpenseAccounts { get; set; }

        private ExpenseAccountEVM _selectedEA;
        public ExpenseAccountEVM SelectedEA
        {
            get => _selectedEA;
            set
            {
                Set(ref _selectedEA, value);

                if (_selectedEA != null)
                {
                    SelectedExpense = null;
                    IsInputEnabled = true;
                    UpdateDaysOfWeek();
                }
                else
                    IsInputEnabled = false;
            }
        }

        private ExpenseEVM _selectedExpense;
        public ExpenseEVM SelectedExpense
        {
            get => _selectedExpense;
            set => Set(ref _selectedExpense, value);
        }

        public ObservableCollection<ExpenseTypeDTO> ExpenseTypes { get; internal set; }
        public ObservableCollection<CurrencyDTO> Currencies { get; internal set; }

        public MRUCollection<string> MRUEmailRecipients { get; set; }

        private string _sendToEmailRecipient;
        public string SendToEmailRecipient
        {
            get => _sendToEmailRecipient;
            set => Set(ref _sendToEmailRecipient, value);
        }

        private string[] _DaysOfWeek;
        public string[] DaysOfWeek
        {
            get => _DaysOfWeek;
            set => Set(ref _DaysOfWeek, value);
        }
        
        #endregion

        #region Commands Definitions
        public RelayCommand<ExpenseAccountEVM> SaveCommand { get; set; }

        public RelayCommand<ExpenseAccountEVM> SendToSAPCommand { get; set; }
        public RelayCommand<string> SendByEmailCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> SaveAsCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> OpenCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> MarkAsRefundedCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> MarkAsAcceptedCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> MarkAsCancelledCommand { get; set; }
        #endregion

        #region Errors Validation
        public string CurrencyText { get; set; }
        public string ExpenseTypeText { get; set; }        

        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            get
            {
                switch(columnName)
                {
                    case "CurrencyText":
                        if (!string.IsNullOrEmpty(CurrencyText) && !Currencies.Any(c => c.Description == CurrencyText))
                            return "Select a valid currency from the combo list!";
                        break;

                    case "ExpenseTypeText":
                        if (!string.IsNullOrEmpty(ExpenseTypeText) && !ExpenseTypes.Any(t => t.Description == ExpenseTypeText))
                            return "Select a valid expense type from the combo list!";
                        break;

                    default:
                        break;
                }

                return null;
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the ExpenseAccountViewModel class.
        /// </summary>
        public ExpenseAccountViewModel(FDLManager manager)
        {
            _fdlManager = manager;

            SaveCommand = new RelayCommand<ExpenseAccountEVM>(SaveEA, (ExpenseAccountEVM ea) => { return IsInputEnabled; });

            SendToSAPCommand = new RelayCommand<ExpenseAccountEVM>(SendToSAP);
            SendByEmailCommand = new RelayCommand<string>(SendByEmail);
            SaveAsCommand = new RelayCommand<ExpenseAccountEVM>(SaveAs);
            OpenCommand = new RelayCommand<ExpenseAccountEVM>(Open);
            MarkAsRefundedCommand = new RelayCommand<ExpenseAccountEVM>(MarkAsRefunded);
            MarkAsAcceptedCommand = new RelayCommand<ExpenseAccountEVM>(MarkAsAccepted);
            MarkAsCancelledCommand = new RelayCommand<ExpenseAccountEVM>(MarkAsCancelled);

            using (DBArchive db = new DBArchive())
            {
                ExpenseTypes = new ObservableCollection<ExpenseTypeDTO>(db.ExpenseTypes.ToList().Select(t => new ExpenseTypeDTO(t)));
                ExpenseAccounts = new ObservableCollectionEx<ExpenseAccountEVM>(db.ExpenseAccounts.ToList().Select(ea => new ExpenseAccountEVM(ea)));
                Currencies = new ObservableCollection<CurrencyDTO>(db.Currencies.ToList().Select(c => new CurrencyDTO(c)));
            }

            MessengerInstance.Register<NewItemMessage<ExpenseAccountEVM>>(this, NewEA);
            MessengerInstance.Register<ItemChangedMessage<ExpenseAccountEVM>>(this, EAChanged);

            List<string> recipients = UserSettings.Email.Recipients.MRU?.Cast<string>().ToList();

            if (recipients != null)
                MRUEmailRecipients = new MRUCollection<string>(ApplicationSettings.EmailRecipients.MRUSize, new Collection<string>(recipients));
            else
                MRUEmailRecipients = new MRUCollection<string>(ApplicationSettings.EmailRecipients.MRUSize);
        }

        public void NewEA(NewItemMessage<ExpenseAccountEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null && ExpenseAccounts.SingleOrDefault(ea => ea.Id == (item.Content as ExpenseAccountEVM).Id) == null)
                        ExpenseAccounts.Add(item.Content);
                })
            );
        }

        public void EAChanged(ItemChangedMessage<ExpenseAccountEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        ExpenseAccountEVM ea = ExpenseAccounts.SingleOrDefault(x => x.Id == (item.Content as ExpenseAccountEVM).Id);

                        if (ea != null)
                            ea.Status = (item.Content as ExpenseAccountEVM).Status;
                    }
                })
            );
        }

        private void UpdateDaysOfWeek()
        {
            if (SelectedEA == null)
                return;

            DateTime StartDay = DateTime.Now.FromUnixTimestamp(SelectedEA.FDL1.StartDay);
            DateTime StartDayOfWeek = StartDay.AddDays((int)DayOfWeek.Monday - (int)StartDay.DayOfWeek);
            var Days = Enumerable.Range(0, 7).Select(i => StartDayOfWeek.AddDays(i)).ToArray();

            string[] tmpDays = new string[7];

            for(int i = 0; i < 7; i++)
                tmpDays[i] = Days[i].Day >= StartDay.Day ? Days[i].ToShortDateString() : string.Empty;

            DaysOfWeek = tmpDays;
        }

        public void SaveEA(ExpenseAccountEVM ea)
        {
            if (ea == null)
                return;

            if (string.IsNullOrEmpty(ea.Currency))
            {
                MessageBox.Show("Please select the currency before continue. Operation cancelled.", "Invalid Expense Account", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (DBArchive db = new DBArchive())
            {
                ea.Save(db);

                if (ea.Id == 0)
                    db.SaveChanges();

                foreach (var expense in ea.Expenses)
                {
                    expense.ExpenseAccount = ea.Id;
                    expense.Save(db);
                }

                db.SaveChanges();
            }
        }

        public void SendToSAP(ExpenseAccountEVM ea)
        {
            if (ea.EStatus == EFDLStatus.Waiting &&
                MessageBox.Show("The selected expense account was already sent. Do you want send it again?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            _fdlManager.SendToSAP(ea);
        }

        public void SendByEmail(string address)
        {
            string error;

            if (!MSExchangeProvider.CheckEmailAddress(address, out error))
            {
                MessageBox.Show(error, "Invalid Email Address", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // reset input box
            SendToEmailRecipient = string.Empty;

            MRUEmailRecipients.Add(address);

            // save to user setting the MRU recipients
            StringCollection collection = new StringCollection();
            collection.AddRange(MRUEmailRecipients.ToArray());
            UserSettings.Email.Recipients.MRU = collection;

            _fdlManager.SendTo(address, SelectedEA);
        }

        public void SaveAs(ExpenseAccountEVM ea)
        {
            if (ea == null)
                return;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save Expense Account As...";
            dlg.FileName = ea.FileName;
            dlg.DefaultExt = ".pdf";
            dlg.Filter = "EA (.pdf) | *.pdf";
            dlg.AddExtension = true;
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (dlg.ShowDialog() == true)
                _fdlManager.SaveAs(ea, dlg.FileName);
        }

        public void Open(ExpenseAccountEVM ea)
        {
            if (ea == null)
                return;

            string fileName = Path.GetTempPath() + Path.GetFileNameWithoutExtension(ea.FileName) + ".XFDF";

            _fdlManager.SaveXFDF(ea, fileName);
            Process.Start(fileName);
        }

        public void MarkAsRefunded(ExpenseAccountEVM ea)
        {
            if (MessageBox.Show("Are you sure to mark as refunded the selected expense account?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            ea.IsRefunded = true;
            ea.Save();
        }

        public void MarkAsAccepted(ExpenseAccountEVM ea)
        {
            if (MessageBox.Show("Are you sure to mark as accepted the selected expense account?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            ea.EStatus = EFDLStatus.Accepted;
            ea.Save();
        }

        public void MarkAsCancelled(ExpenseAccountEVM ea)
        {
            if (MessageBox.Show("Are you sure to mark as Cancelled the selected expense account?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            ea.EStatus = EFDLStatus.Cancelled;
            ea.Save();
        }
    }
}