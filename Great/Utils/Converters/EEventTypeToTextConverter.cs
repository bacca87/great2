using Great.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Great.Utils.Converters
{
    class EEventTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EEventType type = (EEventType)value;
            string result = string.Empty;

            switch (type)
            {
                case EEventType.Vacations:
                case EEventType.OldVacations:
                    result = "Vacations";
                    break;

                case EEventType.CustomerVisit:
                    result = "Customer Visit";
                    break;

                case EEventType.BusinessTrip:
                    result = "Business Trip";
                    break;

                case EEventType.Education:
                    result = "Customer Visit";
                    break;

                case EEventType.Other:
                    result = "Other";
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
