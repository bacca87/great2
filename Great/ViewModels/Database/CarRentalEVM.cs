using AutoMapper;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Utils.Extensions;
using System;
using System.Data.Entity.Migrations;

namespace Great.ViewModels.Database
{
    public class CarRentalHistoryEVM : EntityViewModelBase
    {
        #region Properties
        public long Id { get; set; }

        private long _car;
        public long Car
        {
            get => _car;
            set
            {
                Set(ref _car, value);
                RaisePropertyChanged(nameof(Car));
            }
        }

        private long _startKm;
        public long StartKm
        {
            get => _startKm;
            set
            {
                Set(ref _startKm, value);
                RaisePropertyChanged(nameof(StartKm));
            }
        }

        private long _endKm;
        public long EndKm
        {
            get => _endKm;
            set
            {
                Set(ref _endKm, value);
                RaisePropertyChanged(nameof(EndKm));
            }
        }

        private string _startLocation;
        public string StartLocation
        {
            get => _startLocation;
            set
            {
                Set(ref _startLocation, value);
                RaisePropertyChanged(nameof(StartLocation));
            }
        }

        private string _endLocation;
        public string EndLocation
        {
            get => _endLocation;
            set
            {
                Set(ref _endLocation, value);
                RaisePropertyChanged(nameof(EndLocation));
            }
        }

        private long _startDate;
        public long StartDate
        {
            get => _startDate;
            set
            {
                Set(ref _startDate, value);
                RaisePropertyChanged(nameof(StartDate));
            }
        }

        private long _endDate;
        public long EndDate
        {
            get => _endDate;
            set
            {
                Set(ref _endDate, value);
                RaisePropertyChanged(nameof(EndDate));
            }
        }

        private long _startFuelLevel;
        public long StartFuelLevel
        {
            get => _startFuelLevel;
            set
            {
                Set(ref _startFuelLevel, value);
                RaisePropertyChanged(nameof(StartFuelLevel));
            }
        }

        private long _endFuelLevel;
        public long EndFuelLevel
        {
            get => _endFuelLevel;
            set
            {
                Set(ref _endFuelLevel, value);
                RaisePropertyChanged(nameof(EndFuelLevel));
            }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set
            {
                Set(ref _notes, value);
                RaisePropertyChanged(nameof(Notes));
            }
        }

        public DateTime RentStartDate
        {
            get => DateTime.Now.FromUnixTimestamp(StartDate);
            set => StartDate = value.ToUnixTimestamp();
        }

        public DateTime? RentEndDate
        {
            get
            {

                return DateTime.Now.FromUnixTimestamp(EndDate);


            }

            set
            {
                if (value != null)
                {
                    EndDate = ((DateTime)value).ToUnixTimestamp();

                }

            }

        }

        private CarEVM _car1;
        public CarEVM Car1
        {
            get => _car1;
            set => Set(ref _car1, value);
        }

        #endregion

        public CarRentalHistoryEVM(CarRentalHistory rent = null)
        {
            if(rent != null)
                Mapper.Map(rent, this);
        }

        public override bool Save(DBArchive db)
        {
            CarRentalHistory rent = new CarRentalHistory();

            Mapper.Map(this, rent);
            db.CarRentalHistories.AddOrUpdate(rent);
            db.SaveChanges();
            Id = rent.Id;

            return true;
        }

        public override bool Delete(DBArchive db)
        {
            throw new NotImplementedException();
        }

        public override bool Refresh(DBArchive db)
        {
            throw new NotImplementedException();
        }
    }
}
