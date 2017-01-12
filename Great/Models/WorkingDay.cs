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

        public TimeSpan TotalTime
        {
            get
            {
                return WorkingTime + TravelTime;
            }
        }

        public TimeSpan WorkingTime 
        {
            get
            {
                TimeSpan workingTime = new TimeSpan(0, 0, 0);

                if (Timesheets == null || Timesheets.Count == 0)
                    return workingTime;

                foreach (Timesheet ts in Timesheets)
                {   
                    if (ts.WorkEndTimeAM != null && ts.WorkStartTimeAM != null)
                        workingTime += (ts.WorkEndTimeAM_t - ts.WorkStartTimeAM_t);
                    if (ts.WorkEndTimePM != null && ts.WorkStartTimePM != null)
                        workingTime += (ts.WorkEndTimePM_t - ts.WorkStartTimePM_t);
                }

                return workingTime;
            }
        }

        public TimeSpan TravelTime
        {
            get
            {
                TimeSpan travelTime = new TimeSpan(0, 0, 0);

                if (Timesheets == null || Timesheets.Count == 0)
                    return travelTime;

                foreach (Timesheet ts in Timesheets)
                {
                    if (ts.TravelEndTimeAM != null && ts.TravelStartTimeAM != null)
                        travelTime += (ts.TravelEndTimeAM_t - ts.TravelStartTimeAM_t);
                    if (ts.TravelEndTimePM != null && ts.TravelStartTimePM != null)
                        travelTime += (ts.TravelEndTimePM_t - ts.TravelStartTimePM_t);                    
                }

                return travelTime;
            }
        }

        public TimeSpan Overtime34 
        {
            get 
            {
                TimeSpan overtime34 = new TimeSpan(0,0,0);

                if (Day.DayOfWeek == DayOfWeek.Saturday)
                {
                    if (TotalTime.Hours > 4)
                        overtime34 = TimeSpan.Parse("04:00:00");
                    else
                        overtime34 = TotalTime;
                }
                else
                {
                    if (TotalTime.Hours > 8)
                    {
                        if (TotalTime.Hours >= 10)
                            overtime34 = TimeSpan.Parse("02:00:00");
                        else
                            overtime34 = TotalTime - TimeSpan.Parse("08:00:00");
                    }
                }

                return overtime34;
            }
        }

        public TimeSpan Overtime35 { get; set; } //TODO

        public TimeSpan Overtime50
        {
            get
            {
                TimeSpan overtime50 = new TimeSpan(0, 0, 0);

                if (Day.DayOfWeek == DayOfWeek.Saturday && TotalTime.Hours > 4)
                {
                    overtime50 = TotalTime - TimeSpan.Parse("04:00:00");
                }
                else
                {
                    if (TotalTime.Hours > 10)
                    {
                        overtime50 = TotalTime - TimeSpan.Parse("10:00:00");
                    }
                }

                return overtime50;
            }
        }
        
        public TimeSpan Overtime100
        {
            get
            {
                TimeSpan overtime100 = new TimeSpan(0, 0, 0);
                
                if (Day.DayOfWeek == DayOfWeek.Sunday) //TODO: aggiungere festivi
                {
                    overtime100 = TotalTime;
                }

                return overtime100;
            }
        }

        public bool HasDetails { get { return Timesheets != null ? Timesheets.Count > 0 : false; } }
        public IList<Timesheet> Timesheets { get; set; }

        #region Display Properties
        private static TimeSpan TimeZero = new TimeSpan(0, 0, 0);

        public string WeekNr_Display { get { return Day.DayOfWeek == DayOfWeek.Monday || Day.Day == 1 ? WeekNr.ToString() : ""; } }
        public string Day_Display { get { return Day.ToLongDateString(); } }
        public string TotalTime_Display { get { return TotalTime != TimeZero ? TotalTime.ToString(@"hh\:mm") : ""; } }
        public string WorkingTime_Display { get { return WorkingTime != TimeZero ? WorkingTime.ToString(@"hh\:mm") : ""; } }
        public string TravelTime_Display { get { return TravelTime != TimeZero ? TravelTime.ToString(@"hh\:mm") : ""; } }
        public string Overtime34_Display { get { return Overtime34 != TimeZero ? Overtime34.ToString(@"hh\:mm") : ""; } }
        public string Overtime35_Display { get { return Overtime35 != TimeZero ? Overtime35.ToString(@"hh\:mm") : ""; } }
        public string Overtime50_Display { get { return Overtime50 != TimeZero ? Overtime50.ToString(@"hh\:mm") : ""; } }
        public string Overtime100_Display { get { return Overtime100 != TimeZero ? Overtime100.ToString(@"hh\:mm") : ""; } }
        #endregion
    }
}
