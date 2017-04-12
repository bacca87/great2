using System;
using System.Windows.Data;

namespace Great.Utils.Converters
{
    class FDLStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string result = string.Empty;
            int intValue = System.Convert.ToInt32(value);
            
            switch (intValue)
            {
                case 0:
                    result = "New";
                    break;

                case 1:
                    result = "Waiting";
                    break;

                case 2:
                    result = "Accepted";
                    break;

                case 3:
                    result = "Rejected";
                    break;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
