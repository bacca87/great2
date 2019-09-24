using Great.Models.Database;

namespace Great.Models.DTO
{
    public class CurrencyDTO
    {
        public string Id { get; set; }
        public string Description { get; set; }

        public CurrencyDTO()
        {
        }

        public CurrencyDTO(Currency currency)
        {
            Global.Mapper.Map(currency, this);
        }
    }
}