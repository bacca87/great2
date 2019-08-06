using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("EventStatus")]
    public partial class EventStatus
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
