
using Great2.Models.Database;

namespace Great2.Models.DTO
{
    public class DayDTO
    {
        public long Timestamp { get; set; }
        public long Type { get; set; }

        public DayTypeDTO DayType { get; set; }

        public DayDTO() { }

        public DayDTO(Day day)
        {
            Global.Mapper.Map(day, this);
        }
    }
}
