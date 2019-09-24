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
            set => SetAndCheckChanged(ref _car, value);
        }

        private CarEVM _car1;
        public CarEVM Car1
        {
            get => _car1;
            set
            {
                Set(ref _car1, value);
                RaisePropertyChanged(nameof(Car));

            }
        }

        private long _startKm;
        public long StartKm
        {
            get => _startKm;
            set
            {
                SetAndCheckChanged(ref _startKm, value);
                RaisePropertyChanged(nameof(EndKm));

            }
        }

        private long _endKm;
        public long EndKm
        {
            get => _endKm;
            set
            {
                SetAndCheckChanged(ref _endKm, value);
                RaisePropertyChanged(nameof(StartKm));
                RaisePropertyChanged(nameof(EndLocation));
                RaisePropertyChanged(nameof(RentEndTime));
                RaisePropertyChanged(nameof(RentEndDate));
            }
        }

        private string _startLocation;
        public string StartLocation
        {
            get => _startLocation;
            set => SetAndCheckChanged(ref _startLocation, value);

        }

        private string _endLocation;
        public string EndLocation
        {
            get => _endLocation;
            set
            {
                SetAndCheckChanged(ref _endLocation, value);
                RaisePropertyChanged(nameof(EndKm));
                RaisePropertyChanged(nameof(RentEndDate));
            }
        }

        private long _startDate;
        public long StartDate
        {
            get => _startDate;
            set => SetAndCheckChanged(ref _startDate, value);

        }

        private long _endDate;
        public long EndDate
        {
            get => _endDate;
            set => SetAndCheckChanged(ref _endDate, value);

        }

        private long _startFuelLevel;
        public long StartFuelLevel
        {
            get => _startFuelLevel;
            set => SetAndCheckChanged(ref _startFuelLevel, value);

        }

        private long _endFuelLevel;
        public long EndFuelLevel
        {
            get => _endFuelLevel;
            set => SetAndCheckChanged(ref _endFuelLevel, value);


        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set => SetAndCheckChanged(ref _notes, value);

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
            get
            {
                if (EndDate > 0) return DateTime.Now.FromUnixTimestamp(EndDate);

                return null;
            }
            set
            {
                if (value == null)
                    EndDate = 0;
                else
                    EndDate = ((DateTime)value).ToUnixTimestamp();

                RaisePropertyChanged(nameof(EndKm));
                RaisePropertyChanged(nameof(RentStartDate));
                RaisePropertyChanged(nameof(RentStartTime));
                RaisePropertyChanged(nameof(RentEndDate));
                RaisePropertyChanged(nameof(RentEndTime));
                RaisePropertyChanged(nameof(EndLocation));

            }
        }

        public TimeSpan RentStartTime
        {
            get => RentStartDate.TimeOfDay;
            set => RentStartDate = new DateTime(RentStartDate.Year, RentStartDate.Month, RentStartDate.Day, value.Hours, value.Minutes, 0);
        }

        public TimeSpan? RentEndTime
        {
            get { if (RentEndDate.HasValue) return RentEndDate.Value.TimeOfDay;
                return null; }
            set
            {
                if (value.HasValue) RentEndDate = new DateTime(RentEndDate.Value.Year, RentEndDate.Value.Month, RentEndDate.Value.Day, value.Value.Hours, value.Value.Minutes, 0);
            }
        }

        public TimeSpan? RentDuration => RentEndDate?.Subtract(RentStartDate);

        public long TotalDrivenKm
        {
            get
            {
                if (EndKm > 0)
                    return EndKm - StartKm;
                else
                    return StartKm;
            }
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
            && this["EndLocation"] == null
            && this["RentEndTime"] == null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "StartKm":
                        if (EndKm < StartKm && EndKm > 0) return "Start Km must be lower than End Km";

                        break;

                    case "EndKm":
                        if (EndKm < StartKm && EndKm > 0) return "Start Km must be lower than End Km";

                        if (EndKm == 0 && RentEndDate.HasValue) return "End Km must be set when defining end date";

                        if (!String.IsNullOrWhiteSpace(EndLocation) && EndKm == 0) return "End Km must be set when defining end location";

                        break;

                    case "RentStartDate":
                    case "RentStartTime":

                        if (RentStartDate != null && RentEndDate < RentStartDate) return "Dates not valid: End Date < Start Date";

                        break;

                    case "RentEndDate":
                    case "RentEndTime":

                        if (RentStartDate != null && RentEndDate < RentStartDate) return "Dates not valid: End Date < Start Date";

                        if (!RentEndDate.HasValue && EndKm > StartKm) return "End Date must be set when defining end km";

                        if (!RentEndDate.HasValue && !String.IsNullOrWhiteSpace(EndLocation)) return "End Date must be set when defining end location";

                        break;

                    case "StartLocation":
                        if (string.IsNullOrEmpty(StartLocation) || string.IsNullOrWhiteSpace(StartLocation)) return "Start Location not valid";

                        break;

                    case "EndLocation":
                        if (RentEndDate.HasValue && (string.IsNullOrEmpty(EndLocation) || string.IsNullOrWhiteSpace(EndLocation))) return "End location is required when setting End Date";

                        if (EndKm > StartKm && (string.IsNullOrEmpty(EndLocation) || string.IsNullOrWhiteSpace(EndLocation))) return "End location is required when setting End Km";

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
            RentStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

            if (rent != null) Global.Mapper.Map(rent, this);

            IsChanged = false;
        }

        public override bool Save(DBArchive db)
        {
            CarRentalHistory rent = new CarRentalHistory();

            Global.Mapper.Map(this, rent);
            db.CarRentalHistories.AddOrUpdate(rent);
            db.SaveChanges();
            Id = rent.Id;
            return true;
        }

        public override bool Delete(DBArchive db)
        {
            CarRentalHistory cr = db.CarRentalHistories.SingleOrDefault(x => x.Id == Id);

            if (cr != null)
            {
                db.CarRentalHistories.Remove(cr);
                db.SaveChanges();
                return true;
            }

            return false;
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
