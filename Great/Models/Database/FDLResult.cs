using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("FDLResult")]
    public partial class FDLResult
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
