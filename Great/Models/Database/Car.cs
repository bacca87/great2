using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great2.Models.Database
{
    [Table("Car")]
    public partial class Car
    {
        public Car()
        {
            this.CarRentalHistories = new HashSet<CarRentalHistory>();
        }

        public long Id { get; set; }
        public string LicensePlate { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string CarRentalCompany { get; set; }

        public virtual ICollection<CarRentalHistory> CarRentalHistories { get; set; }
    }
}
