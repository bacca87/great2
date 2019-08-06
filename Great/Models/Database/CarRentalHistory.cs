using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("CarRentalHistory")]
    public partial class CarRentalHistory
    {
        public long Id { get; set; }
        public long Car { get; set; }
        public long StartKm { get; set; }
        public long? EndKm { get; set; }
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
        public long StartDate { get; set; }
        public long? EndDate { get; set; }
        public long StartFuelLevel { get; set; }
        public long? EndFuelLevel { get; set; }
        public string Notes { get; set; }

        [ForeignKey("Car")]
        public virtual Car Car1 { get; set; }
    }
}
