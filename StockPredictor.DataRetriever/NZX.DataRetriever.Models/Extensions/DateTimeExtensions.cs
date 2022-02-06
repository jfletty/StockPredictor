using System;

namespace StockPredictor.DataRetriever.Domain.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp, string timeZone)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return TimeZoneInfo.ConvertTimeFromUtc(dtDateTime, timeZoneInfo);
        }
    }
}
