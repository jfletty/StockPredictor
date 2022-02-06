using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Domain.Enums;
using StockPredictor.DataRetriever.Domain.Tables.Api.Stock;
using Microsoft.Extensions.Logging;
using StockPredictor.DataRetriever.Domain.Extensions;
using StockPredictor.DataRetriever.Services.Infrastructure;
using StockPredictor.DataRetriever.Services.Interfaces;
using StockPredictor.DataRetriever.Services.Services;

namespace StockPredictor.DataRetriever.Services.ApiIntegrators
{
    public class StockRetriever
    {
        private readonly IRefreshesService _refreshesService;
        private readonly RestClientExtensions _restClient;
        private readonly DataWarehousePipeline _dataWarehousePipeline;
        private readonly StockService _stockService;
        private readonly SymbolService _symbolService;
        private readonly ILogger<StockRetriever> _logger;
        private const string RemainingUri = "v10/finance/quoteSummary";

        public StockRetriever(
            IRefreshesService refreshesService,
            RestClientExtensions restClient,
            StockService stockService,
            DataWarehousePipeline dataWarehousePipeline,
            ILogger<StockRetriever> logger, 
            SymbolService symbolService)
        {
            _logger = logger;
            _symbolService = symbolService;
            _refreshesService = refreshesService;
            _restClient = restClient;
            _stockService = stockService;
            _dataWarehousePipeline = dataWarehousePipeline;
        }

        public async Task CheckAndDelegateAsync()
        {
            while (true)
            {
                var symbolsToRefresh = await _refreshesService.IsRefreshRequiredAsync(JobType.Stock);
                if (!symbolsToRefresh.Any()) return;

                _logger.LogInformation($"Beginning to refresh {symbolsToRefresh.Count} Stocks");

                await RefreshGroupedSymbolsAsync(symbolsToRefresh);

                _logger.LogInformation("Batch complete");
            }
        }

        private async Task RefreshGroupedSymbolsAsync(IReadOnlyCollection<string> symbolsToRefresh)
        {
            await _refreshesService.BeginRefresh(JobType.Stock, symbolsToRefresh);
            var parameters = new Dictionary<string, string> { { "modules", "assetProfile,price" } };

            var successfulResponses = new ConcurrentBag<KeyValuePair<string, List<ApiStockResult>>>();
            var failedResponses = new ConcurrentBag<KeyValuePair<string, Exception>>();

            var tasks = symbolsToRefresh.Select(async symbol =>
            {
                var temp = await _restClient.GetAsync<ApiStock>(RemainingUri, symbol, parameters);
                if (temp.HasError || 
                    !string.IsNullOrEmpty(temp.SuccessResult.QuoteSummary.Error?.Description) ||
                    temp.SuccessResult.QuoteSummary.Result.First().AssetProfile == null)
                {
                    Exception error;
                    if (temp.HasError)
                    {
                        error = temp.Error;
                    }
                    else if (temp.SuccessResult.QuoteSummary.Error != null)
                    {
                        error = new Exception(temp.SuccessResult.QuoteSummary.Error?.Description);
                    }
                    else
                    {
                        error = new Exception("No Data");
                    }

                    failedResponses.Add(new KeyValuePair<string, Exception>(symbol, error));
                    successfulResponses.Add(new KeyValuePair<string, List<ApiStockResult>>(symbol, new List<ApiStockResult>()));
                }
                else
                {
                    successfulResponses.Add(new KeyValuePair<string, List<ApiStockResult>>(symbol, temp.SuccessResult.QuoteSummary.Result));
                }
            });
            await Task.WhenAll(tasks);
            
            _logger.LogInformation($"{failedResponses.Count} Failed to retrieve. Adding Anyway");
            _logger.LogInformation($"{successfulResponses.Count} Symbols being saved");
            var convertedStock = await _dataWarehousePipeline.TransformFactAndDimensions(successfulResponses);
            var result = await _stockService.CreateOrUpdateAsync(convertedStock);

            await _symbolService.DisableSymbolsForInactiveExchanges();

            foreach (var (_, exception) in result.Where(x => x.Value != null))
            {
                _logger.LogErrorMessage(exception.ToString());
            }
            await _refreshesService.UpdateRefreshAsync(JobType.Stock, result);
        }
    }
}
