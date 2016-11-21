using Great.DB;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great
{
    public class WorkingDay
    {
        public int WeekNr { get; set; }
        public DateTime Day { get; set; }

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
                        workingTime += (ts.WorkEndTimeAM.Value - ts.WorkStartTimeAM.Value);
                    if (ts.WorkEndTimePM != null && ts.WorkStartTimePM != null)
                        workingTime += (ts.WorkEndTimePM.Value - ts.WorkStartTimePM.Value);
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
                        travelTime += (ts.TravelEndTimeAM.Value - ts.TravelStartTimeAM.Value);
                    if (ts.TravelEndTimePM != null && ts.TravelStartTimePM != null)
                        travelTime += (ts.TravelEndTimePM.Value - ts.TravelStartTimePM.Value);                    
                }

                return travelTime;
            }
        }

        public int Overtime34 { get; set; }
        public int Overtime50 { get; set; }
        public int Overtime85 { get; set; }
        public int Overtime100 { get; set; }

        #region Display Properties
        private static TimeSpan TimeZero = new TimeSpan(0, 0, 0);
        public string Day_Display { get { return Day.ToLongDateString(); } }
        public string TotalTime_Display { get { return TotalTime != TimeZero ? TotalTime.ToString(@"hh\:mm") : ""; } }
        public string WorkingTime_Display { get { return WorkingTime != TimeZero ? WorkingTime.ToString(@"hh\:mm") : ""; } }
        public string TravelTime_Display { get { return TravelTime != TimeZero ? TravelTime.ToString(@"hh\:mm") : ""; } }
        public string Overtime34_Display { get { return Overtime34 > 0 ? Overtime34.ToString() : ""; } }
        public string Overtime50_Display { get { return Overtime50 > 0 ? Overtime50.ToString() : ""; } }
        public string Overtime85_Display { get { return Overtime85 > 0 ? Overtime85.ToString() : ""; } }
        public string Overtime100_Display { get { return Overtime100 > 0 ? Overtime100.ToString() : ""; } }
        #endregion

        public bool HasDetails { get { return Timesheets != null ? Timesheets.Count > 0 : false; } }
        public IList<Timesheet> Timesheets { get; set; }
    }
}
