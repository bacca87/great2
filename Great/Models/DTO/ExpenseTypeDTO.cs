using AutoMapper;
using Great.Models.Database;

namespace Great.Models.DTO
{
    public class ExpenseTypeDTO
    {
        public long Id { get; set; }
        public string Description { get; set; }

        public ExpenseTypeDTO(ExpenseType type)
        {
            Mapper.Map(type, this);
        }
    }
}
