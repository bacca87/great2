using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great2.Models;
using Great2.Models.Database;
using Great2.Models.DTO;
using Great2.Utils;
using Great2.Utils.Extensions;
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
                {

                    RentStartDateFilter = new DateTime(_currentYear, 1, 1);
                    RentStartDateFilter = new DateTime(_currentYear, 12, 31);
                    UpdateRentList();
                    FilteredRentals.MoveCurrentToFirst();
                    SelectedRent = (CarRentalHistoryEVM)FilteredRentals.CurrentItem;
                }

            }
        }

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

        private ObservableCollectionEx<CarRentalHistoryEVM> _Rentals;
        public ObservableCollectionEx<CarRentalHistoryEVM> Rentals
        {
            get => _Rentals;
            set => Set(ref _Rentals, value);
        }

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
        public ICollectionView FilteredRentals
        {
            get => _FilteredRentals;
            set => Set(ref _FilteredRentals, value);
        }

        private ObservableCollectionEx<CarEVM> _Cars;
        public ObservableCollectionEx<CarEVM> Cars
        {
            get => _Cars;
            set => Set(ref _Cars, value);
        }
        public ObservableCollection<string> RentalCompanies { get; set; }

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
                Set(ref _selectedCar, value);
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
                    car.LicensePlate = value;
                    SelectedCar = car;
                }
                else
                {
                    if (value != null)
                    SelectedCar.LicensePlate = value;
                }
                
            }
        }


        #endregion

        #region Filter properties

        private DateTime? _rentStartDateFilter;
        public DateTime? RentStartDateFilter
        {
            get => _rentStartDateFilter;
            set
            {
                Set(ref _rentStartDateFilter, value);
                if (Rentals != null)
                    ApplyFiltersCommand();
            }

        }


        private DateTime? _rentEndDateFilter;
        public DateTime? RentEndDateFilter
        {
            get => _rentEndDateFilter;
            set
            {
                Set(ref _rentEndDateFilter, value);
                if (Rentals != null)
                    ApplyFiltersCommand();
            }
        }

        private string _genericFilter;
        public string GenericFilter
        {
            get => _genericFilter;
            set
            {
                Set(ref _genericFilter, value);
                if (Rentals != null)
                    ApplyFiltersCommand();
            }
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
        public RelayCommand NextYearCommand { get; set; }
        public RelayCommand PreviousYearCommand { get; set; }
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

            RentStartDateFilter = new DateTime(DateTime.Now.Year, 1, 1);
            RentEndDateFilter = new DateTime(DateTime.Now.Year, 12, 31);

            NextYearCommand = new RelayCommand(() => { CurrentYear++; FilteredRentals.Refresh(); });
            PreviousYearCommand = new RelayCommand(() => { CurrentYear--; FilteredRentals.Refresh(); });

            using (DBArchive db = new DBArchive())
            {
                Cars = new ObservableCollectionEx<CarEVM>(db.Cars.ToList().Select(c => new CarEVM(c)));
                RentalCompanies = new ObservableCollection<string>(db.Cars.ToList().Select(c => c.CarRentalCompany).Distinct());

                var startLoc = db.CarRentalHistories.Select(x => x.StartLocation).Distinct();
                var endLoc = db.CarRentalHistories.Select(x => x.EndLocation).Distinct();
                var brands = db.CarRentalHistories.Select(x => x.Car1.Brand).Distinct();
                var models = db.CarRentalHistories.Select(x => x.Car1.Model).Distinct();

                var startTimeStamp = RentStartDateFilter.Value.ToUnixTimestamp();
                var endTimeStamp = RentEndDateFilter.Value.ToUnixTimestamp();

                Locations = new ObservableCollection<string>(startLoc.Union(endLoc).Distinct());
                CarBrands = new ObservableCollection<string>(brands.Distinct());
                CarModels = new ObservableCollection<string>(models.Distinct());
                Rentals = new ObservableCollectionEx<CarRentalHistoryEVM>(db.CarRentalHistories.Where(c => c.StartDate >= startTimeStamp && c.EndDate <= endTimeStamp).ToList().Select(c => new CarRentalHistoryEVM(c)));
            }

            FilteredRentals = CollectionViewSource.GetDefaultView(_Rentals);
            SortDescription sd = new SortDescription("StartDate", ListSortDirection.Descending);
            FilteredRentals.SortDescriptions.Add(sd);
            FilteredRentals.Filter += Filter;
            FilteredRentals.MoveCurrentToFirst();
            SelectedRent = (CarRentalHistoryEVM)FilteredRentals.CurrentItem;

        }

        private void RemoveFiltersCommand()
        {
            GenericFilter = null;

            RentStartDateFilter = new DateTime(CurrentYear, 1, 1);
            RentEndDateFilter = new DateTime(CurrentYear, 12, 31);
            FilteredRentals.Refresh();
        }

        private void ApplyFiltersCommand()
        {
            UpdateRentList();
            FilteredRentals.Refresh();
            FilteredRentals.MoveCurrentToFirst();
            SelectedRent = (CarRentalHistoryEVM)FilteredRentals.CurrentItem ?? new CarRentalHistoryEVM();
        }

        public bool Filter(object cr)
        {
            CarRentalHistoryEVM crh = (CarRentalHistoryEVM)cr;

            string filter = GenericFilter ?? string.Empty;
            DateTime start = RentStartDateFilter ?? new DateTime(DateTime.Now.Year, 1, 1);
            DateTime end = RentEndDateFilter ?? new DateTime(DateTime.Now.Year, 12, 31);

            bool result = crh.Car1.Model.ToUpper().Contains(filter.ToUpper()) || filter == string.Empty;
            result |= crh.Car1.Brand.ToUpper().Contains(filter.ToUpper()) || filter == string.Empty;
            result |= crh.Car1.LicensePlate.ToUpper().Contains(filter.ToUpper()) || filter == string.Empty;
            result &= crh.RentStartDate >= start;
            result &= crh.RentStartDate <= end;

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

            if (RentalCompanies.SingleOrDefault(x => x == rc.Car1.CarRentalCompany.Trim()) == null)
                RentalCompanies.Add(rc.Car1.CarRentalCompany.Trim());

            if (!String.IsNullOrEmpty(rc.EndLocation))
            {
                if (Locations.SingleOrDefault(x => x == rc.EndLocation) == null)
                    Locations.Add(rc.EndLocation);
            }

            ShowEditMenu = false;
            FilteredRentals.Refresh();

        }

        private void UpdateRentList()
        {
            Rentals.Clear();
            var minTimeStamp = RentStartDateFilter.Value.ToUnixTimestamp();
            var maxTimeStamp = RentEndDateFilter.Value.ToUnixTimestamp();

            using (DBArchive db = new DBArchive())
            {
                (from r in db.CarRentalHistories
                 where r.StartDate >= minTimeStamp && r.StartDate <= maxTimeStamp
                 select r).ToList().ForEach(c => Rentals.Add(new CarRentalHistoryEVM(c)));

            }
        }
    }
}
