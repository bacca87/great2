using AutoMapper;
using Great.Models.Database;

namespace Great.Models.DTO
{
    public class FDLStatusDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public FDLStatusDTO(FDLStatus status)
        {
            Mapper.Map(status, this);
        }
    }
}
