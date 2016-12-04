using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.DB
{
    public partial class Factory
    {
        public PointLatLng? MapPoint
        {
            get
            {
                if (Latitude.HasValue && Longitude.HasValue)
                    return new PointLatLng(Latitude.Value, Longitude.Value);

                return null;
            }
        }

        public GMapMarker MapMarker { get; set; }
    }
}
