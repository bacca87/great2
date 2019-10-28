using System.ComponentModel.DataAnnotations.Schema;

namespace Great2.Models.Database
{
    [Table("Timesheet")]
    public partial class Timesheet
    {
        public long Id { get; set; }
        public long Timestamp { get; set; }
        public long? TravelStartTimeAM { get; set; }
        public long? TravelEndTimeAM { get; set; }
        public long? TravelStartTimePM { get; set; }
        public long? TravelEndTimePM { get; set; }
        public long? WorkStartTimeAM { get; set; }
        public long? WorkEndTimeAM { get; set; }
        public long? WorkStartTimePM { get; set; }
        public long? WorkEndTimePM { get; set; }
        public string FDL { get; set; }
        public string Notes { get; set; }

        [ForeignKey("Timestamp")]
        public virtual Day Day { get; set; }
        [ForeignKey("FDL")]
        public virtual FDL FDL1 { get; set; }
    }
}
