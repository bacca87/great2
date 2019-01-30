﻿using AutoMapper;
using Great.Models.Database;

namespace Great.Models.DTO
{
    public class CarDTO
    {
        public long Id { get; set; }
        public string LicensePlate { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public long CarRentalCompany { get; set; }

        public CarDTO() { }

        public CarDTO(Car car)
        {
            Mapper.Map(car, this);
        }
    }
}