using Great.Utils.Extensions;
using Itenso.TimePeriod;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Great.Models
{
    public partial class Timesheet : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Converted Properties
        public DateTime Date
        {
            get { return DateTime.Now.FromUnixTimestamp(Timestamp); }
            set { Timestamp = value.ToUnixTimestamp(); }
        }

        public TimeSpan? TravelStartTimeAM_t
        {
            get { return TravelStartTimeAM.HasValue ? TimeSpan.FromSeconds(TravelStartTimeAM.Value) : (TimeSpan?)null; }
            set { TravelStartTimeAM = value.HasValue ? (long?)value?.TotalSeconds : null; }
        }

        public TimeSpan? TravelEndTimeAM_t
        {
            get { return TravelEndTimeAM.HasValue ? TimeSpan.FromSeconds(TravelEndTimeAM.Value) : (TimeSpan?)null; }
            set { TravelEndTimeAM = value.HasValue ? (long?)value?.TotalSeconds : null; }
        }

        public TimeSpan? TravelStartTimePM_t
        {
            get { return TravelStartTimePM.HasValue ? TimeSpan.FromSeconds(TravelStartTimePM.Value) : (TimeSpan?)null; }
            set { TravelStartTimePM = value.HasValue ? (long?)value?.TotalSeconds : null; }
        }

        public TimeSpan? TravelEndTimePM_t
        {
            get { return TravelEndTimePM.HasValue ? TimeSpan.FromSeconds(TravelEndTimePM.Value) : (TimeSpan?)null; }
            set { TravelEndTimePM = value.HasValue ? (long?)value?.TotalSeconds : null; }
        }

        public TimeSpan? WorkStartTimeAM_t
        {
            get { return WorkStartTimeAM.HasValue ? TimeSpan.FromSeconds(WorkStartTimeAM.Value) : (TimeSpan?)null; }
            set { WorkStartTimeAM = value.HasValue ? (long?)value?.TotalSeconds : null; }
        }

        public TimeSpan? WorkEndTimeAM_t
        {
            get { return WorkEndTimeAM.HasValue ? TimeSpan.FromSeconds(WorkEndTimeAM.Value) : (TimeSpan?)null; }
            set { WorkEndTimeAM = value.HasValue ? (long?)value?.TotalSeconds : null; }
        }

        public TimeSpan? WorkStartTimePM_t
        {
            get { return WorkStartTimePM.HasValue ? TimeSpan.FromSeconds(WorkStartTimePM.Value) : (TimeSpan?)null; }
            set { WorkStartTimePM = value.HasValue ? (long?)value?.TotalSeconds : null; }
        }

        public TimeSpan? WorkEndTimePM_t
        {
            get { return WorkEndTimePM.HasValue ? TimeSpan.FromSeconds(WorkEndTimePM.Value) : (TimeSpan?)null; }
            set { WorkEndTimePM = value.HasValue ? (long?)value?.TotalSeconds : null; }
        }
        #endregion

        #region Totals
        public float? TotalTime
        {
            get
            {
                return TimePeriods.GetRoundedTotalDuration();
            }
        }

        public float? WorkTime
        {
            get
            {
                return WorkPeriods.GetRoundedTotalDuration();
            }
        }

        public float? TravelTime
        {
            get
            {
                return TravelPeriods.GetRoundedTotalDuration();
            }
        }
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

        #region Validation
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

                if (TimePeriods == null && FDL == string.Empty)
                    return false;
                
                if (TimePeriods != null && TimePeriods.HasOverlaps())
                    return false;

                return true;
            }
        }
        
        public bool HasOverlaps(IEnumerable<Timesheet> timesheets)
        {
            if (TimePeriods == null)
                return false;

            foreach (Timesheet timesheet in timesheets)
            {
                if (timesheet.TimePeriods == null)
                    continue;

                if (TimePeriods.HasOverlapPeriods(timesheet.TimePeriods))
                    return true;
            }

            return false;
        }        
        #endregion
        
        public Timesheet Clone()
        {
            return new Timesheet()
            {
                Id = Id,
                Timestamp = Timestamp,
                TravelStartTimeAM = TravelStartTimeAM,
                TravelEndTimeAM = TravelEndTimeAM,
                WorkStartTimeAM = WorkStartTimeAM,
                WorkEndTimeAM = WorkEndTimeAM,
                TravelStartTimePM = TravelStartTimePM,
                TravelEndTimePM = TravelEndTimePM,
                WorkStartTimePM = WorkStartTimePM,
                WorkEndTimePM = WorkEndTimePM,
                FDL = FDL
            };
        }
    }
}
