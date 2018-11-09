using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models.Database;
using Great.Utils;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Windows;

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
        /// The <see cref="ShowContextualMenu" /> property's name.
        /// </summary>
        private bool _showContextualMenu = false;

        /// <summary>
        /// Sets and gets the IsChanged property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public bool ShowContextualMenu
        {
            get => _showContextualMenu;

            set
            {
                if (_showContextualMenu == value)
                {
                    return;
                }

                var oldValue = _showContextualMenu;
                _showContextualMenu = value;

                RaisePropertyChanged(nameof(ShowContextualMenu), oldValue, value);

            }
        }

        /// <summary>
        /// The <see cref="Rentals" /> property's name.
        /// </summary>
        private ObservableCollectionEx<CarRentalHistory> _rentals;

        /// <summary>
        /// Sets and gets the Rentals property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>        
        public ObservableCollectionEx<CarRentalHistory> Rentals
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

                SelectedRentClone = _selectedRent?.Clone();
                SelectedCarClone = _selectedRent?.Car1.Clone();

                RaisePropertyChanged(nameof(SelectedRent), oldValue, value);
                DeleteCommand.RaiseCanExecuteChanged();

                ShowContextualMenu = false;
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
                SaveCommand.RaiseCanExecuteChanged();

            }
        }

        /// <summary>
        /// The <see cref="SelectedCarClone" /> property's name.
        /// </summary>
        private Car _selectedCarClone;

        /// <summary>
        /// Sets and gets the SelectedRentClone property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public Car SelectedCarClone
        {
            get => _selectedCarClone;

            set
            {
                var oldValue = _selectedCarClone;
                _selectedCarClone = value;
                RaisePropertyChanged(nameof(SelectedCarClone), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="RentalCompanies" /> property's name.
        /// </summary>
        public ObservableCollectionEx<CarRentalCompany> RentalCompanies { get; set; }


        /// <summary>
        /// The <see cref="Cars" /> property's name.
        /// </summary>
        public ObservableCollectionEx<Car> Cars { get; set; }
        #endregion

        #region Commands Definitions
        public RelayCommand ClearCommand { get; set; }
        public RelayCommand<CarRentalHistory> SaveCommand { get; set; }
        public RelayCommand<CarRentalHistory> DeleteCommand { get; set; }
        public RelayCommand<CarRentalHistory> NewCommand { get; set; }
        public RelayCommand ShowContextualMenuCommand { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the ExpenseAccountViewModel class.
        /// </summary>
        public CarRentalViewModel(DBArchive db)
        {
            _db = db;

            IsInputEnabled = true;

            SaveCommand = new RelayCommand<CarRentalHistory>(SaveRent);
            DeleteCommand = new RelayCommand<CarRentalHistory>(DeleteRent);
            NewCommand = new RelayCommand<CarRentalHistory>(NewRent);
            ShowContextualMenuCommand = new RelayCommand(() => { ShowContextualMenu = true; });

            Rentals = new ObservableCollectionEx<CarRentalHistory>(_db.CarRentalHistories);
            Cars = new ObservableCollectionEx<Car>(_db.Cars);
            RentalCompanies = new ObservableCollectionEx<CarRentalCompany>(_db.CarRentalCompanies);

        }


        private void NewRent(CarRentalHistory obj)
        {
            SelectedRentClone = new CarRentalHistory();
            SelectedCarClone = new Car();
        }

        private void DeleteRent(CarRentalHistory cr)
        {
            if (MessageBox.Show("Do you want to delete the selected rent?", "Rent Delete", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
            {
                var rentalsWithSameCar = _db.CarRentalHistories.Where(c => c.Car == cr.Car);

                if (rentalsWithSameCar.Count() == 0)
                {
                    var car = _db.Cars.Where(c => c.Id == cr.Car).FirstOrDefault();
                    _db.Cars.Remove(car);
                    Cars.Remove(car);
                }

                _db.CarRentalHistories.Remove(cr);
                Rentals.Remove(cr);

                if (_db.SaveChanges() > 0)
                {
                    SelectedRent?.NotifyCarRentalHistoryPropertiesChanged();
                }
            }

        }

        public void SaveRent(CarRentalHistory rc)
        {
            if (rc == null || SelectedCarClone == null)
            {
                return;
            }
            if (!SelectedCarClone.IsValid)
            {
                MessageBox.Show("Car informations not valid", "Invalid Car informations", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!rc.IsValid)
            {
                MessageBox.Show("Rent informations not valid", "Invalid Rent informations", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _db.Cars.AddOrUpdate(SelectedCarClone);
            _db.SaveChanges();

            rc.Car = SelectedCarClone.Id;
            _db.CarRentalHistories.AddOrUpdate(rc);

            Rentals.Add(rc);

            if (_db.SaveChanges() > 0)
            {
                SelectedRent?.NotifyCarRentalHistoryPropertiesChanged();

            }
        }
    }
}
