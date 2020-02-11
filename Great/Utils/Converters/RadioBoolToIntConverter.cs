using System;
using System.Globalization;
using System.Windows.Data;

namespace Great2.Utils.Converters
{
    public class RadioBoolToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long integer = (long)value;

            if (integer == long.Parse(parameter.ToString()))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
