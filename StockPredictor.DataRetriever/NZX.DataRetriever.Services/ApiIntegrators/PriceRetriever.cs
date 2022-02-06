using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Domain.Enums;
using StockPredictor.DataRetriever.Domain.Tables.Api.Price;
using Microsoft.Extensions.Logging;
using StockPredictor.DataRetriever.Domain.Extensions;
using StockPredictor.DataRetriever.Services.Infrastructure;
using StockPredictor.DataRetriever.Services.Interfaces;
using StockPredictor.DataRetriever.Services.Services;

namespace StockPredictor.DataRetriever.Services.ApiIntegrators
{
    public class PriceRetriever
    {
        private readonly IRefreshesService _refreshesService;
        private readonly RestClientExtensions _restClient;
        private readonly DataWarehousePipeline _dataWarehousePipeline;
        private readonly StockPriceService _stockPriceService;
        private readonly SymbolService _symbolService;
        private readonly ILogger<PriceRetriever> _logger;
        private const string RemainingUri = "v8/finance/chart";

        public PriceRetriever(
            IRefreshesService refreshesService,
            RestClientExtensions restClient,
            StockPriceService stockPriceService,
            DataWarehousePipeline dataWarehousePipeline,
            ILogger<PriceRetriever> logger,
            SymbolService symbolService)
        {
            _logger = logger;
            _symbolService = symbolService;
            _refreshesService = refreshesService;
            _restClient = restClient;
            _stockPriceService = stockPriceService;
            _dataWarehousePipeline = dataWarehousePipeline;
        }

        public async Task CheckAndDelegateAsync()
        {
            while (true)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var symbolsForPricesToRefresh = await _refreshesService.IsRefreshRequiredAsync(JobType.Price);
                if (!symbolsForPricesToRefresh.Any()) return;

                _logger.LogInformation($"------------------ Beginning to refresh {symbolsForPricesToRefresh.Count} Prices -----------------------");

                await RefreshPricesAsync(symbolsForPricesToRefresh);

                if (stopwatch.Elapsed >= TimeSpan.FromMinutes(6))
                    _logger.LogErrorMessage($"PriceRetriever taking too long. Total time: {stopwatch.Elapsed.Minutes} minutes," +
                                            $" {stopwatch.Elapsed.Seconds} seconds.");
                stopwatch.Stop();
                _logger.LogInformation($"------------------ Batch complete in {stopwatch.Elapsed.Minutes} minutes," +
                                       $" {stopwatch.Elapsed.Seconds} seconds -----------------------");
            }
        }


        private async Task RefreshPricesAsync(IReadOnlyCollection<string> symbolsToRefresh)
        {
            var successfulResponses = new ConcurrentBag<KeyValuePair<string, List<MappedPrices>>>();
            var failedResponses = new ConcurrentBag<KeyValuePair<string, Exception>>();

            await _refreshesService.BeginRefresh(JobType.Price, symbolsToRefresh);

            var parameters = new Dictionary<string, string>
                    {{"includePrePost", "false"}, {"interval", "1m"}, {"useYfid", "true"}, {"range", "2d"}};

            var tasks = symbolsToRefresh.Select(async symbol =>
            {
                var temp = await _restClient.GetAsync<ApiStockPrice>(RemainingUri, symbol, parameters);
                if (temp.HasError || !string.IsNullOrEmpty(temp.SuccessResult?.Chart?.Error?.Description) || temp.SuccessResult?.Chart == null)
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

            await _symbolService.DisableSymbolAsync(failedResponses.Where(x => x.Value.Message.Contains("delisted")));
            await _refreshesService.UpdateRefreshAsync(JobType.Price, failedResponses.ToList());

            if (!successfulResponses.Any()) return;

            var emptyResponses = successfulResponses.Where(x => x.Value.Count == 0).Select(x => x.Key).ToList();
            if (emptyResponses.Any())
            {
                await _refreshesService.UpdateRefreshAsync(JobType.Price, successfulResponses.Where(x => x.Value.Count == 0).Select(x => x.Key));
            }

            var convertedStock = await _dataWarehousePipeline.TransformFactAndDimensions(successfulResponses);

            if (!convertedStock.Any()) return;
            await _stockPriceService.DeleteAndInsertAsync(convertedStock);
  
            var processedExternalKeys = convertedStock.Select(x => x.Key.Substring(0, x.Key.LastIndexOf('.'))).Distinct().ToList();
            await _refreshesService.UpdateRefreshAsync(JobType.Price, processedExternalKeys);
        }
    }
}
