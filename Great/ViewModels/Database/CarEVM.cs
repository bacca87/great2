using Great2.Models.Database;
using Great2.Models.DTO;
using Great2.Utils;
using System;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Great2.ViewModels.Database
{
    public class CarEVM : EntityViewModelBase, IDataErrorInfo
    {
        #region Properties

        public long Id { get; set; }

        private string _licencePlate;
        public string LicensePlate
        {
            get => _licencePlate;
            set => SetAndCheckChanged(ref _licencePlate, value);

        }

        private string _brand;
        public string Brand
        {
            get => _brand;
            set => SetAndCheckChanged(ref _brand, value);

        }

        private string _model;
        public string Model
        {
            get => _model;
            set => SetAndCheckChanged(ref _model, value);
        }

        private string _carRentalCompany;
        public string CarRentalCompany

        {
            get => _carRentalCompany;
            set => SetAndCheckChanged(ref _carRentalCompany, value);

        }


        private ObservableCollectionEx<CarRentalHistoryEVM> _carRentalHistories;
        public ObservableCollectionEx<CarRentalHistoryEVM> CarRentalHistories
        {
            get => _carRentalHistories;
            set => Set(ref _carRentalHistories, value);
        }

        #endregion


        #region Errors Validation

        public string Error => throw new NotImplementedException();

        public bool IsValid =>
            this["LicensePlate"] == null
            && this["Brand"] == null
            && this["Model"] == null
            && this["CarRentalCompany"] == null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "LicensePlate":
                        if (string.IsNullOrEmpty(LicensePlate) || string.IsNullOrWhiteSpace(LicensePlate))
                            return "License Plate not valid";
                        break;

                    case "Brand":
                        if (string.IsNullOrEmpty(Brand) || string.IsNullOrWhiteSpace(Brand))
                            return "Brand not valid";
                        break;

                    case "Model":
                        if (string.IsNullOrEmpty(Model) || string.IsNullOrWhiteSpace(Model))
                            return "Model not valid";
                        break;
                    case "CarRentalCompany1":
                        if (string.IsNullOrEmpty(CarRentalCompany) || string.IsNullOrWhiteSpace(CarRentalCompany))
                            return "Rental company not valid";
                        break;

                    default:

                        break;
                }

                return null;
            }
        }
        #endregion
        public CarEVM(Car car = null)
        {
            if (car != null)
                Global.Mapper.Map(car, this);
            IsChanged = false;
        }

        public override bool Save(DBArchive db)
        {
            Car car = new Car();

            Global.Mapper.Map(this, car);
            db.Cars.AddOrUpdate(car);
            db.SaveChanges();
            Id = car.Id;
            IsChanged = false;
            return true;

        }

        public override bool Delete(DBArchive db)
        {
            var car = db.Cars.SingleOrDefault(x => x.Id == Id);
            if (car != null)
            {
                db.Cars.Remove(car);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        public override bool Refresh(DBArchive db)
        {
            var car = db.Cars.SingleOrDefault(x => x.Id == Id);
            if (car != null)
            {
                Global.Mapper.Map(car, this);
                return true;
            }
            return false;
        }

    }
}
