using Great.Utils;
using Itenso.TimePeriod;
using System;

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
                
                if (WorkEndTimeAM_t.HasValue && WorkStartTimeAM_t.HasValue)
                    workingPeriods.Add(new TimeRange(Date + WorkStartTimeAM_t.Value, Date + WorkEndTimeAM_t.Value));
                if (WorkEndTimePM_t.HasValue && WorkStartTimePM_t.HasValue)
                    workingPeriods.Add(new TimeRange(Date + WorkStartTimePM_t.Value, Date + WorkEndTimePM_t.Value));
                
                return workingPeriods.Count > 0 ? workingPeriods : null;
            }
        }

        public TimePeriodCollection TravelPeriods
        {
            get
            {
                TimePeriodCollection travelPeriods = new TimePeriodCollection();

                if (TravelStartTimeAM_t.HasValue && TravelEndTimeAM_t.HasValue && !WorkStartTimeAM_t.HasValue && !WorkEndTimeAM_t.HasValue)
                    travelPeriods.Add(new TimeRange(Date + TravelStartTimeAM_t.Value, Date + TravelEndTimeAM_t.Value));
                if (TravelStartTimeAM_t.HasValue && !TravelEndTimeAM_t.HasValue && WorkStartTimeAM_t.HasValue && WorkEndTimeAM_t.HasValue)
                    travelPeriods.Add(new TimeRange(Date + TravelStartTimeAM_t.Value, Date + WorkStartTimeAM_t.Value));
                if (!TravelStartTimeAM_t.HasValue && TravelEndTimeAM_t.HasValue && WorkStartTimeAM_t.HasValue && WorkEndTimeAM_t.HasValue)
                    travelPeriods.Add(new TimeRange(Date + WorkEndTimeAM_t.Value, Date + TravelEndTimeAM_t.Value));

                if (TravelStartTimePM_t.HasValue && TravelEndTimePM_t.HasValue && !WorkStartTimePM_t.HasValue && !WorkEndTimePM_t.HasValue)
                    travelPeriods.Add(new TimeRange(Date + TravelStartTimePM_t.Value, Date + TravelEndTimePM_t.Value));
                if (TravelStartTimePM_t.HasValue && !TravelEndTimePM_t.HasValue && WorkStartTimePM_t.HasValue && WorkEndTimePM_t.HasValue)
                    travelPeriods.Add(new TimeRange(Date + TravelStartTimePM_t.Value, Date + WorkStartTimePM_t.Value));
                if (!TravelStartTimePM_t.HasValue && TravelEndTimePM_t.HasValue && WorkStartTimePM_t.HasValue && WorkEndTimePM_t.HasValue)
                    travelPeriods.Add(new TimeRange(Date + WorkEndTimePM_t.Value, Date + TravelEndTimePM_t.Value));
                
                return travelPeriods.Count > 0 ? travelPeriods : null;
            }
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
}
