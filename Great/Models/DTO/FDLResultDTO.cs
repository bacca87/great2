using Great.Models.Database;

namespace Great.Models.DTO
{
    public class FDLResultDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public FDLResultDTO()
        {
        }

        public FDLResultDTO(FDLResult result)
        {
            Global.Mapper.Map(result, this);
        }
    }
}