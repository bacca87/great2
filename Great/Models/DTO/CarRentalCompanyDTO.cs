using AutoMapper;
using Great.Models.Database;
using System.Collections.Generic;

namespace Great.Models.DTO
{
    public class CarRentalCompanyDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public ICollection<Car> Cars { get; set; }


        public CarRentalCompanyDTO() { }

        public CarRentalCompanyDTO(CarRentalCompany company)
        {
            Mapper.Map(company, this);
        }

    }
}
