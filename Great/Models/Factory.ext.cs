using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models
{
    public partial class Factory
    {
        public Factory Clone()
        {
            return new Factory() {
                Id = Id,
                Name = Name,
                CompanyName = CompanyName,
                Address = Address,
                TransferType = TransferType,
                IsForfait = IsForfait,
                Latitude = Latitude,
                Longitude = Longitude,
            };
        }
    }
}
