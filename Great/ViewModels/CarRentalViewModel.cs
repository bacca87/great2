using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Controls;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Utils;
using Great.Utils.Extensions;
using Great.ViewModels.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Great.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class CarRentalViewModel : ViewModelBase, IDataErrorInfo
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

        public bool EnableStartDateFilter { get; set; }
        public bool EnableEndDateFilter { get; set; }

        public ObservableCollectionEx<CarRentalHistoryEVM> Rentals { get; set; }

        public ObservableCollection<string> CarModels
        {
            get => new ObservableCollection<string>(Rentals.Select(x => x.Car1?.Model).Distinct());
        }

        public ObservableCollection<string> CarBrands
        {
            get => new ObservableCollection<string>(Rentals.Select(x => x.Car1?.Brand).Distinct());
        }

        public ObservableCollection<string> StartLocations
        {
            get => new ObservableCollection<string>(Rentals.Select(x => x.StartLocation).Distinct());
        }

        public ObservableCollection<string> EndLocations
        {
            get => new ObservableCollection<string>(Rentals.Select(x => x.EndLocation).Distinct());
        }
        private ICollectionView _FilteredRentals;
        public ICollectionView FilteredRentals
        {
            get { return _FilteredRentals; }
        }
        public ObservableCollectionEx<CarEVM> Cars { get; set; }
        public ObservableCollection<CarRentalCompanyDTO> RentalCompanies { get; set; }

        private CarRentalHistoryEVM _selectedRent;
        public CarRentalHistoryEVM SelectedRent
        {
            get => _selectedRent;

            set
            {
                Set(ref _selectedRent, value);
                SelectedCar = _selectedRent?.Car1;

                ShowEditMenu = false;
            }
        }

        private CarEVM _selectedCar;
        public CarEVM SelectedCar
        {
            get => _selectedCar;

            set
            {
                if (value != null)
                    Set(ref _selectedCar, value);
            }
        }

        #endregion

        #region Filter properties

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

        #endregion

        #region Errors Validation

        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {


                    default:
                        break;
                }

                return null;
            }
        }
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
            ApplyFilters = new RelayCommand(ApplyFiltersCommand);
            RemoveFilters = new RelayCommand(RemoveFiltersCommand);

            //by default initialize filter on last year 
            RentStartDateFilter = DateTime.Now;
            RentEndDateFilter = DateTime.Now;

            using (DBArchive db = new DBArchive())
            {
                Rentals = new ObservableCollectionEx<CarRentalHistoryEVM>(db.CarRentalHistories.ToList().Select(cr => new CarRentalHistoryEVM(cr)));
                Cars = new ObservableCollectionEx<CarEVM>(db.Cars.ToList().Select(c => new CarEVM(c)));
                RentalCompanies = new ObservableCollection<CarRentalCompanyDTO>(db.CarRentalCompanies.ToList().Select(c => new CarRentalCompanyDTO(c)));
            }

            _FilteredRentals = CollectionViewSource.GetDefaultView(Rentals);
            SortDescription sd = new SortDescription("RentStartDate", ListSortDirection.Descending);
            _FilteredRentals.SortDescriptions.Add(sd);
            _FilteredRentals.Filter += Filter;
            SelectedCar = new CarEVM();
            SelectedRent = new CarRentalHistoryEVM();

        }

        private void RemoveFiltersCommand()
        {
            LicencePlateFilter = null;
            ModelBrandFilter = null;
            ModelBrandFilter = null;

            //by default initialize filter on last year 
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
            if (MetroMessageBox.Show("Do you want to delete the selected rent?", "Rent Delete", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
            {
                using (DBArchive db = new DBArchive())
                {
                    var rentalsWithSameCar = db.CarRentalHistories.Where(c => c.Car == cr.Car && c.Id != cr.Id).ToList();
                    if (rentalsWithSameCar.Count() == 0)
                    {
                        var car = db.Cars.Where(c => c.Id == cr.Car).FirstOrDefault();
                        db.Cars.Remove(car);
                        Cars.Remove(new CarEVM(car));
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
            {
                return;
            }

      //      var existingCar = Cars.SingleOrDefault(c => c.Id == SelectedCar.Id);
            var existingRent = Rentals.SingleOrDefault(r => r.Id == SelectedRent.Id);

            //if (existingCar != null)
            //{

            //}
            ////avoid update existing car
            //if (existingCar?.Model != SelectedCar.Model
            //    || existingCar?.Brand != SelectedCar.Brand
            //    || existingCar?.LicensePlate != SelectedCar.LicensePlate
            //    || existingCar?.CarRentalCompany != SelectedCar.CarRentalCompany)
            //{
            //    var car = new CarEVM();
            //    car.Id = SelectedCar.Id;
            //    car.Brand = SelectedCar.Brand;
            //    car.Model = SelectedCar.Model;
            //    car.LicensePlate = SelectedCar.LicensePlate;
            //    car.CarRentalCompany = SelectedCar.CarRentalCompany;
            //    SelectedCar = car;
            //}

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
            FilteredRentals.Refresh();
            ShowEditMenu = false;
        }
    }
}
