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
    class DayTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush result =null ;

            switch (value)
            {
                case 1:
                    result = new SolidColorBrush(UserSettings.Themes.VacationColor);
                    break;

                case 2: 
                    result = new SolidColorBrush(UserSettings.Themes.SickColor);
                    break;

                case 3:
                    result = new SolidColorBrush(UserSettings.Themes.HomeWorkColor);
                    break;

                case 4:
                    result = new SolidColorBrush(UserSettings.Themes.PendingVacationColor);
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
