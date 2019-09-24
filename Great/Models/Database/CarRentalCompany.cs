using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("CarRentalCompany")]
    public partial class CarRentalCompany
    {
        public CarRentalCompany()
        {
            this.Cars = new HashSet<Car>();
        }

        public long Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Car> Cars { get; set; }
    }
}