using System;
using System.Threading.Tasks;
using Polly;

namespace StockPredictor.DataRetriever.Domain.Helpers
{
    public static class PollyHelper
    {
        private const int MaximumRetries = 3;
        private const int TimeSpanExtensionFactor = 2;

        public static Task WaitAndRetry<TException>(
            Action<Exception, TimeSpan> onRetry,
            Func<Task> action)
            where TException : Exception
        {
            return Policy
                .Handle<TException>()
                .WaitAndRetryAsync(
                    MaximumRetries,
                    retryCount => TimeSpan.FromSeconds(Math.Pow(TimeSpanExtensionFactor, retryCount - 1)),
                    onRetry)
                .ExecuteAsync(action);
        }
    }
}
