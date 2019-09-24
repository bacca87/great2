using Great.Models.Database;
using System;

namespace Great.Models.DTO
{
    public class TimesheetDTO
    {
        public long Id { get; set; }
        public long Timestamp { get; set; }
        public bool IsValid { get; set; }

        public DateTime Date { get; set; }
        public TimeSpan? TravelStartTimeAM_t { get; set; }
        public TimeSpan? TravelEndTimeAM_t { get; set; }
        public TimeSpan? TravelStartTimePM_t { get; set; }
        public TimeSpan? TravelEndTimePM_t { get; set; }
        public TimeSpan? WorkStartTimeAM_t { get; set; }
        public TimeSpan? WorkEndTimeAM_t { get; set; }
        public TimeSpan? WorkStartTimePM_t { get; set; }
        public TimeSpan? WorkEndTimePM_t { get; set; }

        public TimesheetDTO()
        {
        }

        public TimesheetDTO(Timesheet timesheet)
        {
            Global.Mapper.Map(timesheet, this);
        }
    }
}