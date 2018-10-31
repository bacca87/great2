using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("Currency")]
    public partial class Currency
    {
        public Currency()
        {
            this.ExpenseAccounts = new HashSet<ExpenseAccount>();
        }
    
        public string Id { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<ExpenseAccount> ExpenseAccounts { get; set; }
    }
}
