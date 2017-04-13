using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Great.Models;
using Great.Utils;
using Great.Utils.Messages;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Migrations;
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
        private DBEntities _db;

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
                SaveFDLCommand.RaiseCanExecuteChanged();
                ClearFDLCommand.RaiseCanExecuteChanged();
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
        public RelayCommand ClearFDLCommand { get; set; }
        public RelayCommand<FDL> SaveFDLCommand { get; set; }

        public RelayCommand<FDL> SendToSAPCommand { get; set; }
        public RelayCommand<FDL> SendByEmailCommand { get; set; }
        public RelayCommand<FDL> SaveAsCommand { get; set; }
        public RelayCommand<FDL> OpenCommand { get; set; }
        
        public RelayCommand FactoryLinkCommand { get; set; }        
        #endregion

        /// <summary>
        /// Initializes a new instance of the EmailViewModel class.
        /// </summary>
        public FDLViewModel(FDLManager manager, DBEntities db)
        {
            _db = db;
            _fdlManager = manager;

            //FDLs = new ObservableCollectionEx<FDL>(_db.FDLs.OrderBy(f => f.Status).ThenByDescending(f => f.Id));
            FDLs = new ObservableCollectionEx<FDL>(_db.FDLs);
            FDLResults = new ObservableCollection<FDLResult>(_db.FDLResults);
            Factories = new ObservableCollectionEx<Factory>(_db.Factories.ToList());

            FDLs.ItemPropertyChanged += FDLs_ItemPropertyChanged;

            ClearFDLCommand = new RelayCommand(ClearFDL, () => { return IsInputEnabled; });
            SaveFDLCommand = new RelayCommand<FDL>(SaveFDL, (FDL fdl) => { return IsInputEnabled; });
            
            SendToSAPCommand = new RelayCommand<FDL>(SendToSAP);
            SendByEmailCommand = new RelayCommand<FDL>(SendByEmail);
            SaveAsCommand = new RelayCommand<FDL>(SaveAs);
            OpenCommand = new RelayCommand<FDL>(Open);            

            FactoryLinkCommand = new RelayCommand(FactoryLink);

            MessengerInstance.Register<NewItemMessage<FDL>>(this, NewFDL);
            MessengerInstance.Register<ItemChangedMessage<FDL>>(this, FDLChanged);
            MessengerInstance.Register(this, (PropertyChangedMessage<ObservableCollectionEx<Factory>> p) => { Factories = p.NewValue; });
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

            _fdlManager.SendFDL(fdl);

            fdl.EStatus = EFDLStatus.Waiting;
            _db.SaveChanges();
        }
        
        public void SendByEmail(FDL fdl)
        {
            //TODO
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
                _fdlManager.SaveFDL(fdl, dlg.FileName);
        }

        public void Open(FDL fdl)
        {            
            if (fdl == null)
                return;

            string fileName = Path.GetTempPath() + Path.GetFileNameWithoutExtension(fdl.FileName) + ".FDF";

            _fdlManager.SaveFDF(fdl, fileName);

            System.Diagnostics.Process.Start(fileName);
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
                SelectedFDL.NotifyFDLPropertiesChanged();
        }
    }
}