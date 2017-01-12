using System;

namespace Great.Models
{
    public partial class Timesheet
    {
        public TimeSpan? TravelStartTimeAM_t
        {
            get { return TravelStartTimeAM.HasValue ? TimeSpan.FromSeconds(TravelStartTimeAM.Value) : (TimeSpan?)null; }
            set { TravelStartTimeAM = (long)value?.TotalSeconds; }
        }

        public TimeSpan? TravelEndTimeAM_t
        {
            get { return TravelEndTimeAM.HasValue ? TimeSpan.FromSeconds(TravelEndTimeAM.Value) : (TimeSpan?)null; }
            set { TravelEndTimeAM = (long)value?.TotalSeconds; }
        }

        public TimeSpan? TravelStartTimePM_t
        {
            get { return TravelStartTimePM.HasValue ? TimeSpan.FromSeconds(TravelStartTimePM.Value) : (TimeSpan?)null; }
            set { TravelStartTimePM = (long)value?.TotalSeconds; }
        }

        public TimeSpan? TravelEndTimePM_t
        {
            get { return TravelEndTimePM.HasValue ? TimeSpan.FromSeconds(TravelEndTimePM.Value) : (TimeSpan?)null; }
            set { TravelEndTimePM = (long)value?.TotalSeconds; }
        }

        public TimeSpan? WorkStartTimeAM_t
        {
            get { return WorkStartTimeAM.HasValue ? TimeSpan.FromSeconds(WorkStartTimeAM.Value) : (TimeSpan?)null; }
            set { WorkStartTimeAM = (long)value?.TotalSeconds; }
        }

        public TimeSpan? WorkEndTimeAM_t
        {
            get { return WorkEndTimeAM.HasValue ? TimeSpan.FromSeconds(WorkEndTimeAM.Value) : (TimeSpan?)null; }
            set { WorkEndTimeAM = (long)value?.TotalSeconds; }
        }

        public TimeSpan? WorkStartTimePM_t
        {
            get { return WorkStartTimePM.HasValue ? TimeSpan.FromSeconds(WorkStartTimePM.Value) : (TimeSpan?)null; }
            set { WorkStartTimePM = (long)value?.TotalSeconds; }
        }

        public TimeSpan? WorkEndTimePM_t
        {
            get { return WorkEndTimePM.HasValue ? TimeSpan.FromSeconds(WorkEndTimePM.Value) : (TimeSpan?)null; }
            set { WorkEndTimePM = (long)value?.TotalSeconds; }
        }


        #region Display Properties        
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
