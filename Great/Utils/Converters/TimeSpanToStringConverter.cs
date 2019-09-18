using System;
using System.Globalization;
using System.Windows.Data;

namespace Great.Utils.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            try
            {
                TimeSpan ts = (TimeSpan)value;

                if (ts.Days > 0)
                {
                    result = result + String.Format("{0}d,", ts.Days);
                }

                result = result + String.Format("{0}h,", ts.Hours);
                result = result + String.Format("{0}min", ts.Minutes);
            }
            catch { }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
