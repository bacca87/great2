using Great.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models.DTO
{
    public class EventDTO
    {
        public long Id { get; set; }
        public long SharepointId { get; set; }
        public int Type { get; set; }
        public string Location { get; set; }
        public long StartDateTimeStamp { get; set; }
        public long EndDateTimeStamp { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public bool IsAllDay { get; set; }

        public EventStatusDTO Status1 { get; set; }
        public EventTypeDTO Type1 { get; set; }

        public EventDTO(Event ev)
        {
            Global.Mapper.Map(ev, this);
        }
    }
}
