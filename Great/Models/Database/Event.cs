using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models.Database
{
    [Table("Event")]
    public partial class Event
    {
        public Event()
        {
           this.Days = new HashSet<Day>();
        }

        public long Id { get; set; }
        public long SharepointId { get; set; }
        public long Type { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public long StartDateTimeStamp { get; set; }
        public long EndDateTimeStamp { get; set; }
        public string Description { get; set; }
        public long Status { get; set; }
        public bool IsAllDay { get; set; }
        public string Approver { get; set; }
        public DateTime? ApprovationDate { get; set; }

        [ForeignKey("Status")]
        public virtual EventStatus Status1 { get; set; }
        [ForeignKey("Type")]
        public virtual EventType Type1 { get; set; }
        public virtual ICollection<Day> Days { get; set; }

    }
}
