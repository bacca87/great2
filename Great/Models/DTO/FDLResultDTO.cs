using Great2.Models.Database;

namespace Great2.Models.DTO
{
    public class FDLResultDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public FDLResultDTO() { }

        public FDLResultDTO(FDLResult result)
        {
            Global.Mapper.Map(result, this);
        }
    }
}
