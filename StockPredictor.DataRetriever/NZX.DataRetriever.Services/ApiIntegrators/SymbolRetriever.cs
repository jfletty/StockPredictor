using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Domain;
using StockPredictor.DataRetriever.Domain.Enums;
using StockPredictor.DataRetriever.Domain.Tables.Api;
using Microsoft.Extensions.Logging;
using RestSharp;
using StockPredictor.DataRetriever.Domain.Extensions;
using StockPredictor.DataRetriever.Services.Infrastructure;
using StockPredictor.DataRetriever.Services.Interfaces;
using StockPredictor.DataRetriever.Services.Services;

namespace StockPredictor.DataRetriever.Services.ApiIntegrators
{
    public class SymbolRetriever
    {
        private readonly SymbolService _symbolService;
        private readonly IRefreshesService _refreshesService;
        private readonly ILogger<SymbolRetriever> _logger;

        private const string Uri = "https://finnhub.io/api/v1/stock/symbol?token=c106bfv48v6t383m2k2g";

        public SymbolRetriever(
            SymbolService symbolService,
            IRefreshesService refreshesService,
            ILogger<SymbolRetriever> logger)
        {
            _symbolService = symbolService;
            _refreshesService = refreshesService;
            _logger = logger;
        }

        public async Task CheckAndDelegateAsync()
        {
            var exchangesToRefresh = await _refreshesService.IsRefreshRequiredAsync(JobType.Symbol);
            if (!exchangesToRefresh.Any()) return;

            _logger.LogInformation($"Beginning to refresh {exchangesToRefresh.Count} Exchange Lists");
            foreach(var exchange in exchangesToRefresh)
            {
                await RefreshExchangeAsync(exchange);
            }
            
            _logger.LogInformation("Successfully update all exchanges");
        }

        private async Task RefreshExchangeAsync(string exchangeKey)
        {
            await _refreshesService.BeginRefresh(JobType.Symbol, new List<string> { exchangeKey });
            var requestResult = await GetSymbolsByExchangeAsync<List<ApiSymbol>>(exchangeKey);

            if (requestResult.HasError)
            {
                _logger.LogErrorMessage(requestResult.Error.ToString());
                await _refreshesService.UpdateRefreshAsync(JobType.Symbol,
                    new List<KeyValuePair<string, Exception>>
                    {
                        new KeyValuePair<string, Exception>(exchangeKey,
                            requestResult.Error)
                    });
                return;
            }

            if (requestResult.SuccessResult.Any())
            {
                var convertedSymbols =
                    DataWarehousePipeline.TransformDimensions(requestResult.SuccessResult, exchangeKey);
                var result = await _symbolService.CreateOrUpdateAsync(convertedSymbols);

                if (result.HasError)
                {
                    _logger.LogErrorMessage(requestResult.Error.ToString());
                    await _refreshesService.UpdateRefreshAsync(JobType.Symbol, new List<KeyValuePair<string, Exception>>
                    {
                        new KeyValuePair<string, Exception>(exchangeKey,
                            result.Error)
                    });
                }
            }

            await _refreshesService.UpdateRefreshAsync(JobType.Symbol, new List<string> { exchangeKey });
        }

        private static async Task<Result<T>> GetSymbolsByExchangeAsync<T>(string exchangeKey)
        {
            var client = new RestClient(Uri);
            var request = new RestRequest(Method.GET);
            request.AddParameter("exchange", exchangeKey);

            try
            {
                var result = await client.GetAsync<T>(request);
                return new Result<T>(result);
            }
            catch (Exception e)
            {
                return new Result<T>(e);
            }
        }
    }
}
