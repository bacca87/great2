using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("Event")]
    public partial class Event
    {
        public Event()
        {

        }

        public long Id { get; set; }
        public long SharepointId { get; set; }
        public long Type { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public DateTime? SendDateTime { get; set; }
        public long StartDateTimeStamp { get; set; }
        public long EndDateTimeStamp { get; set; }
        public string Description { get; set; }
        public long Status { get; set; }
        public bool IsSent { get; set; }
        public bool IsAllDay { get; set; }
        public string Approver { get; set; }
        public DateTime? ApprovationDate { get; set; }

        [ForeignKey("Status")]
        public virtual EventStatus Status1 { get; set; }

        [ForeignKey("Type")]
        public virtual EventType Type1 { get; set; }

    }
}
