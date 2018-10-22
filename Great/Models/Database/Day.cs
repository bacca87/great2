using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models
{
    [Table("Day")]
    public partial class Day
    {
        public Day()
        {
            this.Timesheets = new HashSet<Timesheet>();
        }

        [Key]
        public long Timestamp { get; set; }
        public long Type { get; set; }

        [ForeignKey("Type")]
        public virtual DayType DayType { get; set; }
        public virtual ICollection<Timesheet> Timesheets { get; set; }
    }
}
