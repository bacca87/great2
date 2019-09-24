using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("ExpenseType")]
    public partial class ExpenseType
    {
        public long Id { get; set; }
        public string Description { get; set; }
    }
}