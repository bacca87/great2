using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great2.Models.Database;
using Great2.Models.DTO;
using Great2.Utils;
using Great2.ViewModels.Database;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Great2.ViewModels
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

        private bool _isInputEnabled = false;
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

        private bool _showEditMenu;
        public bool ShowEditMenu
        {
            get => _showEditMenu;
            set => Set(ref _showEditMenu, value);
        }

        public ObservableCollectionEx<CarRentalHistoryEVM> Rentals { get; set; }

        private ObservableCollection<string> _CarModels;
        public ObservableCollection<string> CarModels
        {
            get => _CarModels;
            set => Set(ref _CarModels, value);
        }

        private ObservableCollection<string> _CarBrands;
        public ObservableCollection<string> CarBrands
        {
            get => _CarBrands;
            set => Set(ref _CarBrands, value);
        }

        private ObservableCollection<string> _Locations;
        public ObservableCollection<string> Locations
        {
            get => _Locations;
            set => Set(ref _Locations, value);
        }

        private ICollectionView _FilteredRentals;
        public ICollectionView FilteredRentals => _FilteredRentals;

        private ObservableCollectionEx<CarEVM> _Cars;
        public ObservableCollectionEx<CarEVM> Cars
        {
            get => _Cars;
            set => Set(ref _Cars, value);
        }
        public ObservableCollection<CarRentalCompanyDTO> RentalCompanies { get; set; }

        private CarRentalHistoryEVM _selectedRent;
        public CarRentalHistoryEVM SelectedRent
        {
            get => _selectedRent;

            set
            {
                _selectedRent?.CheckChangedEntity();
                Set(ref _selectedRent, value ?? new CarRentalHistoryEVM());
                LicensePlate = _selectedRent?.Car1?.LicensePlate;

                ShowEditMenu = false;
            }
        }

        private CarEVM _selectedCar;
        public CarEVM SelectedCar
        {
            get => _selectedCar;

            set
            {
                _selectedCar?.CheckChangedEntity();

                if (value != null)
                {
                    Set(ref _selectedCar, value);
                }
            }
        }

        private string _LicensePlate;
        public string LicensePlate
        {
            get => _LicensePlate;
            set
            {
                Set(ref _LicensePlate, value);
                var car = Cars.SingleOrDefault(x => x.LicensePlate == _LicensePlate);
                if (car != null)
                {
                    SelectedCar = car;
                }
                //   SelectedCar.LicensePlate = value;
            }
        }


        #endregion

        #region Filter properties
        public bool EnableStartDateFilter { get; set; }
        public bool EnableEndDateFilter { get; set; }

        private DateTime? _rentStartDateFilter;
        public DateTime? RentStartDateFilter
        {
            get => _rentStartDateFilter;
            set => Set(ref _rentStartDateFilter, value);

        }


        private DateTime? _rentEndDateFilter;
        public DateTime? RentEndDateFilter
        {
            get => _rentEndDateFilter;
            set => Set(ref _rentEndDateFilter, value);
        }


        private string _modelBrandFilter;
        public string ModelBrandFilter
        {
            get => _modelBrandFilter;

            set => _modelBrandFilter = value;
        }


        private string _licensePlateFilter;
        public string LicencePlateFilter
        {
            get => _licensePlateFilter;

            set => _licensePlateFilter = value;
        }

        #endregion

        #region Commands Definitions
        public RelayCommand ClearCommand { get; set; }
        public RelayCommand<CarRentalHistoryEVM> SaveCommand { get; set; }
        public RelayCommand<CarRentalHistoryEVM> DeleteCommand { get; set; }
        public RelayCommand<CarRentalHistoryEVM> NewCommand { get; set; }
        public RelayCommand GotFocusCommand { get; set; }
        public RelayCommand LostFocusCommand { get; set; }
        public RelayCommand ApplyFilters { get; set; }
        public RelayCommand RemoveFilters { get; set; }
        public RelayCommand PageUnloadedCommand { get; set; }

        #endregion


        /// <summary>
        /// Initializes a new instance of the CarRentalViewModel class.
        /// </summary>
        public CarRentalViewModel()
        {

            IsInputEnabled = true;

            SaveCommand = new RelayCommand<CarRentalHistoryEVM>(SaveRent);
            DeleteCommand = new RelayCommand<CarRentalHistoryEVM>(DeleteRent);
            NewCommand = new RelayCommand<CarRentalHistoryEVM>(NewRent);
            GotFocusCommand = new RelayCommand(() => { ShowEditMenu = true; });
            LostFocusCommand = new RelayCommand(() => { });
            PageUnloadedCommand = new RelayCommand(() => { SelectedRent?.CheckChangedEntity(); SelectedCar?.CheckChangedEntity(); });

            ApplyFilters = new RelayCommand(ApplyFiltersCommand);
            RemoveFilters = new RelayCommand(RemoveFiltersCommand);

            RentStartDateFilter = DateTime.Now;
            RentEndDateFilter = DateTime.Now;

            using (DBArchive db = new DBArchive())
            {
                Rentals = new ObservableCollectionEx<CarRentalHistoryEVM>(db.CarRentalHistories.ToList().Select(cr => new CarRentalHistoryEVM(cr)));
                Cars = new ObservableCollectionEx<CarEVM>(db.Cars.ToList().Select(c => new CarEVM(c)));
                RentalCompanies = new ObservableCollection<CarRentalCompanyDTO>(db.CarRentalCompanies.ToList().Select(c => new CarRentalCompanyDTO(c)));

                var startLoc = db.CarRentalHistories.Select(x => x.StartLocation).Distinct();
                var endLoc = db.CarRentalHistories.Select(x => x.EndLocation).Distinct();
                var brands = db.CarRentalHistories.Select(x => x.Car1.Brand).Distinct();
                var models = db.CarRentalHistories.Select(x => x.Car1.Model).Distinct();

                Locations = new ObservableCollection<string>(startLoc.Union(endLoc).Distinct());
                CarBrands = new ObservableCollection<string>(brands.Distinct());
                CarModels = new ObservableCollection<string>(models.Distinct());
            }

            _FilteredRentals = CollectionViewSource.GetDefaultView(Rentals);
            SortDescription sd = new SortDescription("RentStartDate", ListSortDirection.Descending);
            _FilteredRentals.SortDescriptions.Add(sd);
            _FilteredRentals.Filter += Filter;

            FilteredRentals.MoveCurrentToFirst();
            SelectedRent = (CarRentalHistoryEVM)FilteredRentals.CurrentItem;
        }




        private void RemoveFiltersCommand()
        {
            LicencePlateFilter = null;
            ModelBrandFilter = null;
            ModelBrandFilter = null;

            RentStartDateFilter = DateTime.Now;
            RentEndDateFilter = DateTime.Now;
            FilteredRentals.Refresh();
        }

        private void ApplyFiltersCommand()
        {
            FilteredRentals.Refresh();
        }

        public bool Filter(object cr)
        {
            CarRentalHistoryEVM crh = (CarRentalHistoryEVM)cr;

            string model = ModelBrandFilter ?? string.Empty;
            string plate = LicencePlateFilter ?? string.Empty;
            DateTime start = RentStartDateFilter ?? DateTime.MinValue;
            DateTime end = RentEndDateFilter ?? DateTime.MaxValue;

            bool result = crh.Car1.Model.ToUpper().Contains(model.ToUpper()) || model == string.Empty;
            result |= crh.Car1.Brand.ToUpper().Contains(model.ToUpper()) || model == string.Empty;
            result &= crh.Car1.LicensePlate.ToUpper().Contains(plate.ToUpper()) || plate == string.Empty;
            result &= crh.RentStartDate >= start || !EnableStartDateFilter;
            result &= crh.RentStartDate <= end || !EnableEndDateFilter;

            return result;
        }

        private void NewRent(CarRentalHistoryEVM obj)
        {
            SelectedRent = new CarRentalHistoryEVM();
            SelectedCar = new CarEVM();
        }

        private void DeleteRent(CarRentalHistoryEVM cr)
        {
            if (cr.Id == 0) return;

            if (MetroMessageBox.Show("Do you want to delete the selected rent?", "Rent Delete", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
            {
                using (DBArchive db = new DBArchive())
                {
                    var rentalsWithSameCar = db.CarRentalHistories.Where(c => c.Car == cr.Car && c.Id != cr.Id).ToList();
                    if (rentalsWithSameCar.Count() == 0)
                    {
                        var car = db.Cars.Where(c => c.Id == cr.Car).SingleOrDefault();
                        if (car != null)
                        {
                            db.Cars.Remove(car);
                            Cars.Remove(new CarEVM(car));
                        }
                    }

                    var rent = db.CarRentalHistories.Where(c => c.Id == cr.Id).FirstOrDefault();
                    db.CarRentalHistories.Remove(rent);

                    Rentals.Remove(cr);
                    db.SaveChanges();
                    FilteredRentals.Refresh();
                }
            }
        }

        public void SaveRent(CarRentalHistoryEVM rc)
        {
            if (rc == null || SelectedCar == null)
                return;

            if (!rc.IsValid || !SelectedCar.IsValid)
            {
                MetroMessageBox.Show("Cannot save/edit the rent. Please check the errors", "Save Rent", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var existingRent = Rentals.SingleOrDefault(r => r.Id == SelectedRent.Id);
            var existingCar = Cars.SingleOrDefault(r => r.Id == SelectedCar.Id);

            using (DBArchive db = new DBArchive())
            {
                SelectedCar.Save(db);

                rc.Car = SelectedCar.Id;
                rc.Car1 = SelectedCar;
                rc.Save(db);
                db.SaveChanges();
            }

            if (existingRent == null)           
                Rentals.Add(rc);

            if (existingCar == null)
                Cars.Add(rc.Car1);

            if (CarBrands.SingleOrDefault(x => x == rc.Car1?.Brand) == null)
                CarBrands.Add(rc.Car1.Brand);

            if (CarBrands.SingleOrDefault(x => x == rc.Car1?.Model) == null)
                CarModels.Add(rc.Car1.Model);

            if (Locations.SingleOrDefault(x => x == rc.StartLocation) == null)
                Locations.Add(rc.StartLocation);

            if (!String.IsNullOrEmpty(rc.EndLocation))
            {
                if (Locations.SingleOrDefault(x => x == rc.EndLocation) == null)
                    Locations.Add(rc.EndLocation);
            }

            ShowEditMenu = false;
            FilteredRentals.Refresh();

        }
    }
}
