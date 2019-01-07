using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("DayType")]
    public partial class DayType
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
