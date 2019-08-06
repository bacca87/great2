using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("Expense")]
    public partial class Expense
    {
        public long Id { get; set; }
        public long ExpenseAccount { get; set; }
        public long Type { get; set; }
        public double? MondayAmount { get; set; }
        public double? TuesdayAmount { get; set; }
        public double? WednesdayAmount { get; set; }
        public double? ThursdayAmount { get; set; }
        public double? FridayAmount { get; set; }
        public double? SaturdayAmount { get; set; }
        public double? SundayAmount { get; set; }

        [ForeignKey("Type")]
        public virtual ExpenseType ExpenseType { get; set; }
        [ForeignKey("ExpenseAccount")]
        public virtual ExpenseAccount ExpenseAccount1 { get; set; }
    }
}
