﻿using AutoMapper;
using Great.Models.Database;

namespace Great.Models.DTO
{
    public class FactoryDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public FactoryDTO() { }

        public FactoryDTO(Factory factory)
        {
            Mapper.Map(factory, this);
        }
    }
}
