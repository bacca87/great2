using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models.Database;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity.Migrations;

namespace Great.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class CarRentalViewModel : ViewModelBase
    {
        #region Properties
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
            get => _isInputEnabled;

            set
            {
                if (_isInputEnabled == value)
                {
                    return;
                }

                var oldValue = _isInputEnabled;
                _isInputEnabled = value;

                RaisePropertyChanged(nameof(IsInputEnabled), oldValue, value);

            }
        }

        /// <summary>
        /// The <see cref="Rentals" /> property's name.
        /// </summary>
        private ObservableCollection<CarRentalHistory> _rentals;

        /// <summary>
        /// Sets and gets the Rentals property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>        
        public ObservableCollection<CarRentalHistory> Rentals
        {
            get => _rentals;
            set
            {
                _rentals = value;
                RaisePropertyChanged(nameof(Rentals), true);
            }
        }

        /// <summary>
        /// The <see cref="SelectedRent" /> property's name.
        /// </summary>
        private CarRentalHistory _selectedRent;

        /// <summary>
        /// Sets and gets the SelectedRent property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public CarRentalHistory SelectedRent
        {
            get => _selectedRent;

            set
            {
                var oldValue = _selectedRent;
                _selectedRent = value;

                RefreshRentals();
                SelectedRentClone = _selectedRent?.Clone();



                RaisePropertyChanged(nameof(SelectedRent), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="SelectedRentClone" /> property's name.
        /// </summary>
        private CarRentalHistory _selectedRentClone;

        /// <summary>
        /// Sets and gets the SelectedRentClone property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public CarRentalHistory SelectedRentClone
        {
            get => _selectedRentClone;

            set
            {
                var oldValue = _selectedRentClone;
                _selectedRentClone = value;
                RaisePropertyChanged(nameof(SelectedRentClone), oldValue, value);
            }
        }

  
        /// <summary>
        /// The <see cref="RentalCompanies" /> property's name.
        /// </summary>
        public ObservableCollection<CarRentalCompany> RentalCompanies { get; set; }
        #endregion

        #region Commands Definitions
        public RelayCommand ClearCommand { get; set; }
        public RelayCommand<CarRentalHistory> SaveCommand { get; set; }
        public RelayCommand<CarRentalHistory> DeleteCommand { get; set; }
        public RelayCommand<CarRentalHistory> NewCommand {get;set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the ExpenseAccountViewModel class.
        /// </summary>
        public CarRentalViewModel(DBArchive db)
        {
            _db = db;

            IsInputEnabled =false;

            ClearCommand = new RelayCommand(ClearRent);
            SaveCommand = new RelayCommand<CarRentalHistory>(SaveRent);
            DeleteCommand = new RelayCommand<CarRentalHistory>(DeleteRent);
            NewCommand = new RelayCommand<CarRentalHistory>(NewRent);

            Rentals = new ObservableCollection<CarRentalHistory>(_db.CarRentalHistories);
            RentalCompanies = new ObservableCollection<CarRentalCompany>(_db.CarRentalCompanies);

        }

        private void NewRent(CarRentalHistory obj)
        {
            ClearRent();
            IsInputEnabled = true;

        }

        private void DeleteRent(CarRentalHistory cr)
        {

        }

        private void RefreshRentals()
        {

        }


        public void ClearRent()
        {
            SelectedRentClone.Id = 0;
            SelectedRentClone.Car = 0;
            SelectedRentClone.StartKm = 0;
            SelectedRentClone.EndKm = 0;
            SelectedRentClone.StartLocation = string.Empty;
            SelectedRentClone.EndLocation = string.Empty;
            SelectedRentClone.StartDate = 0;
            SelectedRentClone.EndDate = 0;
            SelectedRentClone.StartFuelLevel = 0;
            SelectedRentClone.EndFuelLevel = 0;
            SelectedRentClone.Notes = string.Empty;
            SelectedRentClone.Car1 = new Car();

        }

        public void SaveRent(CarRentalHistory rc)
        {
            if (rc == null)
            {
                return;
            }

            _db.CarRentalHistories.AddOrUpdate(rc);
            if (_db.SaveChanges() > 0)
            {
                SelectedRent?.NotifyCarRentalHistoryPropertiesChanged();
            }
        }
    }
}
