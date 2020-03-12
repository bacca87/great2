using Great2.Models.Database;
using System;

namespace Great2.Models.DTO
{
    public class FDLDTO
    {
        public string Id { get; set; }
        public long WeekNr { get; set; }
        public long StartDay { get; set; }
        public DateTime StartDayDate { get; set; }
        public long Order { get; set; }
        public long? Factory { get; set; }
        public bool IsVirtual { get; set; }

        public FactoryDTO Factory1 { get; set; }

        public FDLDTO() { }

        public FDLDTO(FDL fdl)
        {
            Auto.Mapper.Map(fdl, this);
        }
    }
}
