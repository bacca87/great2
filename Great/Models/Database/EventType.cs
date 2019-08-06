using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("EventType")]
    public partial class EventType
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
