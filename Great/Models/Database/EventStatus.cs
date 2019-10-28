using System.ComponentModel.DataAnnotations.Schema;

namespace Great2.Models.Database
{
    [Table("EventStatus")]
    public partial class EventStatus
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
