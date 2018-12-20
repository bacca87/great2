using AutoMapper;
using Great.Models.Database;

namespace Great.Models.DTO
{
    public class FDLDTO
    {
        public string Id { get; set; }
        public long WeekNr { get; set; }
        public long StartDay { get; set; }
        public long Order { get; set; }
        public long? Factory { get; set; }

        public FactoryDTO Factory1 { get; set; }

        public FDLDTO() { }

        public FDLDTO(FDL fdl)
        {
            Mapper.Map(fdl, this);
        }
    }
}
