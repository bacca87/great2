using Great2.Models.Database;
using Great2.Models.DTO;
using Great2.Utils.Extensions;
using Itenso.TimePeriod;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Great2.ViewModels.Database
{
    public class TimesheetEVM : EntityViewModelBase, IDataErrorInfo
    {
        #region Properties
        private long _Id;
        public long Id
        {
            get => _Id;
            set => Set(ref _Id, value);
        }

        private long _Timestamp;
        public long Timestamp
        {
            get => _Timestamp;
            set
            {
                Set(ref _Timestamp, value);
                RaisePropertyChanged(nameof(Date));
            }
        }

        private long? _TravelStartTimeAM;
        public long? TravelStartTimeAM
        {
            get => _TravelStartTimeAM;
            set
            {
                Set(ref _TravelStartTimeAM, value);
                RaisePropertyChanged(nameof(TravelStartTimeAM_t));
                UpdateTotals();
            }
        }

        private long? _TravelEndTimeAM;
        public long? TravelEndTimeAM
        {
            get => _TravelEndTimeAM;
            set
            {
                Set(ref _TravelEndTimeAM, value);
                RaisePropertyChanged(nameof(TravelEndTimeAM_t));
                UpdateTotals();
            }
        }

        private long? _TravelStartTimePM;
        public long? TravelStartTimePM
        {
            get => _TravelStartTimePM;
            set
            {
                Set(ref _TravelStartTimePM, value);
                RaisePropertyChanged(nameof(TravelStartTimePM_t));
                UpdateTotals();
            }
        }

        private long? _TravelEndTimePM;
        public long? TravelEndTimePM
        {
            get => _TravelEndTimePM;
            set
            {
                Set(ref _TravelEndTimePM, value);
                RaisePropertyChanged(nameof(TravelEndTimePM_t));
                UpdateTotals();
            }
        }

        private long? _WorkStartTimeAM;
        public long? WorkStartTimeAM
        {
            get => _WorkStartTimeAM;
            set
            {
                Set(ref _WorkStartTimeAM, value);
                RaisePropertyChanged(nameof(WorkStartTimeAM_t));
                UpdateTotals();
            }
        }

        private long? _WorkEndTimeAM;
        public long? WorkEndTimeAM
        {
            get => _WorkEndTimeAM;
            set
            {
                Set(ref _WorkEndTimeAM, value);
                RaisePropertyChanged(nameof(WorkEndTimeAM_t));
                UpdateTotals();
            }
        }

        private long? _WorkStartTimePM;
        public long? WorkStartTimePM
        {
            get => _WorkStartTimePM;
            set
            {
                Set(ref _WorkStartTimePM, value);
                RaisePropertyChanged(nameof(WorkStartTimePM_t));
                UpdateTotals();
            }
        }

        private long? _WorkEndTimePM;
        public long? WorkEndTimePM
        {
            get => _WorkEndTimePM;
            set
            {
                Set(ref _WorkEndTimePM, value);
                RaisePropertyChanged(nameof(WorkEndTimePM_t));
                UpdateTotals();
            }
        }

        private string _FDL;
        public string FDL
        {
            get => _FDL;
            set => SetAndCheckChanged(ref _FDL, value);
        }

        private string _Notes;
        public string Notes
        {
            get => _Notes;
            set => SetAndCheckChanged(ref _Notes, value);
        }

        private DayDTO _Day;
        public DayDTO Day
        {
            get => _Day;
            set => Set(ref _Day, value);
        }

        private FDLEVM _FDL1;
        public FDLEVM FDL1
        {
            get => _FDL1;
            set => Set(ref _FDL1, value);
        }

        #region Converted Properties
        public DateTime Date
        {
            get => DateTime.Now.FromUnixTimestamp(Timestamp);
            set => Timestamp = value.ToUnixTimestamp();
        }

        public TimeSpan? TravelStartTimeAM_t
        {
            get => TravelStartTimeAM.HasValue ? TimeSpan.FromSeconds(TravelStartTimeAM.Value) : (TimeSpan?)null;
            set => TravelStartTimeAM = value.HasValue ? (long?)value?.TotalSeconds : null;
        }

        public TimeSpan? TravelEndTimeAM_t
        {
            get => TravelEndTimeAM.HasValue ? TimeSpan.FromSeconds(TravelEndTimeAM.Value) : (TimeSpan?)null;
            set => TravelEndTimeAM = value.HasValue ? (long?)value?.TotalSeconds : null;
        }

        public TimeSpan? TravelStartTimePM_t
        {
            get => TravelStartTimePM.HasValue ? TimeSpan.FromSeconds(TravelStartTimePM.Value) : (TimeSpan?)null;
            set => TravelStartTimePM = value.HasValue ? (long?)value?.TotalSeconds : null;
        }

        public TimeSpan? TravelEndTimePM_t
        {
            get => TravelEndTimePM.HasValue ? TimeSpan.FromSeconds(TravelEndTimePM.Value) : (TimeSpan?)null;
            set => TravelEndTimePM = value.HasValue ? (long?)value?.TotalSeconds : null;
        }

        public TimeSpan? WorkStartTimeAM_t
        {
            get => WorkStartTimeAM.HasValue ? TimeSpan.FromSeconds(WorkStartTimeAM.Value) : (TimeSpan?)null;
            set => WorkStartTimeAM = value.HasValue ? (long?)value?.TotalSeconds : null;
        }

        public TimeSpan? WorkEndTimeAM_t
        {
            get => WorkEndTimeAM.HasValue ? TimeSpan.FromSeconds(WorkEndTimeAM.Value) : (TimeSpan?)null;
            set => WorkEndTimeAM = value.HasValue ? (long?)value?.TotalSeconds : null;
        }

        public TimeSpan? WorkStartTimePM_t
        {
            get => WorkStartTimePM.HasValue ? TimeSpan.FromSeconds(WorkStartTimePM.Value) : (TimeSpan?)null;
            set => WorkStartTimePM = value.HasValue ? (long?)value?.TotalSeconds : null;
        }

        public TimeSpan? WorkEndTimePM_t
        {
            get => WorkEndTimePM.HasValue ? TimeSpan.FromSeconds(WorkEndTimePM.Value) : (TimeSpan?)null;
            set => WorkEndTimePM = value.HasValue ? (long?)value?.TotalSeconds : null;
        }
        #endregion

        #region Totals
        public float? TotalTime => TimePeriods.GetRoundedTotalDuration();

        public float? WorkTime => WorkPeriods.GetRoundedTotalDuration();

        public float? TravelTime => TravelPeriods.GetRoundedTotalDuration();
        #endregion

        #region Time Periods
        public TimePeriodCollection TimePeriods
        {
            get
            {
                TimePeriodCollection timePeriods = new TimePeriodCollection();

                if (WorkPeriods != null)
                    timePeriods.AddAll(WorkPeriods);

                if (TravelPeriods != null)
                    timePeriods.AddAll(TravelPeriods);

                return timePeriods.Count > 0 ? timePeriods : null;
            }
        }

        public TimePeriodCollection WorkPeriods
        {
            get
            {
                TimePeriodCollection workPeriods = new TimePeriodCollection();
                DateTime start;
                DateTime end;

                if (WorkEndTimeAM_t.HasValue && WorkStartTimeAM_t.HasValue)
                {
                    start = Date + WorkStartTimeAM_t.Value;
                    end = WorkStartTimeAM_t.Value < WorkEndTimeAM_t.Value ? Date + WorkEndTimeAM_t.Value : Date.AddDays(1) + WorkEndTimeAM_t.Value;
                    workPeriods.Add(new TimeRange(start, end));
                }

                if (WorkEndTimePM_t.HasValue && WorkStartTimePM_t.HasValue)
                {
                    start = Date + WorkStartTimePM_t.Value;
                    end = WorkStartTimePM_t.Value < WorkEndTimePM_t.Value ? Date + WorkEndTimePM_t.Value : Date.AddDays(1) + WorkEndTimePM_t.Value;
                    workPeriods.Add(new TimeRange(start, end));
                }

                return workPeriods.Count > 0 ? workPeriods : null;
            }
        }

        public TimePeriodCollection TravelPeriods
        {
            get
            {
                TimePeriodCollection travelPeriods = new TimePeriodCollection();
                DateTime start;
                DateTime end;

                if (TravelStartTimeAM_t.HasValue && TravelEndTimeAM_t.HasValue && !WorkStartTimeAM_t.HasValue && !WorkEndTimeAM_t.HasValue)
                {
                    start = Date + TravelStartTimeAM_t.Value;
                    end = TravelStartTimeAM_t.Value < TravelEndTimeAM_t.Value ? Date + TravelEndTimeAM_t.Value : Date.AddDays(1) + TravelEndTimeAM_t.Value;
                    travelPeriods.Add(new TimeRange(start, end));
                }
                if (TravelStartTimeAM_t.HasValue && !TravelEndTimeAM_t.HasValue && WorkStartTimeAM_t.HasValue && WorkEndTimeAM_t.HasValue)
                {
                    start = Date + TravelStartTimeAM_t.Value;
                    end = TravelStartTimeAM_t.Value < WorkStartTimeAM_t.Value ? Date + WorkStartTimeAM_t.Value : Date.AddDays(1) + WorkStartTimeAM_t.Value;
                    travelPeriods.Add(new TimeRange(start, end));
                }
                if (!TravelStartTimeAM_t.HasValue && TravelEndTimeAM_t.HasValue && WorkStartTimeAM_t.HasValue && WorkEndTimeAM_t.HasValue)
                {
                    start = Date + WorkEndTimeAM_t.Value;
                    end = WorkEndTimeAM_t.Value < TravelEndTimeAM_t.Value ? Date + TravelEndTimeAM_t.Value : Date.AddDays(1) + TravelEndTimeAM_t.Value;
                    travelPeriods.Add(new TimeRange(start, end));
                }
                if (TravelStartTimePM_t.HasValue && TravelEndTimePM_t.HasValue && !WorkStartTimePM_t.HasValue && !WorkEndTimePM_t.HasValue)
                {
                    start = Date + TravelStartTimePM_t.Value;
                    end = TravelStartTimePM_t.Value < TravelEndTimePM_t.Value ? Date + TravelEndTimePM_t.Value : Date.AddDays(1) + TravelEndTimePM_t.Value;
                    travelPeriods.Add(new TimeRange(start, end));
                }
                if (TravelStartTimePM_t.HasValue && !TravelEndTimePM_t.HasValue && WorkStartTimePM_t.HasValue && WorkEndTimePM_t.HasValue)
                {
                    start = Date + TravelStartTimePM_t.Value;
                    end = TravelStartTimePM_t.Value < WorkStartTimePM_t.Value ? Date + WorkStartTimePM_t.Value : Date.AddDays(1) + WorkStartTimePM_t.Value;
                    travelPeriods.Add(new TimeRange(start, end));
                }
                if (!TravelStartTimePM_t.HasValue && TravelEndTimePM_t.HasValue && WorkStartTimePM_t.HasValue && WorkEndTimePM_t.HasValue)
                {
                    start = Date + WorkEndTimePM_t.Value;
                    end = WorkEndTimePM_t.Value < TravelEndTimePM_t.Value ? Date + TravelEndTimePM_t.Value : Date.AddDays(1) + TravelEndTimePM_t.Value;
                    travelPeriods.Add(new TimeRange(start, end));
                }

                return travelPeriods.Count > 0 ? travelPeriods : null;
            }
        }
        #endregion

        #endregion

        #region Error Validation
        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    //case "WorkStartTimeAM_t":
                    //case "WorkEndTimeAM_t":
                    //    if ((WorkStartTimeAM_t.HasValue && !WorkEndTimeAM_t.HasValue) || (!WorkStartTimeAM_t.HasValue && WorkEndTimeAM_t.HasValue))
                    //        return "Work time AM interval not correct";
                    //    break;

                    //case "WorkStartTimePM_t":
                    //case "WorkEndTimePM_t":
                    //    if ((WorkStartTimePM_t.HasValue && !WorkEndTimePM_t.HasValue) || (!WorkStartTimePM_t.HasValue && WorkEndTimePM_t.HasValue))
                    //        return "Work time PM interval not correct";
                    //    break;

                    //case "TravelStartTimeAM_t":
                    //case "TravelEndTimeAM_t":
                    //    if ((TravelStartTimeAM_t.HasValue && !WorkStartTimeAM_t.HasValue && !TravelEndTimeAM_t.HasValue) ||
                    //        (TravelEndTimeAM_t.HasValue && !WorkEndTimeAM_t.HasValue && !TravelStartTimeAM_t.HasValue))
                    //        return "Travel time AM interval not correct";
                    //    break;

                    //case "TravelStartTimePM_t":
                    //case "TravelEndTimePM_t":
                    //    if ((TravelStartTimePM_t.HasValue && !WorkStartTimePM_t.HasValue && !TravelEndTimePM_t.HasValue) ||
                    //        (TravelEndTimePM_t.HasValue && !WorkEndTimePM_t.HasValue && !TravelStartTimePM_t.HasValue))
                    //        return "Travel time PM interval not correct";
                    //    break;
                    default:

                        break;
                }

                return null;
            }
        }

        public bool IsValid
        {
            get
            {
                if ((WorkStartTimeAM_t.HasValue && !WorkEndTimeAM_t.HasValue) || (!WorkStartTimeAM_t.HasValue && WorkEndTimeAM_t.HasValue))
                    return false;

                if ((WorkStartTimePM_t.HasValue && !WorkEndTimePM_t.HasValue) || (!WorkStartTimePM_t.HasValue && WorkEndTimePM_t.HasValue))
                    return false;

                if ((TravelStartTimeAM_t.HasValue && !WorkStartTimeAM_t.HasValue && !TravelEndTimeAM_t.HasValue) ||
                    (TravelEndTimeAM_t.HasValue && !WorkEndTimeAM_t.HasValue && !TravelStartTimeAM_t.HasValue))
                    return false;

                if ((TravelStartTimePM_t.HasValue && !WorkStartTimePM_t.HasValue && !TravelEndTimePM_t.HasValue) ||
                    (TravelEndTimePM_t.HasValue && !WorkEndTimePM_t.HasValue && !TravelStartTimePM_t.HasValue))
                    return false;

                if (TimePeriods == null && FDL == null && Notes == null)
                    return false;

                if (TimePeriods != null && TimePeriods.HasOverlaps())
                    return false;

                return true;
            }
        }

        public bool HasOverlaps(IEnumerable<TimesheetEVM> timesheets)
        {
            if (TimePeriods == null)
                return false;

            foreach (TimesheetEVM timesheet in timesheets)
            {
                if (timesheet.TimePeriods == null)
                    continue;

                if (TimePeriods.HasOverlapPeriods(timesheet.TimePeriods))
                    return true;
                }

            return false;
        }
        #endregion

        public TimesheetEVM(Timesheet timesheet = null)
        {
            if (timesheet != null)
                Global.Mapper.Map(timesheet, this);
            IsChanged = false;
        }

        public override bool Save(DBArchive db)
        {
            Timesheet timesheet = new Timesheet();

            Global.Mapper.Map(this, timesheet);
            db.Timesheets.AddOrUpdate(timesheet);
            db.SaveChanges();
            Id = timesheet.Id;
            return true;
        }

        public override bool Delete(DBArchive db)
        {
            if (Id > 0)
            {
                db.Timesheets.Remove(db.Timesheets.SingleOrDefault(t => t.Id == Id));
                db.SaveChanges();
                Id = 0;
                return true;
            }
            return false;
        }

        public override bool Refresh(DBArchive db)
        {
            var timesheet = db.Timesheets.SingleOrDefault(t => t.Id == Id);
            db.Entry(timesheet).Reference(p => p.FDL1).Load();

            if (timesheet != null)
                return Global.Mapper.Map(timesheet, this) != null;

            return false;
        }

        private void UpdateTotals()
        {
            RaisePropertyChanged(nameof(TotalTime));
            RaisePropertyChanged(nameof(WorkTime));
            RaisePropertyChanged(nameof(TravelTime));
        }

    }
}
