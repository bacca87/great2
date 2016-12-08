using GMap.NET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Great.Converters
{
    public class PointLatLngToPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PointLatLng point = (PointLatLng)value;
            return new Point(point.Lat, point.Lng);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Point point = (Point)value;
            return new PointLatLng(point.X, point.Y);
        }
    }
}
