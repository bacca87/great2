using Great.Models.Database;

namespace Great.Models.DTO
{
    public class TransferTypeDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public TransferTypeDTO()
        {
        }

        public TransferTypeDTO(TransferType type)
        {
            Global.Mapper.Map(type, this);
        }
    }
}