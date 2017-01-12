using System;

namespace Great.Models
{
    public partial class Timesheet
    {
        public TimeSpan TravelStartTimeAM_t
        {
            get { return TimeSpan.FromSeconds(TravelStartTimeAM ?? 0); }
            set { TravelStartTimeAM = (long)value.TotalSeconds; }
        }

        public TimeSpan TravelEndTimeAM_t
        {
            get { return TimeSpan.FromSeconds(TravelEndTimeAM ?? 0); }
            set { TravelEndTimeAM = (long)value.TotalSeconds; }
        }

        public TimeSpan TravelStartTimePM_t
        {
            get { return TimeSpan.FromSeconds(TravelStartTimePM ?? 0); }
            set { TravelStartTimePM = (long)value.TotalSeconds; }
        }

        public TimeSpan TravelEndTimePM_t
        {
            get { return TimeSpan.FromSeconds(TravelEndTimePM ?? 0); }
            set { TravelEndTimePM = (long)value.TotalSeconds; }
        }

        public TimeSpan WorkStartTimeAM_t
        {
            get { return TimeSpan.FromSeconds(WorkStartTimeAM ?? 0); }
            set { WorkStartTimeAM = (long)value.TotalSeconds; }
        }

        public TimeSpan WorkEndTimeAM_t
        {
            get { return TimeSpan.FromSeconds(WorkEndTimeAM ?? 0); }
            set { WorkEndTimeAM = (long)value.TotalSeconds; }
        }

        public TimeSpan WorkStartTimePM_t
        {
            get { return TimeSpan.FromSeconds(WorkStartTimePM ?? 0); }
            set { WorkStartTimePM = (long)value.TotalSeconds; }
        }

        public TimeSpan WorkEndTimePM_t
        {
            get { return TimeSpan.FromSeconds(WorkEndTimePM ?? 0); }
            set { WorkEndTimePM = (long)value.TotalSeconds; }
        }


        #region Display Properties
        public string TravelStartTimeAM_Display { get { return TravelStartTimeAM != null ? TravelStartTimeAM_t.ToString("hh\\:mm") : ""; } }
        public string TravelEndTimeAM_Display { get { return TravelEndTimeAM != null ? TravelEndTimeAM_t.ToString("hh\\:mm") : ""; } }
        public string TravelStartTimePM_Display { get { return TravelStartTimePM != null ? TravelStartTimePM_t.ToString("hh\\:mm") : ""; } }
        public string TravelEndTimePM_Display { get { return TravelEndTimePM != null ? TravelEndTimePM_t.ToString("hh\\:mm") : ""; } }
        public string WorkStartTimeAM_Display { get { return WorkStartTimeAM != null ? WorkStartTimeAM_t.ToString("hh\\:mm") : ""; } }
        public string WorkEndTimeAM_Display { get { return WorkEndTimeAM != null ? WorkEndTimeAM_t.ToString("hh\\:mm") : ""; } }
        public string WorkStartTimePM_Display { get { return WorkStartTimePM != null ? WorkStartTimePM_t.ToString("hh\\:mm") : ""; } }
        public string WorkEndTimePM_Display { get { return WorkEndTimePM != null ? WorkEndTimePM_t.ToString("hh\\:mm") : ""; } }
        public string FDL_Display { get { return "TODO"; } }
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
