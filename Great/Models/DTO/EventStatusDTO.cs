using Great2.Models.Database;

namespace Great2.Models.DTO
{
    public class EventStatusDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public EventStatusDTO() { }

        public EventStatusDTO(EventStatus status)
        {
            Auto.Mapper.Map(status, this);
        }
    }
}
