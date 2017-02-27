using System;
using System.Globalization;

namespace Great.Utils
{
    public static class DateTimeHelper
    {
        public static int WeekNr(this DateTime date)
        {
            return DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(date, DateTimeFormatInfo.CurrentInfo.CalendarWeekRule, DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);
        }
    }
}
