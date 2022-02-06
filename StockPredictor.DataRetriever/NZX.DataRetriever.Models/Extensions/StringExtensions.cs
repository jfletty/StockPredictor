using System;
using System.Globalization;
using StockPredictor.DataRetriever.Domain.Enums;

namespace StockPredictor.DataRetriever.Domain.Extensions
{
    public static class StringExtensions
    {
        public static string LogNumbers(JobType jobType, string what, int count, TimeSpan duration)
        {
              var log = $"{jobType}: {DateTime.UtcNow.AddHours(12)}. {what}. Count: {count.ToString("N0", new CultureInfo("en-US"))}." +
                      $" TimeTaken: {duration.Minutes} minutes, {duration.Seconds} seconds";
            return log;
        }
    }
}
