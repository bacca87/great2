using System;
using System.Globalization;
using System.Windows.Data;

namespace Great.Utils.Converters
{
    public class StringToUpperCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString().ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString().ToUpper();
        }
    }
}
