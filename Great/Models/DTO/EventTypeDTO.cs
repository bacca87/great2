using Great.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models.DTO
{
    public class EventTypeDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public EventTypeDTO() { }

        public EventTypeDTO(EventType ev)
        {
            Global.Mapper.Map(ev, this);
        }
    }
}
