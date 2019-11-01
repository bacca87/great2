using Great2.Models.Database;

namespace Great2.Models.DTO
{
    public class EventTypeDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public EventTypeDTO() { }

        public EventTypeDTO(EventType ev)
        {
            Auto.Mapper.Map(ev, this);
        }
    }
}
