using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models
{
    [Table("FDLResult")]
    public partial class FDLResult
    {
        public FDLResult()
        {
            this.FDLs = new HashSet<FDL>();
        }
    
        public long Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<FDL> FDLs { get; set; }
    }
}
