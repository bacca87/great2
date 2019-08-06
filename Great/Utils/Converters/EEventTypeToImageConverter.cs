using Great.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Great.Utils.Converters
{
    class EEventTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ImageSource result = null;
            EEventType type = (EEventType)value;

            switch (type)
            {
                case EEventType.Vacations:
                case EEventType.OldVacations:
                    result = new BitmapImage(new Uri(@"pack://application:,,,/Great2;component/Images/16/star.png"));
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
