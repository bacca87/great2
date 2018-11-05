using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models
{
    [Table("FDLStatus")]
    public partial class FDLStatus
    {
        public FDLStatus()
        {
            this.ExpenseAccounts = new HashSet<ExpenseAccount>();
            this.FDLs = new HashSet<FDL>();
        }
    
        public long Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<ExpenseAccount> ExpenseAccounts { get; set; }
        public virtual ICollection<FDL> FDLs { get; set; }
    }
}
