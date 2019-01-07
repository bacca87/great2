﻿
using AutoMapper;
using Great.Models.Database;

namespace Great.Models.DTO
{
    public class DayDTO
    {
        public long Timestamp { get; set; }
        public long Type { get; set; }
                
        public DayTypeDTO DayType { get; set; }

        public DayDTO() { }

        public DayDTO(Day day)
        {
            Mapper.Map(day, this);
        }
    }
}
