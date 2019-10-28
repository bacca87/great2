using System.ComponentModel.DataAnnotations.Schema;

namespace Great2.Models.Database
{
    [Table("FDLStatus")]
    public partial class FDLStatus
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
