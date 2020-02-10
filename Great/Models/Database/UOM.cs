using System.ComponentModel.DataAnnotations.Schema;

namespace Great2.Models.Database
{
    [Table("UOM")]
    public partial class UOM
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
    }
}
