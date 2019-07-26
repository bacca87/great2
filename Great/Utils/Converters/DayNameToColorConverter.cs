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
    class DayNameToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color result = new Color();

            switch (value.ToString())
            {
                case "Saturday":
                    result = UserSettings.Themes.SaturdayColor;
                    break;

                case "Sunday": // Waiting
                    result = UserSettings.Themes.SundayColor;
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
