using System;
using System.Windows.Data;

namespace Great2.Utils.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class NegateBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
