using System;
using System.Globalization;
using System.Windows.Data;

namespace Great.Utils.Converters
{
    public class IntToMonthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DateTime.ParseExact((string)value, "MMMM", CultureInfo.InvariantCulture).Month;
        }
    }
}
