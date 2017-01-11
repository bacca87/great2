using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models
{
    public partial class Timesheet
    {
        #region Display Properties
        public string TravelStartTimeAM_Display { get { return TravelStartTimeAM != null ? TravelStartTimeAM.Value.ToShortTimeString() : ""; } }
        public string TravelEndTimeAM_Display { get { return TravelEndTimeAM != null ? TravelEndTimeAM.Value.ToShortTimeString() : ""; } }
        public string TravelStartTimePM_Display { get { return TravelStartTimePM != null ? TravelStartTimePM.Value.ToShortTimeString() : ""; } }
        public string TravelEndTimePM_Display { get { return TravelEndTimePM != null ? TravelEndTimePM.Value.ToShortTimeString() : ""; } }
        public string WorkStartTimeAM_Display { get { return WorkStartTimeAM != null ? WorkStartTimeAM.Value.ToShortTimeString() : ""; } }
        public string WorkEndTimeAM_Display { get { return WorkEndTimeAM != null ? WorkEndTimeAM.Value.ToShortTimeString() : ""; } }
        public string WorkStartTimePM_Display { get { return WorkStartTimePM != null ? WorkStartTimePM.Value.ToShortTimeString() : ""; } }
        public string WorkEndTimePM_Display { get { return WorkEndTimePM != null ? WorkEndTimePM.Value.ToShortTimeString() : ""; } }
        public string FDL_Display { get { return "TODO"; } }
        #endregion

        public Timesheet Clone()
        {
            return new Timesheet()
            {
                Id = this.Id,
                Date = this.Date,
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
