using Great.Models.Database;

namespace Great.Models.DTO
{
    public class FactoryDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public bool OverrideAddressOnFDL { get; set; }

        public FactoryDTO() { }

        public FactoryDTO(Factory factory)
        {
            Global.Mapper.Map(factory, this);
        }
    }
}
