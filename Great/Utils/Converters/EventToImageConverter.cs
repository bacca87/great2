using Great.Models;
using Great.ViewModels.Database;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Great.Utils.Converters
{
    class EventToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ImageSource result = null;
            EventEVM ev = (EventEVM) value;

            switch (ev.EType)
            {
                case EEventType.Vacations:
                case EEventType.OldVacations:

                    if (ev.EStatus == EEventStatus.Pending) result = new BitmapImage(new Uri(@"pack://application:,,,/Great2;component/Images/16/clock.png"));

                    if (ev.EStatus == EEventStatus.Accepted) result = new BitmapImage(new Uri(@"pack://application:,,,/Great2;component/Images/16/sign-check.png"));

                    if (ev.EStatus == EEventStatus.Rejected) result = new BitmapImage(new Uri(@"pack://application:,,,/Great2;component/Images/16/sign-ban.png"));

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