using System;
using System.Globalization;
using System.Windows.Data;

namespace Great.Utils.Converters
{
    public class StringToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null)
                    return ((TimeSpan)value).ToString("hh\\:mm");
                }
            catch { }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null)
                {
                    TimeSpan time;
                    string valueString = value as string;

                    if (valueString.Trim().Length < 5)
                        return null;

                    TimeSpan.TryParse(valueString, out time);
                    return time;
                }
            }
            catch { }

            return value;
        }
    }
}
