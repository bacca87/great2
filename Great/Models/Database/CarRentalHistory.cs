using System.ComponentModel.DataAnnotations.Schema;

namespace Great2.Models.Database
{
    [Table("CarRentalHistory")]
    public partial class CarRentalHistory
    {
        public long Id { get; set; }
        public long Car { get; set; }
        public long StartDistance { get; set; }
        public long? EndDistance { get; set; }
        public long UOM { get; set; }
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
        public long StartDate { get; set; }
        public long? EndDate { get; set; }
        public long StartFuelLevel { get; set; }
        public long? EndFuelLevel { get; set; }
        public string Notes { get; set; }

        [ForeignKey("Car")]
        public virtual Car Car1 { get; set; }
        [ForeignKey("UOM")]
        public virtual UOM UOM1 { get; set; }
    }
}
