using Great2.Models.Database;

namespace Great2.Models.DTO
{
    public class UOMDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }

        public UOMDTO() { }

        public UOMDTO(UOM uom)
        {
            Auto.Mapper.Map(uom, this);
        }
    }
}
