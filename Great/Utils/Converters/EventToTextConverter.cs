using Great.Models;
using Great.ViewModels.Database;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Great.Utils.Converters
{
    class EventToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            EventEVM ev = (EventEVM)value;

            switch (ev?.EType)
            {
                case EEventType.Vacations:
                case EEventType.OldVacations:

                    if (ev.EStatus == EEventStatus.Pending) result = "Vacations Pending";

                    if (ev.EStatus == EEventStatus.Accepted) result = "Vacations Accepted";

                    if (ev.EStatus == EEventStatus.Rejected) result = "Vacations Rejected";

                    break;

                default:
                    if (ev?.EType == EEventType.BusinessTrip) result = "Business Trip";

                    if (ev?.EType == EEventType.CustomerVisit) result = "Customer Visit";

                    if (ev?.EType == EEventType.Education) result = "Education";

                    if (ev?.EType == EEventType.Other) result = "Other";

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
