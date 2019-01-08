using AutoMapper;
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
        public RelayCommand<string> SendByEmailCommand { get; set; }
        public RelayCommand<FDLEVM> SaveAsCommand { get; set; }
        public RelayCommand<FDLEVM> OpenCommand { get; set; }
        public RelayCommand<FDLEVM> MarkAsAcceptedCommand { get; set; }
        public RelayCommand<FDLEVM> MarkAsCancelledCommand { get; set; }
        public RelayCommand<FDLEVM> SendCancellationRequestCommand { get; set; }

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
            SendByEmailCommand = new RelayCommand<string>(SendByEmail);
            SaveAsCommand = new RelayCommand<FDLEVM>(SaveAs);
            OpenCommand = new RelayCommand<FDLEVM>(Open);
            MarkAsAcceptedCommand = new RelayCommand<FDLEVM>(MarkAsAccepted);
            MarkAsCancelledCommand = new RelayCommand<FDLEVM>(MarkAsCancelled);
            SendCancellationRequestCommand = new RelayCommand<FDLEVM>(CancellationRequest);

            FactoryLinkCommand = new RelayCommand(FactoryLink);

            using (DBArchive db = new DBArchive())
            {
                Factories = new ObservableCollection<FactoryDTO>(db.Factories.ToList().Select(f => new FactoryDTO(f)));
                FDLResults = new ObservableCollection<FDLResultDTO>(db.FDLResults.ToList().Select(r => new FDLResultDTO(r)));
                FDLs = new ObservableCollectionEx<FDLEVM>(db.FDLs.ToList().Select(fdl => new FDLEVM(fdl)));
            }
                        
            MessengerInstance.Register<NewItemMessage<FDLEVM>>(this, NewFDL);
            MessengerInstance.Register<ItemChangedMessage<FDLEVM>>(this, FDLChanged);

            MessengerInstance.Register<NewItemMessage<FactoryEVM>>(this, NewFactory);
            MessengerInstance.Register<ItemChangedMessage<FactoryEVM>>(this, FactoryChanged);
            MessengerInstance.Register<DeletedItemMessage<FactoryEVM>>(this, FactoryDeleted);

            List<string> recipients = UserSettings.Email.Recipients.MRU?.Cast<string>().ToList();

            if (recipients != null)
                MRUEmailRecipients = new MRUCollection<string>(ApplicationSettings.EmailRecipients.MRUSize, new Collection<string>(recipients));
            else
                MRUEmailRecipients = new MRUCollection<string>(ApplicationSettings.EmailRecipients.MRUSize);
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

        public void NewFactory(NewItemMessage<FactoryEVM> item)
        {
            if (!(item.Sender is FactoriesViewModel))
                return;

            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null && !Factories.Any(f => f.Id == item.Content.Id))
                    {
                        FactoryDTO factory = new FactoryDTO();
                        Mapper.Map(item.Content, factory);
                        Factories.Add(factory);
                    }
                })
            );
        }

        public void FactoryChanged(ItemChangedMessage<FactoryEVM> item)
        {
            if (!(item.Sender is FactoriesViewModel))
                return;

            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        FactoryDTO factory = Factories.SingleOrDefault(f => f.Id == item.Content.Id);

                        if (factory != null)
                            Mapper.Map(item.Content, factory);
                    }
                })
            );
        }

        public void FactoryDeleted(DeletedItemMessage<FactoryEVM> item)
        {
            if (!(item.Sender is FactoriesViewModel))
                return;

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
            if (fdl.EStatus == EFDLStatus.Waiting && 
                MessageBox.Show("The selected FDL was already sent. Do you want send it again?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
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

            _fdlManager.SendToSAP(fdl);
            fdl.Save();
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

        public void SaveAs(FDLEVM fdl)
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

        public void Open(FDLEVM fdl)
        {            
            if (fdl == null)
                return;

            string fileName = Path.GetTempPath() + Path.GetFileNameWithoutExtension(fdl.FileName) + ".XFDF";

            _fdlManager.SaveXFDF(fdl, fileName);
            Process.Start(fileName);
        }

        public void MarkAsAccepted(FDLEVM fdl)
        {
            if (MessageBox.Show("Are you sure to mark as accepted the selected FDL?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            fdl.EStatus = EFDLStatus.Accepted;
            fdl.Save();
        }

        public void MarkAsCancelled(FDLEVM fdl)
        {
            if (MessageBox.Show("Are you sure to mark as Cancelled the selected FDL?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            fdl.EStatus = EFDLStatus.Cancelled;
            fdl.Save();
        }

        public void CancellationRequest(FDLEVM fdl)
        {
            if (MessageBox.Show("Are you sure to send a cancellation request for the selected FDL?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            _fdlManager.SendCancellationRequest(fdl);
            fdl.Save();
        }

        private void FactoryLink()
        {
            if(SelectedFDL.Factory.HasValue)
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
            if (fdl == null)
                return;

            if (fdl.Factory == null || fdl.Factory == -1)
            {
                MessageBox.Show("Please select a factory before continue. Operation cancelled.", "Invalid FDL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            fdl.Save();
        }
    }
}