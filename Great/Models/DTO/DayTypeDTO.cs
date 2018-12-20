using AutoMapper;
using Great.Models.Database;

namespace Great.Models.DTO
{
    public class DayTypeDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public DayTypeDTO() { }

        public DayTypeDTO(DayType type)
        {
            Mapper.Map(type, this);
        }
    }
}
