using Itenso.TimePeriod;
using System;

namespace Great.Utils
{
    public static class TimePeriodTools
    {
        static RoundByQuarterHourDurationProvider roundByQuarterHour = new RoundByQuarterHourDurationProvider();

        public static float? GetRoundedTotalDuration(ITimePeriodCollection periods)
        {
            if (periods == null || periods.Count == 0)
                return null;

            TimeSpan totalDuration = periods.GetTotalDuration(roundByQuarterHour);
            float total = totalDuration.Hours + (totalDuration.Minutes / 100f);

            return total > 0 ? total : 24;
        }
    }

    public class RoundByQuarterHourDurationProvider : IDurationProvider
    {
        public TimeSpan GetDuration(DateTime start, DateTime end)
        {
            start = start.Date + TimeSpanExtensions.Round(start.TimeOfDay, RoundingDirection.Up, 15);
            end = end.Date + TimeSpanExtensions.Round(end.TimeOfDay, RoundingDirection.Down, 15);
            return end.Subtract(start);
        }
    }
}
