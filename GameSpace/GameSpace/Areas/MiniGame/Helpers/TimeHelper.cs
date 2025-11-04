using System;

namespace GameSpace.Areas.MiniGame.Helpers
{
    /// <summary>
    /// Time conversion helper for UTC+8 (Taiwan Standard Time) display
    /// </summary>
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo Utc8TimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");

        /// <summary>
        /// Convert DateTime (assumed UTC) to UTC+8 formatted string
        /// </summary>
        /// <param name="dt">DateTime in UTC</param>
        /// <param name="format">Output format string</param>
        /// <returns>Formatted UTC+8 time string</returns>
        public static string ToUtc8String(this DateTime dt, string format = "yyyy-MM-dd HH:mm")
        {
            var utc8Time = TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.Utc, Utc8TimeZone);
            return utc8Time.ToString(format);
        }

        /// <summary>
        /// Convert nullable DateTime (assumed UTC) to UTC+8 formatted string
        /// </summary>
        /// <param name="dt">Nullable DateTime in UTC</param>
        /// <param name="format">Output format string</param>
        /// <returns>Formatted UTC+8 time string, or "-" if null</returns>
        public static string ToUtc8String(this DateTime? dt, string format = "yyyy-MM-dd HH:mm")
        {
            if (dt == null) return "-";
            return dt.Value.ToUtc8String(format);
        }

        /// <summary>
        /// Convert DateTime (assumed UTC) to UTC+8 DateTime object
        /// </summary>
        /// <param name="dt">DateTime in UTC</param>
        /// <returns>DateTime in UTC+8</returns>
        public static DateTime ToUtc8(this DateTime dt)
        {
            return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.Utc, Utc8TimeZone);
        }

        /// <summary>
        /// Convert nullable DateTime (assumed UTC) to UTC+8 DateTime object
        /// </summary>
        /// <param name="dt">Nullable DateTime in UTC</param>
        /// <returns>Nullable DateTime in UTC+8</returns>
        public static DateTime? ToUtc8(this DateTime? dt)
        {
            if (dt == null) return null;
            return dt.Value.ToUtc8();
        }
    }
}
