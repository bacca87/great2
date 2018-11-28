using AutoMapper;
using Great.Models.Database;

namespace Great.Models.DTO
{
    public class FDLDTO
    {
        public string Id { get; set; }
        public long WeekNr { get; set; }
        public long Order { get; set; }

        public FDLDTO(FDL fdl)
        {
            Mapper.Map(fdl, this);
        }
    }
}
