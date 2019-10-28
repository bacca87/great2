using System;
using System.Globalization;
using System.Windows.Data;

namespace Great2.Utils.Converters
{
    public class EmptyStringToTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value != null && (value as string) != string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
