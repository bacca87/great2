using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Great.Models;
using Great.Models.Database;
using Great.Utils;
using Great.Utils.Messages;
using Great.Views.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Great.ViewModels
{
    public class FDLViewModel : ViewModelBase
    {
        #region Properties
        public int PerfDescMaxLength { get { return ApplicationSettings.FDL.PerformanceDescriptionMaxLength; } }
        public int FinalTestResultMaxLength { get { return ApplicationSettings.FDL.FinalTestResultMaxLength; } }
        public int OtherNotesMaxLength { get { return ApplicationSettings.FDL.OtherNotesMaxLength; } }
        public int PerfDescDetMaxLength { get { return ApplicationSettings.FDL.PerformanceDescriptionDetailsMaxLength; } }

        private FDLManager _fdlManager;
        private DBArchive _db;

        /// <summary>
        /// The <see cref="IsInputEnabled" /> property's name.
        /// </summary>
        private bool _isInputEnabled = false;

        /// <summary>
        /// Sets and gets the IsInputEnabled property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public bool IsInputEnabled
        {
            get
            {
                return _isInputEnabled;
            }

            set
            {
                if (_isInputEnabled == value)
                {
                    return;
                }

                var oldValue = _isInputEnabled;
                _isInputEnabled = value;

                RaisePropertyChanged(nameof(IsInputEnabled), oldValue, value);
                SaveCommand.RaiseCanExecuteChanged();
                ClearCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The <see cref="FDLs" /> property's name.
        /// </summary>
        private ObservableCollectionEx<FDL> _FDLs;

        /// <summary>
        /// Sets and gets the FDLs property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>        
        public ObservableCollectionEx<FDL> FDLs
        {
            get
            {
                return _FDLs;
            }
            set
            {
                _FDLs = value;
                RaisePropertyChanged(nameof(FDLs), true);
            }
        }

        /// <summary>
        /// The <see cref="SelectedFDL" /> property's name.
        /// </summary>
        private FDL _selectedFDL;

        /// <summary>
        /// Sets and gets the SelectedFDL property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public FDL SelectedFDL
        {
            get
            {
                return _selectedFDL;
            }

            set
            {
                var oldValue = _selectedFDL;
                _selectedFDL = value;

                RefreshTimesheets();

                SelectedFDLClone = _selectedFDL?.Clone();

                if (_selectedFDL != null)
                {   
                    SelectedTimesheet = null;
                    IsInputEnabled = true;
                }
                else
                    IsInputEnabled = false;                    
                
                RaisePropertyChanged(nameof(SelectedFDL), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="SelectedFDLClone" /> property's name.
        /// </summary>
        private FDL _selectedFDLClone;

        /// <summary>
        /// Sets and gets the SelectedFDLClone property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public FDL SelectedFDLClone
        {
            get
            {
                return _selectedFDLClone;
            }

            set
            {
                var oldValue = _selectedFDLClone;
                _selectedFDLClone = value;
                RaisePropertyChanged(nameof(SelectedFDLClone), oldValue, value);                
            }
        }

        /// <summary>
        /// The <see cref="SelectedTimesheet" /> property's name.
        /// </summary>
        private Timesheet _selectedTimesheet;

        /// <summary>
        /// Sets and gets the SelectedTimesheet property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public Timesheet SelectedTimesheet
        {
            get
            {
                return _selectedTimesheet;
            }

            set
            {
                var oldValue = _selectedTimesheet;
                _selectedTimesheet = value;
                
                RaisePropertyChanged(nameof(SelectedTimesheet), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="Timesheets" /> property's name.
        /// </summary>
        private IList<Timesheet> _timesheets;

        /// <summary>
        /// Sets and gets the Timesheets property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public IList<Timesheet> Timesheets
        {
            get
            {
                return _timesheets;
            }
            internal set
            {
                _timesheets = value;
                RaisePropertyChanged(nameof(Timesheets));
            }
        }

        /// <summary>
        /// The <see cref="FDLResults" /> property's name.
        /// </summary>
        public ObservableCollection<FDLResult> FDLResults { get; set; }

        /// <summary>
        /// The <see cref="MRUEmailRecipients" /> property's name.
        /// </summary>
        public MRUCollection<string> MRUEmailRecipients { get; set; }

        /// <summary>
        /// The <see cref="SendToEmailRecipient" /> property's name.
        /// </summary>
        private string _sendToEmailRecipient;

        /// <summary>
        /// Sets and gets the SendToEmailRecipient property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public string SendToEmailRecipient
        {
            get
            {
                return _sendToEmailRecipient;
            }

            set
            {
                var oldValue = _sendToEmailRecipient;
                _sendToEmailRecipient = value;
                RaisePropertyChanged(nameof(SendToEmailRecipient), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="Factories" /> property's name.
        /// </summary>
        private ObservableCollectionEx<Factory> _factories;

        /// <summary>
        /// Sets and gets the Factories property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public ObservableCollectionEx<Factory> Factories
        {
            get
            {
                return _factories;
            }
            set
            {
                _factories = value;
                RaisePropertyChanged(nameof(Factories));
            }
        }

        /// <summary>
        /// Sets and gets the OnFactoryLink Action.
        /// </summary>
        public Action<long> OnFactoryLink;
        #endregion

        #region Commands Definitions
        public RelayCommand ClearCommand { get; set; }
        public RelayCommand<FDL> SaveCommand { get; set; }

        public RelayCommand<FDL> SendToSAPCommand { get; set; }
        public RelayCommand<string> SendByEmailCommand { get; set; }
        public RelayCommand<FDL> SaveAsCommand { get; set; }
        public RelayCommand<FDL> OpenCommand { get; set; }
        public RelayCommand<FDL> MarkAsAcceptedCommand { get; set; }
        public RelayCommand<FDL> MarkAsCancelledCommand { get; set; }
        public RelayCommand<FDL> SendCancellationRequestCommand { get; set; }

        public RelayCommand FactoryLinkCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the EmailViewModel class.
        /// </summary>
        public FDLViewModel(FDLManager manager, DBArchive db)
        {
            _db = db;
            _fdlManager = manager;

            ClearCommand = new RelayCommand(ClearFDL, () => { return IsInputEnabled; });
            SaveCommand = new RelayCommand<FDL>(SaveFDL, (FDL fdl) => { return IsInputEnabled; });

            SendToSAPCommand = new RelayCommand<FDL>(SendToSAP);
            SendByEmailCommand = new RelayCommand<string>(SendByEmail);
            SaveAsCommand = new RelayCommand<FDL>(SaveAs);
            OpenCommand = new RelayCommand<FDL>(Open);
            MarkAsAcceptedCommand = new RelayCommand<FDL>(MarkAsAccepted);
            MarkAsCancelledCommand = new RelayCommand<FDL>(MarkAsCancelled);
            SendCancellationRequestCommand = new RelayCommand<FDL>(CancellationRequest);

            FactoryLinkCommand = new RelayCommand(FactoryLink);
                        
            Factories = new ObservableCollectionEx<Factory>(_db.Factories.ToList());
            FDLResults = new ObservableCollection<FDLResult>(_db.FDLResults);
            FDLs = new ObservableCollectionEx<FDL>(_db.FDLs);            

            FDLs.ItemPropertyChanged += FDLs_ItemPropertyChanged;
            
            MessengerInstance.Register<NewItemMessage<FDL>>(this, NewFDL);
            MessengerInstance.Register<ItemChangedMessage<FDL>>(this, FDLChanged);
            MessengerInstance.Register(this, (PropertyChangedMessage<ObservableCollectionEx<Factory>> p) => { Factories = p.NewValue; });

            List<string> recipients = UserSettings.Email.Recipients.MRU?.Cast<string>().ToList();

            if (recipients != null)
                MRUEmailRecipients = new MRUCollection<string>(ApplicationSettings.EmailRecipients.MRUSize, new Collection<string>(recipients));
            else
                MRUEmailRecipients = new MRUCollection<string>(ApplicationSettings.EmailRecipients.MRUSize);
        }

        private void FDLs_ItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {   
            if (SelectedFDL != null && SelectedFDL.Id == FDLs[e.CollectionIndex].Id)
                RefreshTimesheets();
        }

        private void RefreshTimesheets()
        {
            if (SelectedFDL != null && SelectedFDL.Timesheets != null)
                Timesheets = SelectedFDL.Timesheets.OrderBy(t => t.Date).ToList();
            else
                Timesheets = null;
        }
        
        public void NewFDL(NewItemMessage<FDL> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background, 
                new Action(() => 
                {
                    if (item.Content != null)
                    {
                        FDL fdl = _db.FDLs.SingleOrDefault(f => f.Id == item.Content.Id);

                        if (fdl != null && !FDLs.Contains(fdl))
                            FDLs.Add(fdl);
                    }
                })
            );
        }

        public void FDLChanged(ItemChangedMessage<FDL> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background, 
                new Action(() => 
                {
                    if (item.Content != null)
                    {
                        _db.FDLs.AddOrUpdate(item.Content);
                        _db.SaveChanges();

                        FDLs.SingleOrDefault(f => f.Id == item.Content.Id)?.NotifyFDLPropertiesChanged();
                    }
                })
            );
        }

        public void SendToSAP(FDL fdl)
        {
            if (fdl.EStatus == EFDLStatus.Waiting && 
                MessageBox.Show("The selected FDL was already sent. Do you want send it again?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            if (_db.OrderEmailRecipients.Count(r => r.Order == fdl.Order) == 0)
            {
                OrderRecipientsViewModel recipientsVM = SimpleIoc.Default.GetInstance<OrderRecipientsViewModel>();
                OrderRecipientsView recipientsView = new OrderRecipientsView();

                recipientsVM.Order = fdl.Order;                
                recipientsView.ShowDialog();
            }

            _fdlManager.SendToSAP(fdl);
            _db.SaveChanges();
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

            _fdlManager.SendTo(address, SelectedFDL);
        }

        public void SaveAs(FDL fdl)
        {
            if (fdl == null)
                return;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save FDL As...";
            dlg.FileName = fdl.FileName;
            dlg.DefaultExt = ".pdf";
            dlg.Filter = "FDL (.pdf) | *.pdf";
            dlg.AddExtension = true;
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            
            if (dlg.ShowDialog() == true)
                _fdlManager.SaveAs(fdl, dlg.FileName);
        }

        public void Open(FDL fdl)
        {            
            if (fdl == null)
                return;

            string fileName = Path.GetTempPath() + Path.GetFileNameWithoutExtension(fdl.FileName) + ".XFDF";

            _fdlManager.SaveXFDF(fdl, fileName);
            Process.Start(fileName);
        }

        public void MarkAsAccepted(FDL fdl)
        {
            if (MessageBox.Show("Are you sure to mark as accepted the selected FDL?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            fdl.EStatus = EFDLStatus.Accepted;
            _db.SaveChanges();

            fdl.NotifyFDLPropertiesChanged();
        }

        public void MarkAsCancelled(FDL fdl)
        {
            if (MessageBox.Show("Are you sure to mark as Cancelled the selected FDL?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            fdl.EStatus = EFDLStatus.Cancelled;
            _db.SaveChanges();

            fdl.NotifyFDLPropertiesChanged();
        }

        public void CancellationRequest(FDL fdl)
        {
            if (MessageBox.Show("Are you sure to send a cancellation request for the selected FDL?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            _fdlManager.SendCancellationRequest(fdl);
            _db.SaveChanges();

            fdl.NotifyFDLPropertiesChanged();
        }

        private void FactoryLink()
        {
            if(SelectedFDL.Factory.HasValue)
                OnFactoryLink?.Invoke(SelectedFDL.Factory.Value);
        }

        public void ClearFDL()
        {
            SelectedFDLClone.Factory = -1;
            SelectedFDLClone.OutwardCar = false;
            SelectedFDLClone.OutwardTaxi = false;
            SelectedFDLClone.OutwardAircraft = false;
            SelectedFDLClone.ReturnCar = false;
            SelectedFDLClone.ReturnTaxi = false;
            SelectedFDLClone.ReturnAircraft = false;
            SelectedFDLClone.PerformanceDescription = string.Empty;
            SelectedFDLClone.Result = 0;
            SelectedFDLClone.ResultNotes = string.Empty;
            SelectedFDLClone.Notes = string.Empty;
            SelectedFDLClone.PerformanceDescriptionDetails = string.Empty;
            SelectedFDLClone = SelectedFDLClone; // Raise PropertyChanged
        }

        public void SaveFDL(FDL fdl)
        {
            if (fdl == null)
                return;

            if (fdl.Factory == null || fdl.Factory == -1)
            {
                MessageBox.Show("Please select a factory before continue. Operation cancelled.", "Invalid FDL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            _db.FDLs.AddOrUpdate(fdl);

            if (_db.SaveChanges() > 0)
                SelectedFDL?.NotifyFDLPropertiesChanged();
        }
    }
}