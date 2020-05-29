using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Great2.Models;
using Great2.Models.Database;
using Great2.Models.DTO;
using Great2.Utils;
using Great2.Utils.Extensions;
using Great2.Utils.Messages;
using Great2.ViewModels.Database;
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

namespace Great2.ViewModels
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

        private int _currentYear = DateTime.Now.Year;
        public int CurrentYear
        {
            get => _currentYear;
            set
            {
                bool updateDays = _currentYear != value;
                int year = 0;

                if (value < ApplicationSettings.Timesheets.MinYear)
                    year = ApplicationSettings.Timesheets.MinYear;
                else if (value > ApplicationSettings.Timesheets.MaxYear)
                    year = ApplicationSettings.Timesheets.MaxYear;
                else
                    year = value;

                Set(ref _currentYear, year);

                if (updateDays)
                    UpdateEaList();
            }
        }

        private FDLManager _fdlManager;

        public int NotesMaxLength => ApplicationSettings.ExpenseAccount.NotesMaxLength;

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

        private ObservableCollectionEx<ExpenseAccountEVM> _ExpenseAccounts;
        public ObservableCollectionEx<ExpenseAccountEVM> ExpenseAccounts
        {
            get => _ExpenseAccounts;
            set => Set(ref _ExpenseAccounts, value);
        }

        private ExpenseAccountEVM _selectedEA;
        public ExpenseAccountEVM SelectedEA
        {
            get => _selectedEA;
            set
            {
                _selectedEA?.CheckChangedEntity();

                Set(ref _selectedEA, value);

                if (_selectedEA != null)
                {
                    SelectedExpense = null;
                    IsInputEnabled = true;

                    using (DBArchive db = new DBArchive())
                        ExpenseTypes = new ObservableCollection<ExpenseTypeEVM>(db.ExpenseTypes.Where(t => t.Category == (SelectedEA.IsExcel ? 1 : 0)).ToList().Select(t => new ExpenseTypeEVM(t)));

                    SendToSAPCommand.RaiseCanExecuteChanged();
                    NewMessageCommand.RaiseCanExecuteChanged();
                    CompileCommand.RaiseCanExecuteChanged();
                    SendByEmailCommand.RaiseCanExecuteChanged();
                    SaveAsCommand.RaiseCanExecuteChanged();
                    OpenCommand.RaiseCanExecuteChanged();
                    MarkAsRefundedCommand.RaiseCanExecuteChanged();
                    MarkAsAcceptedCommand.RaiseCanExecuteChanged();
                    MarkAsCancelledCommand.RaiseCanExecuteChanged();
                    PrintCommand.RaiseCanExecuteChanged();
                }
                else
                    IsInputEnabled = false;

                ShowEditMenu = false;
            }
        }

        private ExpenseEVM _selectedExpense;
        public ExpenseEVM SelectedExpense
        {
            get => _selectedExpense;
            set => Set(ref _selectedExpense, value);
        }

        public ObservableCollection<ExpenseTypeEVM> ExpenseTypes { get; internal set; }
        public ObservableCollection<CurrencyDTO> Currencies { get; internal set; }

        public MRUCollection<string> MRUEmailRecipients { get; set; }

        private string _sendToEmailRecipient;
        public string SendToEmailRecipient
        {
            get => _sendToEmailRecipient;
            set => Set(ref _sendToEmailRecipient, value);
        }

        private bool _showEditMenu;
        public bool ShowEditMenu
        {
            get => _showEditMenu;
            set => Set(ref _showEditMenu, value);
        }

        public Action<long> OnFactoryLink { get; set; }

        #endregion

        #region Commands Definitions
        public RelayCommand<ExpenseAccountEVM> SaveCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> SendToSAPCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> NewMessageCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> CompileCommand { get; set; }
        public RelayCommand<string> SendByEmailCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> SaveAsCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> OpenCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> MarkAsRefundedCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> MarkAsAcceptedCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> MarkAsCancelledCommand { get; set; }
        public RelayCommand GotFocusCommand { get; set; }
        public RelayCommand LostFocusCommand { get; set; }
        public RelayCommand PageUnloadedCommand { get; set; }
        public RelayCommand NextYearCommand { get; set; }
        public RelayCommand PreviousYearCommand { get; set; }
        public RelayCommand FactoryLinkCommand { get; set; }
        public RelayCommand<ExpenseAccountEVM> PrintCommand { get; set; }
        #endregion

        #region Errors Validation
        public string CurrencyText { get; set; }
        public string ExpenseTypeText { get; set; }

        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
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

            NextYearCommand = new RelayCommand(() => CurrentYear++);
            PreviousYearCommand = new RelayCommand(() => CurrentYear--);

            SaveCommand = new RelayCommand<ExpenseAccountEVM>(SaveEA, (ExpenseAccountEVM ea) => { return IsInputEnabled; });

            SendToSAPCommand = new RelayCommand<ExpenseAccountEVM>(SendToSAP, (x) => { return SelectedEA != null && !SelectedEA.IsVirtual; });
            NewMessageCommand = new RelayCommand<ExpenseAccountEVM>(NewMessage, (x) => { return SelectedEA != null && !SelectedEA.IsVirtual; });
            CompileCommand = new RelayCommand<ExpenseAccountEVM>(Compile, (x) => { return SelectedEA != null && !SelectedEA.IsVirtual; });
            SendByEmailCommand = new RelayCommand<string>(SendByEmail, (x) => { return SelectedEA != null && !SelectedEA.IsVirtual; });
            SaveAsCommand = new RelayCommand<ExpenseAccountEVM>(SaveAs, (x) => { return SelectedEA != null && !SelectedEA.IsVirtual; });
            OpenCommand = new RelayCommand<ExpenseAccountEVM>(Open, (x) => { return SelectedEA != null && !SelectedEA.IsVirtual; });
            MarkAsRefundedCommand = new RelayCommand<ExpenseAccountEVM>(MarkAsRefunded, (x) => { return SelectedEA != null; });
            MarkAsAcceptedCommand = new RelayCommand<ExpenseAccountEVM>(MarkAsAccepted, (x) => { return SelectedEA != null; });
            MarkAsCancelledCommand = new RelayCommand<ExpenseAccountEVM>(MarkAsCancelled, (x) => { return SelectedEA != null; });
            PrintCommand = new RelayCommand<ExpenseAccountEVM>(Print, (x) => { return SelectedEA != null && !SelectedEA.IsVirtual; });
            GotFocusCommand = new RelayCommand(() => { ShowEditMenu = true; });
            LostFocusCommand = new RelayCommand(() => { });
            PageUnloadedCommand = new RelayCommand(() => { SelectedEA?.CheckChangedEntity(); });
            FactoryLinkCommand = new RelayCommand(FactoryLink);

            using (DBArchive db = new DBArchive())
            {
                string year = CurrentYear.ToString();                
                Currencies = new ObservableCollection<CurrencyDTO>(db.Currencies.ToList().Select(c => new CurrencyDTO(c)));
                ExpenseAccounts = new ObservableCollectionEx<ExpenseAccountEVM>(db.ExpenseAccounts.ToList().Select(ea => new ExpenseAccountEVM(ea)));
            }

            UpdateEaList();

            MessengerInstance.Register<NewItemMessage<ExpenseAccountEVM>>(this, NewEA);
            MessengerInstance.Register<ItemChangedMessage<ExpenseAccountEVM>>(this, EAChanged);
            MessengerInstance.Register<ItemChangedMessage<FactoryEVM>>(this, FactoryChanged);
            MessengerInstance.Register<ItemChangedMessage<FDLEVM>>(this, FDLChanged);
            MessengerInstance.Register<ItemChangedMessage<TimesheetEVM>>(this, TimeSheetChanged);
            MessengerInstance.Register<DeletedItemMessage<TimesheetEVM>>(this, TimeSheetDeleted);

            List<string> recipients = UserSettings.Email.Recipients.MRU?.Cast<string>().ToList();

            if (recipients != null)
                MRUEmailRecipients = new MRUCollection<string>(ApplicationSettings.EmailRecipients.MRUSize, new Collection<string>(recipients));
            else
                MRUEmailRecipients = new MRUCollection<string>(ApplicationSettings.EmailRecipients.MRUSize);
        }

        private void NewEA(NewItemMessage<ExpenseAccountEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null && !ExpenseAccounts.Any(ea => ea.Id == item.Content.Id))
                        ExpenseAccounts.Add(item.Content);
                })
            );
        }

        private void EAChanged(ItemChangedMessage<ExpenseAccountEVM> item)
        {
            if (item.Sender == this)
                return;

            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        ExpenseAccountEVM ea = ExpenseAccounts.SingleOrDefault(x => x.Id == item.Content.Id);

                        if (ea != null)
                        {
                            ea.Status = item.Content.Status;
                            ea.NotifyAsNew = item.Content.NotifyAsNew;
                            ea.LastError = item.Content.LastError;
                        }
                    }
                })
            );
        }

        private void FactoryChanged(ItemChangedMessage<FactoryEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        FactoryDTO factory = Auto.Mapper.Map<FactoryDTO>(item.Content);

                        if (factory != null)
                        {
                            var eaToUpdate = ExpenseAccounts.Where(e => e.FDL1.Factory.HasValue && e.FDL1.Factory.Value == item.Content.Id);

                            foreach (var ea in eaToUpdate)
                            {
                                ea.FDL1.Factory1 = factory;
                                ea.FDL1 = ea.FDL1; // hack to force the View to update the factory name
                            }
                        }
                    }
                })
            );
        }

        private void FDLChanged(ItemChangedMessage<FDLEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        var eaToUpdate = ExpenseAccounts.Where(e => e.FDL1 != null && e.FDL == item.Content.Id);

                        foreach (var ea in eaToUpdate)
                        {
                            if(ea.FDL1.Factory1 != item.Content.Factory1)
                            {
                                ea.FDL1.Factory1 = item.Content.Factory1;
                                ea.RaisePropertyChanged(nameof(ea.Factory_Display));

                                if(ea.FDL1.Factory1 != null)
                                {
                                    ea.InitExpensesByCountry();
                                    ea.UpdateDiariaAndPocketMoney(item.Content.Timesheets);
                                }
                            }
                        }   
                    }
                })
            );
        }

        private void TimeSheetChanged(ItemChangedMessage<TimesheetEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        ExpenseAccountEVM ea = ExpenseAccounts.SingleOrDefault(x => x.FDL == item.Content.FDL1?.Id && x.Currency == "EUR");

                        if (ea != null)
                        {                            
                            ea.UpdateDiaria(item.Content, false);
                            ea.UpdatePocketMoney(item.Content, false);
                        }
                    }
                })
            );
        }

        private void TimeSheetDeleted(DeletedItemMessage<TimesheetEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        ExpenseAccountEVM ea = ExpenseAccounts.SingleOrDefault(x => x.FDL == item.Content.FDL1?.Id && x.Currency == "EUR");

                        if (ea != null)
                        {
                            ea.UpdateDiaria(item.Content, true);
                            ea.UpdatePocketMoney(item.Content, true);
                        }
                    }
                })
            );
        }

        public void SaveEA(ExpenseAccountEVM ea)
        {
            if (ea == null || ea.IsReadOnly)
                return;

            if (string.IsNullOrEmpty(ea.Currency))
            {
                MetroMessageBox.Show("Please select the currency before continue. Operation cancelled.", "Invalid Expense Account", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (DBArchive db = new DBArchive())
            {
                var oldEa = new ExpenseAccountEVM(db.ExpenseAccounts.SingleOrDefault(x => x.Id == ea.Id));
                ea.IsCompiled = ea.IsCompiled ? ea.Equals(oldEa) : false;
                ea.NotifyAsNew = false;
                ea.Save(db);

                if (ea.Id == 0)
                    db.SaveChanges();
                else
                    db.Expenses.RemoveRange(db.Expenses.Where(e => e.ExpenseAccount == ea.Id));

                foreach (var expense in ea.Expenses)
                {
                    expense.Id = 0;
                    expense.ExpenseAccount = ea.Id;
                    expense.Save(db);
                }

                db.SaveChanges();
                ea.IsChanged = false;
            }

            // update notifications
            Messenger.Default.Send(new ItemChangedMessage<ExpenseAccountEVM>(this, ea));
            ShowEditMenu = false;
        }

        public void SendToSAP(ExpenseAccountEVM ea)
        {
            if (!ea.IsCompiled)
            {
                MetroMessageBox.Show("The selected EA is not compiled! Compile the EA before send it to SAP. Operation cancelled!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_fdlManager.IsExchangeAvailable())
            {
                MetroMessageBox.Show("The email server is not reachable, please check your connection. Operation cancelled!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if ((ea.EStatus == EFDLStatus.Waiting || ea.EStatus == EFDLStatus.Accepted) &&
                MetroMessageBox.Show("The selected expense account was already sent. Would you like to send it again?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            using (new WaitCursor())
            {
                if (_fdlManager.SendToSAP(ea))
                    ea.EStatus = EFDLStatus.Waiting; // don't save the fdl status until the message is sent
            }
        }

        public void SendByEmail(string address)
        {
            string error;

            if (!SelectedEA.IsCompiled)
            {
                MetroMessageBox.Show("The selected EA is not compiled! Compile the EA before send it by e-mail. Operation cancelled!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_fdlManager.IsExchangeAvailable())
            {
                MetroMessageBox.Show("The email server is not reachable, please check your connection. Operation cancelled!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!MSExchangeProvider.CheckEmailAddress(address, out error))
            {
                MetroMessageBox.Show(error, "Invalid Email Address", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (new WaitCursor())
            {
                // reset input box
                SendToEmailRecipient = string.Empty;

                MRUEmailRecipients.Add(address);

                // save to user setting the MRU recipients
                StringCollection collection = new StringCollection();
                collection.AddRange(MRUEmailRecipients.ToArray());
                UserSettings.Email.Recipients.MRU = collection;

                _fdlManager.SendTo(address, SelectedEA);
            }
        }

        public void NewMessage(ExpenseAccountEVM ea)
        {
            if (!SelectedEA.IsCompiled)
            {
                MetroMessageBox.Show("The selected EA is not compiled! Compile the EA before send it by e-mail. Operation cancelled!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (new WaitCursor())
            {
                _fdlManager.NewOutlookMessage(ea);
            }
        }

        public void Print(ExpenseAccountEVM ea)
        {
            if (ea == null)
                return;

            _fdlManager.Print(ea);
        }

        public void SaveAs(ExpenseAccountEVM ea)
        {
            if (ea == null)
                return;

            using (new WaitCursor())
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Title = "Save Expense Account As...";
                dlg.FileName = ea.FileName;
                dlg.DefaultExt = ea.IsExcel ? ".xlsx" : ".pdf";
                dlg.Filter = ea.IsExcel ? "EA (.xlsx) | *.xlsx" : "EA (.pdf) | *.pdf";
                dlg.AddExtension = true;
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (dlg.ShowDialog() == true)
                {
                    if (ea.IsExcel)
                        File.Copy(ea.FilePath, dlg.FileName, true);
                    else
                        _fdlManager.SaveAs(ea, dlg.FileName);
                }   
            }
        }

        public void Compile(ExpenseAccountEVM ea)
        {
            List<DateTime> timesheetDates = new List<DateTime>();

            using (DBArchive db = new DBArchive())
            {
                db.Timesheets.Where(x => x.FDL == ea.FDL).ToList().ForEach(x => timesheetDates.Add(DateTime.Now.FromUnixTimestamp(x.Timestamp)));
            }

            bool showWarning = false;

            foreach (var exp in ea.Expenses)
            {
                showWarning = exp.MondayAmount > 0 && !timesheetDates.Any(d => d.DayOfWeek == DayOfWeek.Monday);
                showWarning |= exp.TuesdayAmount > 0 && !timesheetDates.Any(d => d.DayOfWeek == DayOfWeek.Tuesday);
                showWarning |= exp.WednesdayAmount > 0 && !timesheetDates.Any(d => d.DayOfWeek == DayOfWeek.Wednesday);
                showWarning |= exp.ThursdayAmount > 0 && !timesheetDates.Any(d => d.DayOfWeek == DayOfWeek.Thursday);
                showWarning |= exp.FridayAmount > 0 && !timesheetDates.Any(d => d.DayOfWeek == DayOfWeek.Friday);
                showWarning |= exp.SaturdayAmount > 0 && !timesheetDates.Any(d => d.DayOfWeek == DayOfWeek.Saturday);
                showWarning |= exp.SundayAmount > 0 && !timesheetDates.Any(d => d.DayOfWeek == DayOfWeek.Sunday);

                if (showWarning)
                {
                    if (MetroMessageBox.Show("Some expenses are referencing days without fdl connected. Are you sure?", "Compile", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        return;
                }
            }

            if (ea == null)
                return;

            using (new WaitCursor())
            {
                string filePath;

                if(ea.IsExcel)
                    _fdlManager.Compile(ea, ea.FilePath);
                else if (_fdlManager.CreateXFDF(ea, out filePath))
                    Process.Start(filePath);

                ea.IsCompiled = true;
                ea.NotifyAsNew = false;
                ea.Save();
            }
        }

        public void Open(ExpenseAccountEVM ea)
        {
            if (ea == null)
                return;

            Process.Start(ea.FilePath);
        }

        public void MarkAsRefunded(ExpenseAccountEVM ea)
        {
            if (ea.EStatus != EFDLStatus.Accepted)
            {
                MetroMessageBox.Show("Only accepted expense accounts can be marked as refounded.\nOperation cancelled!");
                return;
            }

            if (MetroMessageBox.Show("Are you sure to change the \"Refunded\" status of the selected expense account?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;


            ea.IsRefunded = !ea.IsRefunded;
            ea.NotifyAsNew = false;
            ea.Save();
        }

        public void MarkAsAccepted(ExpenseAccountEVM ea)
        {
            if (MetroMessageBox.Show("Are you sure to mark as \"Accepted\" the selected expense account?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            ea.EStatus = EFDLStatus.Accepted;
            ea.NotifyAsNew = false;
            ea.Save();
        }

        public void MarkAsCancelled(ExpenseAccountEVM ea)
        {
            if (MetroMessageBox.Show("Are you sure to mark as \"Cancelled\" the selected expense account?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            ea.EStatus = EFDLStatus.Cancelled;
            ea.NotifyAsNew = false;
            ea.Save();
        }

        private void FactoryLink()
        {
            if (SelectedEA.FDL1.Factory.HasValue)
                OnFactoryLink?.Invoke(SelectedEA.FDL1.Factory.Value);
        }

        private void UpdateEaList()
        {
            ExpenseAccounts.Clear();

            using (DBArchive db = new DBArchive())
            {
                db.ExpenseAccounts.SqlQuery(
                    $"select * from ExpenseAccount where SUBSTR(FDL, 1, 4) = '{CurrentYear}' or (CAST(SUBSTR(FDL, 1, 4) as int) < {CurrentYear} and Status == 0)")
                    .ToList().ForEach(ea => ExpenseAccounts.Add(new ExpenseAccountEVM(ea)));
            }
        }
    }
}
