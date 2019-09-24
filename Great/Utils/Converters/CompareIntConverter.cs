using System;
using System.Globalization;
using System.Windows.Data;

namespace Great.Utils.Converters
{
    public class CompareIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int) parameter == (int) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? parameter : Binding.DoNothing;
        }
    }
}