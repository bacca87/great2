using Great.Utils;
using System;
using System.Collections.Generic;

namespace Great.Models
{
    public class WorkingDay
    {
        public int WeekNr { get; set; }
        public DateTime Day { get; set; }
        public long Timestamp { get { return UnixTimestamp.GetTimestamp(Day); } }

        public TimeSpan? TotalTime
        {
            get
            {
                TimeSpan totalTime = WorkingTime ?? new TimeSpan() + TravelTime ?? new TimeSpan();
                return totalTime.Ticks > 0 ? totalTime : (TimeSpan?)null;
            }
        }

        public TimeSpan? WorkingTime 
        {
            get
            {
                TimeSpan workingTime = new TimeSpan();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                {   
                    if (ts.WorkEndTimeAM_t.HasValue && ts.WorkStartTimeAM_t.HasValue)
                        workingTime += (ts.WorkEndTimeAM_t.Value - ts.WorkStartTimeAM_t.Value);
                    if (ts.WorkEndTimePM_t.HasValue && ts.WorkStartTimePM_t.HasValue)
                        workingTime += (ts.WorkEndTimePM_t.Value - ts.WorkStartTimePM_t.Value);
                }

                return workingTime.Ticks > 0 ? workingTime : (TimeSpan?)null;
            }
        }

        public TimeSpan? TravelTime
        {
            get
            {
                TimeSpan travelTime = new TimeSpan();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                {
                    if (ts.TravelEndTimeAM_t.HasValue && ts.TravelStartTimeAM_t.HasValue)
                        travelTime += (ts.TravelEndTimeAM_t.Value - ts.TravelStartTimeAM_t.Value);
                    if (ts.TravelEndTimePM_t.HasValue && ts.TravelStartTimePM_t.HasValue)
                        travelTime += (ts.TravelEndTimePM_t.Value - ts.TravelStartTimePM_t.Value);                    
                }

                return travelTime.Ticks > 0 ? travelTime : (TimeSpan?)null;
            }
        }

        public TimeSpan? Overtime34 
        {
            get 
            {
                TimeSpan overtime34 = new TimeSpan();

                if (Day.DayOfWeek == DayOfWeek.Saturday)
                {
                    if (TotalTime?.Hours > 4)
                        overtime34 = TimeSpan.Parse("04:00:00");
                    else
                        overtime34 = TotalTime ?? new TimeSpan();
                }
                else
                {
                    if (TotalTime?.Hours > 8)
                    {
                        if (TotalTime?.Hours >= 10)
                            overtime34 = TimeSpan.Parse("02:00:00");
                        else
                            overtime34 = TotalTime ?? new TimeSpan() - TimeSpan.Parse("08:00:00");
                    }
                }

                return overtime34.Ticks > 0 ? overtime34 : (TimeSpan?)null;
            }
        }

        public TimeSpan? Overtime35 { get; set; } //TODO

        public TimeSpan? Overtime50
        {
            get
            {
                TimeSpan overtime50 = new TimeSpan();

                if (Day.DayOfWeek == DayOfWeek.Saturday && TotalTime?.Hours > 4)
                {
                    overtime50 = TotalTime ?? new TimeSpan() - TimeSpan.Parse("04:00:00");
                }
                else
                {
                    if (TotalTime?.Hours > 10)
                    {
                        overtime50 = TotalTime ?? new TimeSpan() - TimeSpan.Parse("10:00:00");
                    }
                }

                return overtime50.Ticks > 0 ? overtime50 : (TimeSpan?)null;
            }
        }
        
        public TimeSpan? Overtime100
        {
            get
            {
                TimeSpan overtime100 = new TimeSpan();
                
                if (Day.DayOfWeek == DayOfWeek.Sunday) //TODO: aggiungere festivi
                {
                    overtime100 = TotalTime ?? new TimeSpan();
                }

                return overtime100.Ticks > 0 ? overtime100 : (TimeSpan?)null;
            }
        }

        public bool HasDetails { get { return Timesheets != null ? Timesheets.Count > 0 : false; } }
        public IList<Timesheet> Timesheets { get; set; }

        #region Display Properties        
        public string WeekNr_Display { get { return Day.DayOfWeek == DayOfWeek.Monday || Day.Day == 1 ? WeekNr.ToString() : ""; } }        
        #endregion
    }
}
