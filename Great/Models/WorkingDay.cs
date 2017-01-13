using Great.Utils;
using Itenso.TimePeriod;
using System;
using System.Collections.Generic;

namespace Great.Models
{
    public class WorkingDay
    {
        public int WeekNr { get; set; }
        public DateTime Day { get; set; }
        public long Timestamp { get { return UnixTimestamp.GetTimestamp(Day); } }

        public bool HasDetails { get { return Timesheets != null ? Timesheets.Count > 0 : false; } }
        public IList<Timesheet> Timesheets { get; set; }

        #region Time Periods
        public TimePeriodCollection TimePeriods
        {
            get
            {
                TimePeriodCollection timePeriods = new TimePeriodCollection();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    if(ts.TimePeriods != null)
                        timePeriods.AddAll(ts.TimePeriods);

                return timePeriods.Count > 0 ? timePeriods : null;
            }
        }
        
        public TimePeriodCollection WorkingPeriods
        {
            get
            {
                TimePeriodCollection workingPeriods = new TimePeriodCollection();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    if(ts.WorkingPeriods != null)
                        workingPeriods.AddAll(ts.WorkingPeriods);

                return workingPeriods.Count > 0 ? workingPeriods : null;
            }
        }

        public TimePeriodCollection TravelPeriods
        {
            get
            {
                TimePeriodCollection travelPeriods = new TimePeriodCollection();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    if(ts.TravelPeriods != null)
                        travelPeriods.AddAll(ts.TravelPeriods);

                return travelPeriods.Count > 0 ? travelPeriods : null;
            }
        }
        #endregion

        #region Overtimes
        public TimeSpan? Overtime34 
        {
            get 
            {
                TimeSpan overtime34 = new TimeSpan();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    overtime34 += ts.Overtime34 ?? new TimeSpan();

                return overtime34.Ticks > 0 ? overtime34 : (TimeSpan?)null;
            }
        }

        public TimeSpan? Overtime35
        {
            get
            {
                TimeSpan overtime35 = new TimeSpan();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    overtime35 += ts.Overtime35 ?? new TimeSpan();

                return overtime35.Ticks > 0 ? overtime35 : (TimeSpan?)null;
            }
        }

        public TimeSpan? Overtime50
        {
            get
            {
                TimeSpan overtime50 = new TimeSpan();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    overtime50 += ts.Overtime50 ?? new TimeSpan();

                return overtime50.Ticks > 0 ? overtime50 : (TimeSpan?)null;
            }
        }
        
        public TimeSpan? Overtime100
        {
            get
            {
                TimeSpan overtime100 = new TimeSpan();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    overtime100 += ts.Overtime100 ?? new TimeSpan();

                return overtime100.Ticks > 0 ? overtime100 : (TimeSpan?)null;
            }
        }
        #endregion

        #region Display Properties        
        public string WeekNr_Display { get { return Day.DayOfWeek == DayOfWeek.Monday || Day.Day == 1 ? WeekNr.ToString() : ""; } }        
        #endregion
    }
}
