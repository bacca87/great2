using AutoMapper;
using Great.Models.Database;

namespace Great.Models.DTO
{
    public class CurrencyDTO
    {
        public string Id { get; set; }
        public string Description { get; set; }

        public CurrencyDTO(Currency currency)
        {
            Mapper.Map(currency, this);
        }
    }
}
