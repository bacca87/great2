using System;

namespace Great.Utils.Extensions
{
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Rounds a TimeSpan based on the provided values.
        /// </summary>
        /// <param name="ts">The extension target.</param>
        /// <param name="Direction">The direction in which to round.</param>
        /// <param name="MinutePrecision">The precision to round to.</param>
        /// <returns>A new TimeSpan based on the provided values.</returns>
        public static TimeSpan Round(this TimeSpan ts, RoundingDirection Direction, int MinutePrecision)
        {
            if (Direction == RoundingDirection.Up)
                return TimeSpan.FromMinutes(
                MinutePrecision * Math.Ceiling(ts.TotalMinutes / MinutePrecision));

            if (Direction == RoundingDirection.Down)
                return TimeSpan.FromMinutes(
                MinutePrecision * Math.Floor(ts.TotalMinutes / MinutePrecision));

            // Really shouldn't be able to get here...
            return ts;
        }
    }

    /// <summary>
    /// Rounding direction used in rounding operations. 
    /// </summary>
    public enum RoundingDirection
    {
        /// <summary>
        /// Round up.
        /// </summary>
        Up

        , /// <summary>
        /// Round down.
        /// </summary>
        Down
    }
}