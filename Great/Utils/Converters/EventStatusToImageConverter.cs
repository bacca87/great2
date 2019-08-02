using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Great.Utils.Converters
{
    public class EventStatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ImageSource result = null;
            int intValue = System.Convert.ToInt32(value);

            switch (intValue)
            {
                case 0: // Accepted
                    result = new BitmapImage(new Uri(@"pack://application:,,,/Great2;component/Images/16/sign-check.png"));
                    break;

                case 1: // Rejected
                    result = new BitmapImage(new Uri(@"pack://application:,,,/Great2;component/Images/16/sign-error.png"));
                    break;

                case 2: // Pending
                    result = new BitmapImage(new Uri(@"pack://application:,,,/Great2;component/Images/16/clock.png"));
                    break;

                case 3: // New
                    result = new BitmapImage(new Uri(@"pack://application:,,,/Great2;component/Images/16/sign-add-blue.png"));

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
