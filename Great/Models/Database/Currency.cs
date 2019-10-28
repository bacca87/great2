using System.ComponentModel.DataAnnotations.Schema;

namespace Great2.Models.Database
{
    [Table("Currency")]
    public partial class Currency
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }
}
