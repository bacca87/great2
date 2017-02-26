using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Windows;

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
        private BindingList<FDL> _FDLs;

        /// <summary>
        /// Sets and gets the FDLs property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>        
        public BindingList<FDL> FDLs
        {
            get
            {
                return _FDLs;
            }
            internal set
            {
                _FDLs = value;
                RaisePropertyChanged(nameof(FDLs));
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

                if (_selectedFDL != null)
                {
                    Timesheets = _db.FDLs.SingleOrDefault(f => f.Id == _selectedFDL.Id).Timesheets.ToList();
                    SelectedFDLClone = _selectedFDL.Clone();
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
        /// Sets and gets the Factories property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public ObservableCollection<Factory> Factories { get; set; }
        #endregion

        #region Commands Definitions
        public RelayCommand ClearFDLCommand { get; set; }
        public RelayCommand<FDL> SaveFDLCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the EmailViewModel class.
        /// </summary>
        public FDLViewModel(FDLManager manager)
        {
            _db = new DBEntities();
            _fdlManager = manager;

            FDLs = new BindingList<FDL>(_db.FDLs.ToList());
            FDLResults = new ObservableCollection<FDLResult>(_db.FDLResults);
            Factories = new ObservableCollection<Factory>(_db.Factories);

            ClearFDLCommand = new RelayCommand(ClearFDL, () => { return IsInputEnabled; });
            SaveFDLCommand = new RelayCommand<FDL>(SaveFDL, (FDL fdl) => { return IsInputEnabled && fdl != null; });
        }

        public void ClearFDL()
        {

        }

        public void SaveFDL(FDL fdl)
        {
            if (fdl == null)
                return;

            if(fdl.Factory == null)
                MessageBox.Show("Please select a factory before continue. Operation cancelled.", "Invalid FDL", MessageBoxButton.OK, MessageBoxImage.Warning);
            
            _db.FDLs.AddOrUpdate(fdl);
            _db.SaveChanges();
        }
    }
}