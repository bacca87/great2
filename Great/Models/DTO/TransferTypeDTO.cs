using Great2.Models.Database;

namespace Great2.Models.DTO
{
    public class TransferTypeDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public TransferTypeDTO() { }

        public TransferTypeDTO(TransferType type)
        {
            Global.Mapper.Map(type, this);
        }
    }
}
