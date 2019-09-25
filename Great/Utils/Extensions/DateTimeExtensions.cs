using System;
using System.Globalization;

namespace Great.Utils.Extensions
{
    public static class DateTimeExtensions
    {
        public static int WeekNr(this DateTime datetime)
        {
            return DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(datetime, DateTimeFormatInfo.CurrentInfo.CalendarWeekRule, DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);
        }

        public static long ToUnixTimestamp(this DateTime datetime)
        {
            return (long)datetime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        public static DateTime FromUnixTimestamp(this DateTime datetime, long unixTimestamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            date = date.AddSeconds(unixTimestamp);
            return date;
        }

        public static long UnixTimestampNow(this DateTime datetime)
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public static DateTime Round(this DateTime value, TimeSpan unit)
        {
            return Round(value, unit, default(MidpointRounding));
        }

        public static DateTime Round(this DateTime value, TimeSpan unit, MidpointRounding style)
        {
            if (unit <= TimeSpan.Zero) throw new ArgumentOutOfRangeException("unit", "value must be positive");

            Decimal units = (decimal)value.Ticks / (decimal)unit.Ticks;
            Decimal roundedUnits = Math.Round(units, style);
            long roundedTicks = (long)roundedUnits * unit.Ticks;
            DateTime instance = new DateTime(roundedTicks);

            return instance;
        }
    }
}
