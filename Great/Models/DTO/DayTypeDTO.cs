using Great.Models.Database;

namespace Great.Models.DTO
{
    public class DayTypeDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public DayTypeDTO()
        {
        }

        public DayTypeDTO(DayType type)
        {
            Global.Mapper.Map(type, this);
        }
    }
}