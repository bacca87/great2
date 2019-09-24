using Great.Models.Database;

namespace Great.Models.DTO
{
    public class EventDTO
    {
        public long Id { get; set; }
        public long SharepointId { get; set; }
        public int Type { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public long StartDateTimeStamp { get; set; }
        public long EndDateTimeStamp { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public bool IsAllDay { get; set; }
        public bool IsSent { get; set; }

        public EventStatusDTO Status1 { get; set; }
        public EventTypeDTO Type1 { get; set; }
        public EEventStatus EStatus { get; set; }

        public EventDTO(Event ev)
        {
            Global.Mapper.Map(ev, this);
        }
    }
}