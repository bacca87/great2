using Great2.Models.Database;

namespace Great2.Models.DTO
{
    public class CurrencyDTO
    {
        public string Id { get; set; }
        public string Description { get; set; }

        public CurrencyDTO() { }

        public CurrencyDTO(Currency currency)
        {
            Global.Mapper.Map(currency, this);
        }
    }
}
