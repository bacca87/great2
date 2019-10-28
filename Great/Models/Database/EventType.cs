using System.ComponentModel.DataAnnotations.Schema;

namespace Great2.Models.Database
{
    [Table("EventType")]
    public partial class EventType
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
