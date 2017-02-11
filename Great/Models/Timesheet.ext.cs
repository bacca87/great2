using Great.Utils;
using Itenso.TimePeriod;
using System;
using System.Collections.Generic;

namespace Great.Models
{
    public partial class Timesheet
    {
        #region Converted Properties
        public DateTime Date
        {
            get { return UnixTimestamp.GetDateTime(Timestamp); }
            set { Timestamp = UnixTimestamp.GetTimestamp(value); }
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
                return GetRoundedTotalDuration(TimePeriods);
            }
        }

        public float? WorkingTime
        {
            get
            {
                return GetRoundedTotalDuration(WorkingPeriods);
            }
        }

        public float? TravelTime
        {
            get
            {
                return GetRoundedTotalDuration(TravelPeriods);
            }
        }

        RoundByQuarterHourDurationProvider roundByQuarterHour = new RoundByQuarterHourDurationProvider();

        private float? GetRoundedTotalDuration(TimePeriodCollection periods)
        {
            if (periods == null || periods.Count == 0)
                return null;

            TimeSpan totalDuration = periods.GetTotalDuration(roundByQuarterHour);
            float total = totalDuration.Hours + (totalDuration.Minutes / 100);

            return total > 0 ? total : 24;
        }
        #endregion

        #region Time Periods
        public TimePeriodCollection TimePeriods
        {
            get
            {
                TimePeriodCollection timePeriods = new TimePeriodCollection();

                if (WorkingPeriods != null)
                    timePeriods.AddAll(WorkingPeriods);

                if (TravelPeriods != null)
                    timePeriods.AddAll(TravelPeriods);

                return timePeriods.Count > 0 ? timePeriods : null;
            }
        }

        public TimePeriodCollection WorkingPeriods
        {
            get
            {
                TimePeriodCollection workingPeriods = new TimePeriodCollection();
                DateTime start;
                DateTime end;

                if (WorkEndTimeAM_t.HasValue && WorkStartTimeAM_t.HasValue)
                {
                    start = Date + WorkStartTimeAM_t.Value;
                    end = WorkStartTimeAM_t.Value < WorkEndTimeAM_t.Value ? Date + WorkEndTimeAM_t.Value : Date.AddDays(1) + WorkEndTimeAM_t.Value;
                    workingPeriods.Add(new TimeRange(start, end));
                }

                if (WorkEndTimePM_t.HasValue && WorkStartTimePM_t.HasValue)
                {
                    start = Date + WorkStartTimePM_t.Value;
                    end = WorkStartTimePM_t.Value < WorkEndTimePM_t.Value ? Date + WorkEndTimePM_t.Value : Date.AddDays(1) + WorkEndTimePM_t.Value;
                    workingPeriods.Add(new TimeRange(start, end));
                }
                                
                return workingPeriods.Count > 0 ? workingPeriods : null;
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
                Id = this.Id,
                Timestamp = this.Timestamp,
                TravelStartTimeAM = this.TravelStartTimeAM,
                TravelEndTimeAM = this.TravelEndTimeAM,
                WorkStartTimeAM = this.WorkStartTimeAM,
                WorkEndTimeAM = this.WorkEndTimeAM,
                TravelStartTimePM = this.TravelStartTimePM,
                TravelEndTimePM = this.TravelEndTimePM,
                WorkStartTimePM = this.WorkStartTimePM,
                WorkEndTimePM = this.WorkEndTimePM,
                FDL = this.FDL
            };
        }
    }

    public class RoundByQuarterHourDurationProvider : IDurationProvider
    {   
        public virtual TimeSpan GetDuration(DateTime start, DateTime end)
        {
            start = start.Date + TimeSpanExtensions.Round(start.TimeOfDay, RoundingDirection.Down, 15);
            end = end.Date + TimeSpanExtensions.Round(end.TimeOfDay, RoundingDirection.Down, 15);
            return end.Subtract(start);
        }
    }
}
