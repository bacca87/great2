using Great2.Models.Database;
using Great2.Models.DTO;
using Great2.Utils.Extensions;
using System;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Great2.ViewModels.Database
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

        private long _startDistance;
        public long StartDistance
        {
            get => _startDistance;
            set
            {
                SetAndCheckChanged(ref _startDistance, value);
                RaisePropertyChanged(nameof(EndDistance));
                RaisePropertyChanged(nameof(TotalDrivenKm));
            }
        }

        private long _endDistance;
        public long EndDistance
        {
            get => _endDistance;
            set
            {
                SetAndCheckChanged(ref _endDistance, value);
                RaisePropertyChanged(nameof(StartDistance));
                RaisePropertyChanged(nameof(EndLocation));
                RaisePropertyChanged(nameof(RentEndTime));
                RaisePropertyChanged(nameof(RentEndDate));
                RaisePropertyChanged(nameof(TotalDrivenKm));
            }
        }

        private long _UOM;
        public long UOM
        {
            get => _UOM;
            set
            {
                SetAndCheckChanged(ref _UOM, value);
                RaisePropertyChanged(nameof(TotalDrivenKm));
            }
        }

        private UOMDTO _UOM1;
        public UOMDTO UOM1
        {
            get => _UOM1;
            set => Set(ref _UOM1, value);
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
                RaisePropertyChanged(nameof(EndDistance));
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
            get => EndDate > 0 ? DateTime.Now.FromUnixTimestamp(EndDate) : (DateTime ?)null;            
            set
            {
                if (value == null)
                    EndDate = 0;
                else
                    EndDate = ((DateTime)value).ToUnixTimestamp();

                RaisePropertyChanged(nameof(EndDistance));
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
            set
            {
                RentStartDate = new DateTime(RentStartDate.Year, RentStartDate.Month, RentStartDate.Day, value.Hours, value.Minutes, 0);
            }
        }

        public TimeSpan? RentEndTime
        {
            get => RentEndDate.HasValue ? RentEndDate.Value.TimeOfDay : (TimeSpan?)null;
            set
            {
                if (value.HasValue)
                    RentEndDate = new DateTime(RentEndDate.Value.Year, RentEndDate.Value.Month, RentEndDate.Value.Day, value.Value.Hours, value.Value.Minutes, 0);
            }
        }

        public TimeSpan? RentDuration => RentEndDate?.Subtract(RentStartDate);

        public long TotalDrivenKm
        {
            get
            {
                if (EndDistance > StartDistance)
                {
                    if (UOM == 1)
                        return EndDistance - StartDistance;
                    else
                        return (long)((EndDistance / 0.62137) - (StartDistance / 0.62137)); 
                }   
                
                return 0;
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
                        if (EndDistance < StartDistance && EndDistance > 0)
                            return "Start Km must be lower than End Km";
                        break;

                    case "EndKm":
                        if (EndDistance < StartDistance && EndDistance > 0)
                            return "Start Km must be lower than End Km";

                        if (EndDistance == 0 && RentEndDate.HasValue)
                            return "End Km must be set when defining end date";

                        if (!String.IsNullOrWhiteSpace(EndLocation) && EndDistance == 0)
                            return "End Km must be set when defining end location";
                        break;

                    case "RentStartDate":
                    case "RentStartTime":

                        if (RentStartDate != null && RentEndDate < RentStartDate)
                            return "Dates not valid: End Date < Start Date";
                        break;

                    case "RentEndDate":
                    case "RentEndTime":

                        if (RentStartDate != null && RentEndDate < RentStartDate)
                            return "Dates not valid: End Date < Start Date";

                        if (!RentEndDate.HasValue && EndDistance > StartDistance)
                            return "End Date must be set when defining end km";

                        if (!RentEndDate.HasValue && !String.IsNullOrWhiteSpace(EndLocation))
                            return "End Date must be set when defining end location";
                        break;

                    case "StartLocation":
                        if (string.IsNullOrEmpty(StartLocation) || string.IsNullOrWhiteSpace(StartLocation))
                            return "Start Location not valid";
                        break;

                    case "EndLocation":
                        if (RentEndDate.HasValue && (string.IsNullOrEmpty(EndLocation) || string.IsNullOrWhiteSpace(EndLocation)))
                            return "End location is required when setting End Date";

                        if (EndDistance > StartDistance && (string.IsNullOrEmpty(EndLocation) || string.IsNullOrWhiteSpace(EndLocation)))
                            return "End location is required when setting End Km";

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
            UOM = 1;
            StartFuelLevel = 8;
            EndFuelLevel = 8;
            RentStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

            if (rent != null)
                Auto.Mapper.Map(rent, this);

            IsChanged = false;
        }

        public override bool Save(DBArchive db)
        {
            CarRentalHistory rent = new CarRentalHistory();

            Auto.Mapper.Map(this, rent);
            db.CarRentalHistories.AddOrUpdate(rent);
            db.SaveChanges();
            Id = rent.Id;
            IsChanged = false;
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
                Auto.Mapper.Map(cr, this);
                return true;
            }

            return false;
        }
    }
}
