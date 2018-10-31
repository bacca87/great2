using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("ExpenseType")]
    public partial class ExpenseType
    {
        public ExpenseType()
        {
            this.Expenses = new HashSet<Expense>();
        }
    
        public long Id { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<Expense> Expenses { get; set; }
    }
}
