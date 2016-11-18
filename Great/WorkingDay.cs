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
        public TimeSpan WorkingTime 
        {
            get
            {
                TimeSpan totalTime = new TimeSpan(0,0,0);

                if (Timesheets == null || Timesheets.Count == 0)
                    return totalTime;

                foreach(Timesheet ts in Timesheets)
                {
                    totalTime += (ts.TravelEndTimeAM.Value - ts.TravelStartTimeAM.Value) +
                                 (ts.TravelEndTimePM.Value - ts.TravelStartTimePM.Value) +
                                 (ts.WorkEndTimeAM.Value - ts.WorkStartTimeAM.Value) +
                                 (ts.WorkEndTimePM.Value - ts.WorkStartTimePM.Value);
                }

                return totalTime;
            }
        }
        public TimeSpan TravelTime { get; set; }
        public int Overtime34 { get; set; }
        public int Overtime50 { get; set; }
        public int Overtime85 { get; set; }
        public int Overtime100 { get; set; }

        #region Display Properties
        public string DayString { get { return Day.ToLongDateString(); } }
        public string WorkingTimeString { get { return WorkingTime.ToString(@"hh\:mm"); } }
        public string TravelTimeString { get { return TravelTime.ToString(@"hh\:mm"); } }
        #endregion

        public IList<Timesheet> Timesheets { get; set; }
    }
}
