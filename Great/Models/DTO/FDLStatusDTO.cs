using Great2.Models.Database;

namespace Great2.Models.DTO
{
    public class FDLStatusDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public FDLStatusDTO() { }

        public FDLStatusDTO(FDLStatus status)
        {
            Auto.Mapper.Map(status, this);
        }
    }
}
