using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great2.Models.Database
{
    [Table("OrderEmailRecipient")]
    public partial class OrderEmailRecipient
    {
        [Key, Column(Order = 1)]
        public long Order { get; set; }
        [Key, Column(Order = 2)]
        public string Address { get; set; }
    }
}
