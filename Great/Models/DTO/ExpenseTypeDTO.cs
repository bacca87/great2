using Great2.Models.Database;

namespace Great2.Models.DTO
{
    public class ExpenseTypeDTO
    {
        public long Id { get; set; }
        public string Description { get; set; }

        public ExpenseTypeDTO() { }

        public ExpenseTypeDTO(ExpenseType type)
        {
            Global.Mapper.Map(type, this);
        }
    }
}
