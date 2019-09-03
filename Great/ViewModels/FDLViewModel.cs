using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Great.Models;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Utils;
using Great.Utils.Messages;
using Great.ViewModels.Database;
using Great.Views.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Great.ViewModels
{
    public class FDLViewModel : ViewModelBase
    {
        #region Properties
        public int PerfDescMaxLength => ApplicationSettings.FDL.PerformanceDescriptionMaxLength;
        public int FinalTestResultMaxLength => ApplicationSettings.FDL.FinalTestResultMaxLength;
        public int OtherNotesMaxLength => ApplicationSettings.FDL.OtherNotesMaxLength;
        public int PerfDescDetMaxLength => ApplicationSettings.FDL.PerformanceDescriptionDetailsMaxLength;

        private FDLManager _fdlManager;

        private bool _isInputEnabled = false;
        public bool IsInputEnabled
        {
            get => _isInputEnabled;
            set
            {
                Set(ref _isInputEnabled, value);
                SaveCommand.RaiseCanExecuteChanged();
                ClearCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _showEditMenu;
        public bool ShowEditMenu
        {
            get => _showEditMenu;
            set => Set(ref _showEditMenu, value);
        }

        private ObservableCollectionEx<FDLEVM> _FDLs;
        public ObservableCollectionEx<FDLEVM> FDLs
        {
            get => _FDLs;
            set => Set(ref _FDLs, value);
        }

        private FDLEVM _selectedFDL;
        public FDLEVM SelectedFDL
        {
            get => _selectedFDL;
            set
            {

                Set(ref _selectedFDL, value);

                if (_selectedFDL != null)
                {
                    SelectedTimesheet = null;
                    IsInputEnabled = true;
                }
                else
                    IsInputEnabled = false;

                ShowEditMenu = false;
            }
        }

        private TimesheetDTO _selectedTimesheet;
        public TimesheetDTO SelectedTimesheet
        {
            get => _selectedTimesheet;
            set => Set(ref _selectedTimesheet, value);
        }

        public ObservableCollection<FDLResultDTO> FDLResults { get; set; }
        public MRUCollection<string> MRUEmailRecipients { get; set; }

        private string _sendToEmailRecipient;
        public string SendToEmailRecipient
        {
            get => _sendToEmailRecipient;
            set => Set(ref _sendToEmailRecipient, value);
        }

        private ObservableCollection<FactoryDTO> _factories;
        public ObservableCollection<FactoryDTO> Factories
        {
            get => _factories;
            set => Set(ref _factories, value);
        }

        public Action<long> OnFactoryLink { get; set; }
        #endregion

        #region Commands Definitions
        public RelayCommand ClearCommand { get; set; }
        public RelayCommand<FDLEVM> SaveCommand { get; set; }

        public RelayCommand<FDLEVM> SendToSAPCommand { get; set; }
        public RelayCommand<FDLEVM> CompileCommand { get; set; }
        public RelayCommand<string> SendByEmailCommand { get; set; }
        public RelayCommand<FDLEVM> SaveAsCommand { get; set; }
        public RelayCommand<FDLEVM> OpenCommand { get; set; }
        public RelayCommand<FDLEVM> MarkAsAcceptedCommand { get; set; }
        public RelayCommand<FDLEVM> MarkAsCancelledCommand { get; set; }
        public RelayCommand<FDLEVM> SendCancellationRequestCommand { get; set; }

        public RelayCommand GotFocusCommand { get; set; }
        public RelayCommand LostFocusCommand { get; set; }

        public RelayCommand PageLoadedCommand { get; set; }
        public RelayCommand PageUnloadedCommand { get; set; }

        public RelayCommand FactoryLinkCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the EmailViewModel class.
        /// </summary>
        public FDLViewModel(FDLManager manager)
        {
            _fdlManager = manager;

            ClearCommand = new RelayCommand(ClearFDL, () => { return IsInputEnabled; });
            SaveCommand = new RelayCommand<FDLEVM>(SaveFDL, (FDLEVM fdl) => { return IsInputEnabled; });

            SendToSAPCommand = new RelayCommand<FDLEVM>(SendToSAP);
            CompileCommand = new RelayCommand<FDLEVM>(Compile);
            SendByEmailCommand = new RelayCommand<string>(SendByEmail);
            SaveAsCommand = new RelayCommand<FDLEVM>(SaveAs);
            OpenCommand = new RelayCommand<FDLEVM>(Open);
            MarkAsAcceptedCommand = new RelayCommand<FDLEVM>(MarkAsAccepted);
            MarkAsCancelledCommand = new RelayCommand<FDLEVM>(MarkAsCancelled);
            SendCancellationRequestCommand = new RelayCommand<FDLEVM>(CancellationRequest);

            GotFocusCommand = new RelayCommand(() => { ShowEditMenu = true; });
            LostFocusCommand = new RelayCommand(() => { });
            PageLoadedCommand = new RelayCommand(PageLoaded);
            PageUnloadedCommand = new RelayCommand(PageUnloaded);

            FactoryLinkCommand = new RelayCommand(FactoryLink);

            using (DBArchive db = new DBArchive())
            {
                Factories = new ObservableCollection<FactoryDTO>(db.Factories.ToList().Select(f => new FactoryDTO(f)));
                FDLResults = new ObservableCollection<FDLResultDTO>(db.FDLResults.ToList().Select(r => new FDLResultDTO(r)));
                FDLs = new ObservableCollectionEx<FDLEVM>(db.FDLs.ToList().Select(fdl => new FDLEVM(fdl)));
            }

            MessengerInstance.Register<NewItemMessage<FDLEVM>>(this, NewFDL);
            MessengerInstance.Register<ItemChangedMessage<FDLEVM>>(this, FDLChanged);
            MessengerInstance.Register<ItemChangedMessage<TimesheetEVM>>(this, TimeSheetChanged);
            MessengerInstance.Register<DeletedItemMessage<TimesheetEVM>>(this, TimeSheetDeleted);

            MessengerInstance.Register<NewItemMessage<FactoryEVM>>(this, NewFactory);
            MessengerInstance.Register<ItemChangedMessage<FactoryEVM>>(this, FactoryChanged);
            MessengerInstance.Register<DeletedItemMessage<FactoryEVM>>(this, FactoryDeleted);

            List<string> recipients = UserSettings.Email.Recipients.MRU?.Cast<string>().ToList();

            if (recipients != null)
                MRUEmailRecipients = new MRUCollection<string>(ApplicationSettings.EmailRecipients.MRUSize, new Collection<string>(recipients));
            else
                MRUEmailRecipients = new MRUCollection<string>(ApplicationSettings.EmailRecipients.MRUSize);
        }

        private void PageUnloaded()
        {
            var changed = FDLs.Where(x => x.IsChanged);
            if (changed.Count() == 0) return;


            if (MetroMessageBox.Show("You changed page without saving. Do you want to commit changes?", "Save Items", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                changed.ToList().ForEach(x => x.Save());

            else
                changed.ToList().ForEach(x => x.RejectChanges());

        }

        private void PageLoaded()
        {
            FDLs.ToList().ForEach(x => x.IsChanged =false);
        }

        public void NewFDL(NewItemMessage<FDLEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null && !FDLs.Any(f => f.Id == item.Content.Id))
                        FDLs.Add(item.Content);
                })
            );
        }

        public void FDLChanged(ItemChangedMessage<FDLEVM> item)
        {
            if (item.Sender == this)
                return;

            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        FDLEVM fdl = FDLs.SingleOrDefault(x => x.Id == item.Content.Id);

                        if (fdl != null)
                        {
                            fdl.Status = item.Content.Status;
                            fdl.LastError = item.Content.LastError;
                        }
                    }
                })
            );
        }

        public void TimeSheetChanged(ItemChangedMessage<TimesheetEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        FDLEVM fdl = FDLs.SingleOrDefault(x => x.Id == item.Content.FDL1?.Id);

                        if (fdl != null)
                        {
                            var ts = fdl.Timesheets.Where(x => x.Timestamp == item.Content.Timestamp).FirstOrDefault();

                            if (ts != null)
                                ts = item.Content;
                            else
                                fdl.Timesheets.Add(item.Content);

                            fdl.IsCompiled = false;
                            fdl.Save();
                        }

                    }
                })
            );
        }

        public void TimeSheetDeleted(DeletedItemMessage<TimesheetEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        FDLEVM fdl = FDLs.SingleOrDefault(x => x.Id == item.Content.FDL1?.Id);

                        if (fdl != null)
                        {
                            var ts = fdl.Timesheets.Where(x => x.Timestamp == item.Content.Timestamp).FirstOrDefault();
                            fdl.Timesheets.Remove(ts);

                            fdl.IsCompiled = false;
                            fdl.Save();
                        }
                    }
                })
            );
        }

        public void NewFactory(NewItemMessage<FactoryEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null && !Factories.Any(f => f.Id == item.Content.Id))
                    {
                        FactoryDTO factory = new FactoryDTO();
                        Global.Mapper.Map(item.Content, factory);
                        Factories.Add(factory);
                    }
                })
            );
        }

        public void FactoryChanged(ItemChangedMessage<FactoryEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        FactoryDTO factory = Factories.SingleOrDefault(f => f.Id == item.Content.Id);

                        if (factory != null)
                            Global.Mapper.Map(item.Content, factory);

                        var fdlToUpdate = FDLs.Where(f => f.Factory.HasValue && f.Factory.Value == item.Content.Id);

                        foreach (var fdl in fdlToUpdate)
                            fdl.Factory1 = factory;
                    }
                })
            );
        }

        public void FactoryDeleted(DeletedItemMessage<FactoryEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        FactoryDTO factory = Factories.SingleOrDefault(f => f.Id == item.Content.Id);

                        if (factory != null)
                            Factories.Remove(factory);
                    }
                })
            );
        }

        public void SendToSAP(FDLEVM fdl)
        {
            if (!fdl.IsCompiled)
            {
                MetroMessageBox.Show("The selected FDL is not compiled! Compile the FDL before send it to SAP. Operation cancelled!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_fdlManager.IsExchangeAvailable())
            {
                MetroMessageBox.Show("The email server is not reachable, please check your connection. Operation cancelled!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (fdl.EStatus == EFDLStatus.Waiting &&
                MetroMessageBox.Show("The selected FDL was already sent. Do you want send it again?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            using (DBArchive db = new DBArchive())
            {
                if (db.OrderEmailRecipients.Count(r => r.Order == fdl.Order) == 0)
                {
                    OrderRecipientsViewModel recipientsVM = SimpleIoc.Default.GetInstance<OrderRecipientsViewModel>();
                    OrderRecipientsView recipientsView = new OrderRecipientsView();

                    recipientsVM.Order = fdl.Order;
                    recipientsView.ShowDialog();
                }
            }

            using (new WaitCursor())
            {
                _fdlManager.SendToSAP(fdl);
            }
        }

        public void SendByEmail(string address)
        {
            string error;

            if (!SelectedFDL.IsCompiled)
            {
                MetroMessageBox.Show("The selected FDL is not compiled! Compile the FDL before send it by e-mail. Operation cancelled!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                _fdlManager.SendTo(address, SelectedFDL);
            }
        }

        public void SaveAs(FDLEVM fdl)
        {
            if (fdl == null)
                return;

            using (new WaitCursor())
            {
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
        }

        public void Compile(FDLEVM fdl)
        {
            if (fdl == null)
                return;

            using (new WaitCursor())
            {
                string filePath;

                if (_fdlManager.CreateXFDF(fdl, out filePath))
                {
                    Process.Start(filePath);
                    fdl.IsCompiled = true;
                    fdl.NotifyAsNew = false;
                    fdl.Save();
                }
            }
        }

        public void Open(FDLEVM fdl)
        {
            if (fdl == null)
                return;

            Process.Start(fdl.FilePath);
        }

        public void MarkAsAccepted(FDLEVM fdl)
        {
            if (MetroMessageBox.Show("Are you sure to mark as accepted the selected FDL?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            fdl.EStatus = EFDLStatus.Accepted;
            fdl.NotifyAsNew = false;
            fdl.Save();
        }

        public void MarkAsCancelled(FDLEVM fdl)
        {
            if (MetroMessageBox.Show("Are you sure to mark as Cancelled the selected FDL?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            fdl.EStatus = EFDLStatus.Cancelled;
            fdl.NotifyAsNew = false;
            fdl.Save();
        }

        public void CancellationRequest(FDLEVM fdl)
        {
            if (!_fdlManager.IsExchangeAvailable())
            {
                MetroMessageBox.Show("The email server is not reachable, please check your connection. Operation cancelled!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MetroMessageBox.Show("Are you sure to send a cancellation request for the selected FDL?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            using (new WaitCursor())
            {
                _fdlManager.SendCancellationRequest(fdl);
            }
        }

        private void FactoryLink()
        {
            if (SelectedFDL.Factory.HasValue)
                OnFactoryLink?.Invoke(SelectedFDL.Factory.Value);
        }

        public void ClearFDL()
        {
            SelectedFDL.Factory = -1;
            SelectedFDL.OutwardCar = false;
            SelectedFDL.OutwardTaxi = false;
            SelectedFDL.OutwardAircraft = false;
            SelectedFDL.ReturnCar = false;
            SelectedFDL.ReturnTaxi = false;
            SelectedFDL.ReturnAircraft = false;
            SelectedFDL.PerformanceDescription = string.Empty;
            SelectedFDL.Result = 0;
            SelectedFDL.ResultNotes = string.Empty;
            SelectedFDL.Notes = string.Empty;
            SelectedFDL.PerformanceDescriptionDetails = string.Empty;
        }

        public void SaveFDL(FDLEVM fdl)
        {
            if (fdl == null || fdl.IsReadOnly)
                return;

            if (fdl.Factory == null || fdl.Factory == -1)
            {
                MessageBox.Show("Please select a factory before continue. Operation cancelled.", "Invalid FDL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            fdl.IsCompiled = false;
            fdl.NotifyAsNew = false;
            fdl.Save();

            // update timesheets and notifications
            Messenger.Default.Send(new ItemChangedMessage<FDLEVM>(this, fdl));
        }
    }
}