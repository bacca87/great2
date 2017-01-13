using System;

namespace Great.Utils
{
    public static class UnixTimestamp
    {
        public static long GetTimestamp(DateTime dateTime)
        {
            return (long)dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        public static DateTime GetDateTime(long unixTimestamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            date = date.AddSeconds(unixTimestamp);
            return date;
        }

        public static long Now()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds();
        }
    }
}
