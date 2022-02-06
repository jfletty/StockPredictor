using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Domain.Enums;
using StockPredictor.DataRetriever.Domain.Tables.Api.Price;
using StockPredictor.DataRetriever.Domain.Tables.Fact;
using Microsoft.Extensions.Logging;
using StockPredictor.DataRetriever.Domain.Extensions;
using StockPredictor.DataRetriever.Services.Infrastructure;
using StockPredictor.DataRetriever.Services.Interfaces;
using StockPredictor.DataRetriever.Services.Services;

namespace StockPredictor.DataRetriever.Services.ApiIntegrators
{
    public class HistoricDailyRetriever
    {
        private readonly IRefreshesService _refreshesService;
        private readonly RestClientExtensions _restClient;
        private readonly DataWarehousePipeline _dataWarehousePipeline;
        private readonly HistoricDailyService _historicDailyService;
        private readonly ILogger<HistoricDailyRetriever> _logger;
        private const string RemainingUri = "v8/finance/chart";

        public HistoricDailyRetriever(
            IRefreshesService refreshesService,
            RestClientExtensions restClient,
            HistoricDailyService historicDailyService,
            DataWarehousePipeline dataWarehousePipeline,
            ILogger<HistoricDailyRetriever> logger)
        {
            _logger = logger;
            _refreshesService = refreshesService;
            _restClient = restClient;
            _historicDailyService = historicDailyService;
            _dataWarehousePipeline = dataWarehousePipeline;
        }

        public async Task CheckAndDelegateAsync()
        {
            while (true)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var symbolsForPricesToRefresh = await _refreshesService.IsRefreshRequiredAsync(JobType.HistoricDaily);

                if (!symbolsForPricesToRefresh.Any()) return;

                _logger.LogInformation(
                    $"Beginning to refresh {symbolsForPricesToRefresh.Count} Historic Daily Prices");

                await RefreshPricesAsync(symbolsForPricesToRefresh);

                stopwatch.Stop();

                if (stopwatch.Elapsed >= TimeSpan.FromMinutes(8))
                    _logger.LogErrorMessage(
                        "Historic Daily PriceRetriever taking too long. Total time: " +
                        $"{stopwatch.Elapsed.Minutes} minutes," +
                        $" {stopwatch.Elapsed.Seconds} seconds.");
                else
                    _logger.LogInformation($"Batch complete in {stopwatch.Elapsed.Minutes} minutes," +
                                           $" {stopwatch.Elapsed.Seconds} seconds");
            }
        }

        private async Task RefreshPricesAsync(IReadOnlyCollection<string> symbolsToRefresh)
        {
            var successfulResponses = new ConcurrentBag<KeyValuePair<string, List<MappedPrices>>>();
            var failedResponses = new ConcurrentBag<KeyValuePair<string, Exception>>();

            await _refreshesService.BeginRefresh(JobType.HistoricDaily, symbolsToRefresh);

            var parameters = new Dictionary<string, string>
                {{"interval", "1d"}, {"range", "1y"}, {"includePrePost", "false"}};

            var tasks = symbolsToRefresh.Select(async symbol =>
            {
                var temp = await _restClient.GetAsync<ApiStockPrice>(RemainingUri, symbol, parameters);
                if (temp.HasError || !string.IsNullOrEmpty(temp.SuccessResult?.Chart?.Error?.Description) ||
                    temp.SuccessResult?.Chart == null)
                {
                    Exception error;
                    if (temp.HasError)
                    {
                        error = temp.Error;
                    }
                    else if (temp.SuccessResult?.Chart?.Error != null)
                    {
                        error = new Exception(temp.SuccessResult.Chart.Error?.Description);
                    }
                    else
                    {
                        error = new Exception("No Data");
                    }

                    failedResponses.Add(new KeyValuePair<string, Exception>(symbol, error));
                }
                else
                {
                    successfulResponses.Add(
                        new KeyValuePair<string, List<MappedPrices>>(symbol,
                            temp.SuccessResult.Chart.GetFromList()));
                }
            });
            await Task.WhenAll(tasks);
            await _refreshesService.UpdateRefreshAsync(JobType.HistoricDaily, failedResponses.ToList());

            if (!successfulResponses.Any()) return;

            var emptyResponses = successfulResponses.Where(x => x.Value.Count == 0).Select(x => x.Key).ToList();
            if (emptyResponses.Any())
            {
                await _refreshesService.UpdateRefreshAsync(JobType.HistoricDaily,
                    successfulResponses.Where(x => x.Value.Count == 0).Select(x => x.Key));
            }

            var convertedStock =
                await _dataWarehousePipeline.TransformFactAndDimensions<DailyPriceRecap>(successfulResponses);
            if (!convertedStock.Any()) return;
            var (failed, passed) = await _historicDailyService.DeleteAndInsertAsync(convertedStock);

            await _refreshesService.UpdateRefreshAsync(JobType.HistoricDaily, failed);
            await _refreshesService.UpdateRefreshAsync(JobType.HistoricDaily, passed);
        }
    }
}