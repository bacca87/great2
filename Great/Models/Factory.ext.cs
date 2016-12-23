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
                Id = this.Id,
                Name = this.Name,
                CompanyName = this.CompanyName,
                Address = this.Address,
                TransferType = this.TransferType,
                IsForfait = this.IsForfait,
                Latitude = this.Latitude,
                Longitude = this.Longitude,

                TransferType1 = this.TransferType1,
            };
        }
    }
}
