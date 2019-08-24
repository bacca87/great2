using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("DayEvent")]
    public partial class DayEvent
    {
        public DayEvent()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public long Timestamp { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 2)]
        public long EventId { get; set; }

        [ForeignKey("Timestamp")]
        public virtual Day Day1 { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event1 { get; set; }
    }
}
