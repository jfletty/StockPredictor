using System;
using System.Threading;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Services.ApiIntegrators;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockPredictor.DataRetriever.Domain.Extensions;

namespace StockPredictor.DataRetriever.Console
{
    public class Processor : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly DailyPriceRetriever _dailyPriceRetriever;
        private readonly SymbolRetriever _symbolRetriever;
        private readonly StockRetriever _stockRetriever;
        private readonly PriceRetriever _priceRetriever;
        private readonly HistoricDailyRetriever _historicDailyRetriever;

        public Processor(
            ILogger<Processor> logger,
            DailyPriceRetriever dailyPriceRetriever, 
            SymbolRetriever symbolRetriever, 
            StockRetriever stockRetriever, 
            PriceRetriever priceRetriever, 
            HistoricDailyRetriever historicDailyRetriever)
        {
            _logger = logger;
            _dailyPriceRetriever = dailyPriceRetriever;
            _symbolRetriever = symbolRetriever;
            _stockRetriever = stockRetriever;
            _priceRetriever = priceRetriever;
            _historicDailyRetriever = historicDailyRetriever;
        }
        
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var queueMonitorArray = new Task[1];
            for (var i = 0; i < queueMonitorArray.Length; ++i)
            {
                var monitorId = i;
                queueMonitorArray[i] =
                    Task.Factory.StartNew(async () => await WorkAsync(cancellationToken, monitorId), cancellationToken);
            }

            await Task.WhenAll(queueMonitorArray);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.FromResult(true);
        }

        private async Task WorkAsync(CancellationToken cancellationToken, int monitorId)
        {
            var delay = Math.Pow(3, monitorId);
            await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _symbolRetriever.CheckAndDelegateAsync();
                    await _stockRetriever.CheckAndDelegateAsync();
                    await _dailyPriceRetriever.CheckAndDelegateAsync();
                    await _priceRetriever.CheckAndDelegateAsync();
                    await _historicDailyRetriever.CheckAndDelegateAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogErrorMessage(ex.ToString());
                }

                var sleeping = TimeSpan.FromHours(1);
                _logger.LogInformation(
                    $"Monitor {monitorId} paused processing. Going to sleep. Next poll in {sleeping.TotalMinutes} minutes.");
                Thread.Sleep(sleeping);
            }
        }
    }
}
