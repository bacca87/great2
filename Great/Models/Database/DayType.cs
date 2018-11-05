using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("DayType")]
    public partial class DayType
    {
        public DayType()
        {
            this.Days = new HashSet<Day>();
        }
    
        public long Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<Day> Days { get; set; }
    }
}
