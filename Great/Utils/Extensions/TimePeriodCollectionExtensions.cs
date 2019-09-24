using Itenso.TimePeriod;
using System;

namespace Great.Utils.Extensions
{
    public static class TimePeriodCollectionExtensions
    {
        static RoundByQuarterHourDurationProvider roundByQuarterHour = new RoundByQuarterHourDurationProvider();

        public static float? GetRoundedTotalDuration(this ITimePeriodCollection periods)
        {
            if (periods == null || periods.Count == 0) return null;

            TimeSpan totalDuration = periods.GetTotalDuration(roundByQuarterHour);
            float total = totalDuration.Hours + totalDuration.Minutes / 100f;

            return total > 0 ? total : 24;
        }
    }

    public class RoundByQuarterHourDurationProvider : IDurationProvider
    {
        public TimeSpan GetDuration(DateTime start, DateTime end)
        {
            start = start.Date + start.TimeOfDay.Round(RoundingDirection.Up, 15);
            end = end.Date + end.TimeOfDay.Round(RoundingDirection.Down, 15);
            return end.Subtract(start);
        }
    }
}