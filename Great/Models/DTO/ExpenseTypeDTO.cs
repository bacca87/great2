using Great.Models.Database;

namespace Great.Models.DTO
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
