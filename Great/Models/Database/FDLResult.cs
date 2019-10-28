using System.ComponentModel.DataAnnotations.Schema;

namespace Great2.Models.Database
{
    [Table("FDLResult")]
    public partial class FDLResult
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
