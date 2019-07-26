using Great.Models;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Great.Utils.Converters
{
    class DayNameToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush result = null;

            switch (value.ToString())
            {
                case "Saturday":
                    result = new SolidColorBrush( UserSettings.Themes.SaturdayColor);
                    break;

                case "Sunday": 
                    result = new SolidColorBrush(UserSettings.Themes.SundayColor);
                    break;


                default:
                    break;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
