using System.ComponentModel.DataAnnotations.Schema;

namespace Great2.Models.Database
{
    [Table("TransferType")]
    public partial class TransferType
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
