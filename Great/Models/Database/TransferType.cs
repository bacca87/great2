using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("TransferType")]
    public partial class TransferType
    {
        public TransferType()
        {
            this.Factories = new HashSet<Factory>();
        }
    
        public long Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<Factory> Factories { get; set; }
    }
}
