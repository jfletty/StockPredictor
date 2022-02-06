using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Domain.Enums;
using StockPredictor.DataRetriever.Domain.Tables.Api.Stock;
using StockPredictor.DataRetriever.Domain.Tables.Fact;
using Microsoft.Extensions.Logging;
using StockPredictor.DataRetriever.Domain.Extensions;
using StockPredictor.DataRetriever.Services.Infrastructure;
using StockPredictor.DataRetriever.Services.Interfaces;
using StockPredictor.DataRetriever.Services.Services;

namespace StockPredictor.DataRetriever.Services.ApiIntegrators
{
    public class DailyPriceRetriever
    {
        private readonly IRefreshesService _refreshesService;
        private readonly RestClientExtensions _restClient;
        private readonly DataWarehousePipeline _dataWarehousePipeline;
        private readonly DailyPriceService _dailyPriceService;
        private readonly ILogger<DailyPriceRetriever> _logger;
        private const string RemainingUri = "v10/finance/quoteSummary";

        public DailyPriceRetriever(
            IRefreshesService refreshesService,
            RestClientExtensions restClient,
            DailyPriceService dailyPriceService,
            DataWarehousePipeline dataWarehousePipeline,
            ILogger<DailyPriceRetriever> logger)
        {
            _logger = logger;
            _refreshesService = refreshesService;
            _restClient = restClient;
            _dailyPriceService = dailyPriceService;
            _dataWarehousePipeline = dataWarehousePipeline;
        }

        public async Task CheckAndDelegateAsync()
        {
            while (true)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var symbolsForPricesToRefresh = await _refreshesService.IsRefreshRequiredAsync(JobType.DailyPrice);
                if (!symbolsForPricesToRefresh.Any()) return;

                _logger.LogInformation($"------------------ Beginning to refresh {symbolsForPricesToRefresh.Count} Daily Prices -----------------------");

                await RefreshPricesAsync(symbolsForPricesToRefresh);

                if (stopwatch.Elapsed >= TimeSpan.FromMinutes(15))
                    _logger.LogErrorMessage($"Daily PriceRetriever taking too long. Total time: {stopwatch.Elapsed.Minutes} minutes," +
                                            $" {stopwatch.Elapsed.Seconds} seconds.");
                stopwatch.Stop();
                _logger.LogInformation($"------------------ Batch complete in {stopwatch.Elapsed.Minutes} minutes," +
                                       $" {stopwatch.Elapsed.Seconds} seconds -----------------------");
            }
        }

        private async Task RefreshPricesAsync(IReadOnlyCollection<string> symbolsToRefresh)
        {
            var successfulResponses = new ConcurrentBag<KeyValuePair<string, List<ApiStockResult>>>();
            var failedResponses = new ConcurrentBag<KeyValuePair<string, Exception>>();

            await _refreshesService.BeginRefresh(JobType.DailyPrice, symbolsToRefresh);

            var parameters = new Dictionary<string, string> { { "modules", "price" } };
            var tasks = symbolsToRefresh.Select(async symbol =>
            {
                var temp = await _restClient.GetAsync<ApiStock>(RemainingUri, symbol, parameters);
                if (temp.HasError || temp.SuccessResult.QuoteSummary == null || !string.IsNullOrEmpty(temp.SuccessResult.QuoteSummary.Error?.Description) || temp.SuccessResult.QuoteSummary.Result.First().Price == null
                )
                {
                    Exception error;
                    if (temp.HasError)
                    {
                        error = temp.Error;
                    }
                    else if (temp.SuccessResult?.QuoteSummary?.Error != null)
                    {
                        error = new Exception(temp.SuccessResult.QuoteSummary.Error?.Description);
                    }
                    else
                    {
                        error = new Exception("No Data");
                    }

                    failedResponses.Add(new KeyValuePair<string, Exception>(symbol, error));
                }
                else
                {
                    successfulResponses.Add(new KeyValuePair<string, List<ApiStockResult>>(symbol, temp.SuccessResult.QuoteSummary.Result));
                }
            });
            await Task.WhenAll(tasks);

            await _refreshesService.UpdateRefreshAsync(JobType.DailyPrice, failedResponses.ToList());

            if (!successfulResponses.Any()) return;

            var emptyResponses = successfulResponses.Where(x => x.Value.Count == 0).Select(x => x.Key).ToList();
            if (emptyResponses.Any())
            {
                await _refreshesService.UpdateRefreshAsync(JobType.DailyPrice, successfulResponses.Where(x => x.Value.Count == 0).Select(x => x.Key));
            }

            var convertedStock = await _dataWarehousePipeline.TransformFactAndDimensions<DailyPriceRecap>(successfulResponses);


            if (!convertedStock.Any()) return;
            var (failed, passed) = await _dailyPriceService.DeleteAndInsertAsync(convertedStock);

            await _refreshesService.UpdateRefreshAsync(JobType.DailyPrice, failed);
            await _refreshesService.UpdateRefreshAsync(JobType.DailyPrice, passed);
        }
    }
}

