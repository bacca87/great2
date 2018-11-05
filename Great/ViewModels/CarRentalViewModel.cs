using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models;
using Great.Utils.Messages;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

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
        private DBArchive _db { get; set; }

        /// <summary>
        /// The <see cref="CarRentalHostory" /> property's name.
        /// </summary>
        private ObservableCollection<CarRentalHistory> _carRentals;

        /// <summary>
        /// Sets and gets the CarRentalHistory property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>        
        public ObservableCollection<CarRentalHistory> CarRentals
        {
            get => _carRentals;
            set
            {
                _carRentals = value;
                RaisePropertyChanged(nameof(CarRentalCompany), true);
            }
        }

        /// <summary>
        /// The <see cref="SelectedCarRental" /> property's name.
        /// </summary>
        private CarRentalHistory _selectedCarRental;

        /// <summary>
        /// Sets and gets the NewCar property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Car NewCar
        {
            get => _newCar;

            set
            {
                _newCar = value;
                RaisePropertyChanged(nameof(NewCar), value);
            }
        }

        /// <summary>
        /// The <see cref="NewCar" /> property's name.
        /// </summary>
        private Car _newCar;

        /// <summary>
        /// Sets and gets the SelectedCarRentalHistory property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public CarRentalHistory SelectedCarRental
        {
            get => _selectedCarRental;

            set
            {
                _selectedCarRental = value;
                RaisePropertyChanged(nameof(SelectedCarRental), value);
            }
        }

        /// <summary>
        /// Sets and gets the Companies property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        /// 
        /// <summary>
        /// The <see cref="Companies" /> property's name.
        /// </summary>
        private ObservableCollection<CarRentalCompany> _companies;
        public ObservableCollection<CarRentalCompany> Companies
        {
            get => _companies;
            set
            {
                _companies = value;
                RaisePropertyChanged(nameof(Companies), value);
            }
        }

        #endregion

        #region Commands
        public RelayCommand<CarRentalHistory> DeleteRentCommand { get; set; }
        public RelayCommand<CarRentalHistory> SaveRentCommand { get; set; }
        public RelayCommand ClearSelectionCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the CarRentalViewModel class.
        /// </summary>
        public CarRentalViewModel(DBArchive db)
        {
            _db = db;

            CarRentals = new ObservableCollection<CarRentalHistory>(_db.CarRentalHistories.ToList());
            Companies = new ObservableCollection<CarRentalCompany>(_db.CarRentalCompanies.ToList());
            CarRentals.CollectionChanged += CarRentals_CollectionChanged;

            DeleteRentCommand = new RelayCommand<CarRentalHistory>(DeleteRent);
            SaveRentCommand = new RelayCommand<CarRentalHistory>(SaveRent);
            ClearSelectionCommand = new RelayCommand(ClearSelection);

        }

        private void CarRentals_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CarRentals), null, CarRentals, true);
        }

        public void NewFactory(NewItemMessage<CarRentalHistory> item)
        {
            // Using the dispatcher for preventing thread conflicts
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        CarRentalHistory rental = _db.CarRentalHistories.SingleOrDefault(f => f.Id == item.Content.Id);

                        if (rental != null && !CarRentals.Contains(rental))
                        {
                            CarRentals.Add(rental);
                        }
                    }
                })
            );
        }

        private void ClearSelection()
        {
            SelectedCarRental = null;
        }

        private void DeleteRent(CarRentalHistory rent)
        {
            _db.CarRentalHistories.Remove(rent);

            if (_db.SaveChanges() > 0)
            {
                CarRentals.Remove(rent);
                SelectedCarRental = null;
            }
        }

        private void SaveRent(CarRentalHistory rent)
        {
            _db.CarRentalHistories.AddOrUpdate(rent);

            if (_db.SaveChanges() > 0)
            {
                // check if already exist in the collection
                CarRentalHistory itemRent = CarRentals.Where(f => f.Id == rent.Id).FirstOrDefault();

                if (itemRent != null)
                {
                    CarRentals[CarRentals.IndexOf(itemRent)] = rent;
                }
                else
                {
                    CarRentals.Add(rent);
                }

                SelectedCarRental = rent;
            }
        }

    }
}
