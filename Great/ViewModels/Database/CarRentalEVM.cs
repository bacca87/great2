using Great.Models.Database;
using Great.Utils.Extensions;
using System;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Great.ViewModels.Database
{
    public class CarRentalHistoryEVM : EntityViewModelBase, IDataErrorInfo
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
                IsChanged = true;
            }
        }

        private long _startKm;
        public long StartKm
        {
            get => _startKm;
            set
            {
                Set(ref _startKm, value);
                RaisePropertyChanged(nameof(EndKm));

                IsChanged = true;
            }
        }

        private long _endKm;
        public long EndKm
        {
            get => _endKm;
            set
            {
                Set(ref _endKm, value);
                RaisePropertyChanged(nameof(StartKm));
                IsChanged = true;
            }
        }

        private string _startLocation;
        public string StartLocation
        {
            get => _startLocation;
            set
            {
                Set(ref _startLocation, value);
                IsChanged = true;
            }
        }

        private string _endLocation;
        public string EndLocation
        {
            get => _endLocation;
            set
            {
                Set(ref _endLocation, value);
                IsChanged = true;
            }
        }

        private long _startDate;
        public long StartDate
        {
            get => _startDate;
            set
            {
                Set(ref _startDate, value);
                IsChanged = true;
            }
        }

        private long _endDate;
        public long EndDate
        {
            get => _endDate;
            set
            {
                Set(ref _endDate, value);
                IsChanged = true;
            }
        }


        private long _startFuelLevel;
        public long StartFuelLevel
        {
            get => _startFuelLevel;
            set
            {
                Set(ref _startFuelLevel, value);
                IsChanged = true;
            }
        }

        private long _endFuelLevel;
        public long EndFuelLevel
        {
            get => _endFuelLevel;
            set
            {
                Set(ref _endFuelLevel, value);
                IsChanged = true;
            }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set
            {
                Set(ref _notes, value);
                IsChanged = true;
            }
        }

        public DateTime RentStartDate
        {
            get => DateTime.Now.FromUnixTimestamp(StartDate);
            set
            {
                StartDate = value.ToUnixTimestamp();
                RaisePropertyChanged(nameof(RentStartDate));
                RaisePropertyChanged(nameof(RentStartTime));
                RaisePropertyChanged(nameof(RentEndDate));
                RaisePropertyChanged(nameof(RentEndTime));
            }
        }

        public DateTime? RentEndDate
        {
            get => DateTime.Now.FromUnixTimestamp(EndDate);
            set
            {
                if (value != null)
                {
                    EndDate = ((DateTime)value).ToUnixTimestamp();
                    RaisePropertyChanged(nameof(RentStartDate));
                    RaisePropertyChanged(nameof(RentStartTime));
                    RaisePropertyChanged(nameof(RentEndDate));
                    RaisePropertyChanged(nameof(RentEndTime));
                }
            }
        }

        public TimeSpan RentStartTime
        {
            get => RentStartDate.TimeOfDay;
            set
            {
                RentStartDate = new DateTime(RentStartDate.Year, RentStartDate.Month, RentStartDate.Day, value.Hours, value.Minutes, 0);
            }
        }

        public TimeSpan? RentEndTime
        {
            get { if (RentEndDate.HasValue) return RentEndDate.Value.TimeOfDay; return null; }
            set
            {
                if (value.HasValue)
                    RentEndDate = new DateTime(RentEndDate.Value.Year, RentEndDate.Value.Month, RentEndDate.Value.Day, value.Value.Hours, value.Value.Minutes, 0);
            }
        }

        public TimeSpan? RentDuration
        {
            get
            {

                if (RentEndDate.HasValue)
                {
                    return RentEndDate.Value.Subtract(RentStartDate);
                }

                return null;
            }
        }

        public long TotalDrivenKm
        {
            get
            {
                if (EndKm > 0)
                {
                    return EndKm - StartKm;
                }
                else return StartKm;

            }
        }

        private CarEVM _car1;
        public CarEVM Car1
        {
            get => _car1;
            set { Set(ref _car1, value); IsChanged = true; }
        }

        #endregion


        #region Errors Validation

        public string Error => throw new NotImplementedException();

        public bool IsValid =>
            this["StartKm"] == null
            && this["EndKm"] == null
            && this["RentStartDate"] == null
            && this["RentEndDate"] == null
            && this["RentStartTime"] == null
            && this["StartLocation"] == null
            && this["RentEndTime"] == null;


        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "StartKm":
                    case "EndKm":
                        if (EndKm < StartKm && EndKm>0)
                            return "Start Km must be lower than End Km";
                        break;
                    case "RentStartDate":
                    case "RentEndDate":
                    case "RentStartTime":
                    case "RentEndTime":
                        if (RentStartDate != null && RentEndDate.Value < RentStartDate)
                            return "Dates not valid: End Date < Start Date";
                        break;

                    case "StartLocation":
                        if (string.IsNullOrEmpty(StartLocation) || string.IsNullOrWhiteSpace(StartLocation))
                            return "Start Location not valid";
                        break;


                    default:
                        ;
                        break;
                }

                return null;
            }
        }
        #endregion

        public CarRentalHistoryEVM(CarRentalHistory rent = null)
        {
            StartFuelLevel = 8;
            EndFuelLevel = 8;
            RentStartDate = DateTime.Now;
            RentEndDate = DateTime.Now;

            if (rent != null)
                Global.Mapper.Map(rent, this);
        }

        public override bool Save(DBArchive db)
        {
            CarRentalHistory rent = new CarRentalHistory();

            Global.Mapper.Map(this, rent);
            db.CarRentalHistories.AddOrUpdate(rent);
            db.SaveChanges();
            Id = rent.Id;
            AcceptChanges();
            return true;
        }

        public override bool Delete(DBArchive db)
        {
            CarRentalHistory rent = new CarRentalHistory();
            Global.Mapper.Map(this, rent);
            db.CarRentalHistories.Remove(rent);
            db.SaveChanges();
            return true;
        }

        public override bool Refresh(DBArchive db)
        {
            CarRentalHistory cr = db.CarRentalHistories.SingleOrDefault(x => x.Id == Id);

            if (cr != null)
            {
                Global.Mapper.Map(cr, this);
                return true;
            }

            return false;
        }
    }
}
