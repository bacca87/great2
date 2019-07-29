using Great.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models.DTO
{
    public class EventStatusDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public EventStatusDTO() { }

        public EventStatusDTO(EventStatus status)
        {
            Global.Mapper.Map(status, this);
        }
    }
}
