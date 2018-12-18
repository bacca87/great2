using AutoMapper;
using Great.Models.Database;

namespace Great.Models.DTO
{
    public class FDLResultDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public FDLResultDTO() { }

        public FDLResultDTO(FDLResult result)
        {
            Mapper.Map(result, this);
        }
    }
}
