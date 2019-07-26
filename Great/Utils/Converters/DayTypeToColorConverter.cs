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
    class DayTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color result = new Color() ;

            switch (value)
            {
                case 1:
                    result = UserSettings.VacationColor;
                    break;

                case 2: 
                    result = UserSettings.SickColor;
                    break;

                case 3:
                    result = UserSettings.HomeWorkColor;
                    break;

                case 4:
                    result = UserSettings.PendingVacationColor;
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
