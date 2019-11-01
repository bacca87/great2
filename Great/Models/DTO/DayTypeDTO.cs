using Great2.Models.Database;

namespace Great2.Models.DTO
{
    public class DayTypeDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public DayTypeDTO() { }

        public DayTypeDTO(DayType type)
        {
            Auto.Mapper.Map(type, this);
        }
    }
}
