using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Domain.Tables.Api;
using StockPredictor.DataRetriever.Domain.Tables.Api.Price;
using StockPredictor.DataRetriever.Domain.Tables.Api.Stock;
using StockPredictor.DataRetriever.Domain.Tables.Dimension;
using StockPredictor.DataRetriever.Domain.Tables.Fact;
using StockPredictor.DataRetriever.Domain.Tables.Stage;
using StockPredictor.DataRetriever.Domain.Extensions;
using StockPredictor.DataRetriever.Services.DataWarehousing;

namespace StockPredictor.DataRetriever.Services.Infrastructure
{
    public class DataWarehousePipeline
    {
        private readonly Etl _etl;

        public DataWarehousePipeline(Etl etl)
        {
            _etl = etl;
        }

        public static List<Symbol> TransformDimensions(List<ApiSymbol> rows, string exchangeKey)
        {
            rows = rows.Where(x => x.Symbol.Split('.').Length < 3 && !x.Symbol.Contains("/")).ToList();
            return rows.Select(symbol => new Symbol
            {
                SymbolValue = symbol.SymbolValue,
                ExchangeKey = exchangeKey,
                ExchangeValue = symbol.ExchangeValue

            }).ToList();
        }

        public async Task<List<Stock>> TransformFactAndDimensions(IEnumerable<KeyValuePair<string, List<ApiStockResult>>> rows)
        {
            await _etl.PrepareDimensionsForListings();
            var result = new ConcurrentBag<Stock>();
            Parallel.ForEach(rows, row =>
            {
                var (symbol, summary) = row;
                summary = summary.Count == 0 ? new List<ApiStockResult> { new ApiStockResult() } : summary;

                foreach (var stock in summary)
                {
                    result.Add(new Stock
                    {
                        SymbolKey = _etl.MapSymbol(symbol),
                        LongName = stock?.Price?.LongName,
                        Currency = stock?.Price?.Currency,
                        StreetAddress = string.Concat(stock?.AssetProfile?.Address1, stock?.AssetProfile?.Address2),
                        City = stock?.AssetProfile?.City,
                        PostCode = stock?.AssetProfile?.Zip,
                        Country = stock?.AssetProfile?.Country,
                        Website = stock?.AssetProfile?.Website,
                        IndustrySector = stock?.AssetProfile?.Industry,
                        Symbol = symbol
                    });
                }
            });

            return result.ToList();
        }

        public async Task<Dictionary<int, List<DailyPriceRecap>>> TransformFactAndDimensions<T>(IEnumerable<KeyValuePair<string, List<MappedPrices>>> rows)
        {
            await _etl.PrepareDimensionsForPrices();
            var result = new ConcurrentDictionary<int, List<DailyPriceRecap>>();

            var tasks = rows.Select(async row =>
            {
                var (symbol, prices) = row;
                foreach (var price in prices)
                {
                    var timeZone = _etl.MapSymbolExchange(symbol);
                    var date = DateTimeExtensions.UnixTimeStampToDateTime(price.Timestamp, timeZone);

                    var x = new DailyPriceRecap
                    {
                        StockKey = await _etl.MapOrCreateStockAsync(symbol),
                        DateKey = _etl.MapDate(date),
                        RegularMarketHigh = price.High,
                        RegularMarketOpen = price.Open,
                        RegularMarketVolume = price.Volume,
                        RegularMarketClose = price.Close,
                        RegularMarketPrice = price.Close, //Unsure
                        RegularMarketLow = price.Low,

                        Symbol = symbol
                    };

                    if (result.ContainsKey(x.StockKey))
                    {
                        result[x.StockKey].Add(x);
                    }
                    else
                    {
                        result[x.StockKey] = new List<DailyPriceRecap> { x };
                    }
                }
            });
            await Task.WhenAll(tasks);
            return result.ToDictionary(x => x.Key, x => x.Value);
        }

        public async Task<Dictionary<int, List<DailyPriceRecap>>> TransformFactAndDimensions<T>(IEnumerable<KeyValuePair<string, List<ApiStockResult>>> rows)
        {
            await _etl.PrepareDimensionsForPrices();
            var result = new ConcurrentDictionary<int, List<DailyPriceRecap>>();
            rows = rows.Where(x => x.Value != null);

            var tasks = rows.Select(async row =>
            {
                var (symbol, prices) = row;
                foreach (var subRow in prices.Where(x => x.Price.RegularMarketTime != 0))
                {
                    var price = subRow.Price;

                    var timeZone = _etl.MapSymbolExchange(symbol);
                    var date = DateTimeExtensions.UnixTimeStampToDateTime(price.RegularMarketTime, timeZone);
                    if (date < new DateTime(2010, 01, 01)) continue;
                    var x = new DailyPriceRecap
                    {
                        StockKey = await _etl.MapOrCreateStockAsync(symbol),
                        DateKey = _etl.MapDate(date),
                        PreMarketChange = price.PreMarketChange?.Raw,
                        PreMarketChangePercent = price.PreMarketChangePercent?.Raw,
                        PostMarketChange = price.PostMarketChange?.Raw,
                        PostMarketChangePercent = price.PostMarketChangePercent?.Raw ?? null,

                        RegularMarketPrice = price.RegularMarketPrice?.Raw,
                        RegularMarketLow = price.RegularMarketDayLow?.Raw,
                        RegularMarketHigh = price.RegularMarketDayHigh?.Raw,
                        RegularMarketVolume = price.RegularMarketVolume.Raw == null
                            ? (long?) null
                            : (long) price.RegularMarketVolume?.Raw,

                        RegularMarketOpen = price.RegularMarketOpen?.Raw,
                        RegularMarketClose = price.RegularMarketPrice?.Raw,
                        MarketCap = Convert.ToInt64(price.MarketCap?.Raw ?? 0),

                        Symbol = symbol,
                    };

                    if (result.ContainsKey(x.StockKey))
                    {
                        result[x.StockKey].Add(x);
                    }
                    else
                    {
                        result[x.StockKey] = new List<DailyPriceRecap> {x};
                    }
                }
            });

            await Task.WhenAll(tasks);
            return result.ToDictionary(x => x.Key, x => x.Value);
        }

        public async Task<Dictionary<string, StockPrice>> TransformFactAndDimensions(IEnumerable<KeyValuePair<string, List<MappedPrices>>> rows)
        {
            await _etl.PrepareDimensionsForPrices();
            var result = new ConcurrentDictionary<string, StockPrice>();

            var tasks = rows.Select(async row =>
            {
                var (symbol, prices) = row;
                foreach (var price in prices)
                {
                    var timeZone = _etl.MapSymbolExchange(symbol);
                    var date = DateTimeExtensions.UnixTimeStampToDateTime(price.Timestamp, timeZone);
                    var x = new StockPrice
                    {
                        StockKey = await _etl.MapOrCreateStockAsync(symbol),
                        DateKey = _etl.MapDate(date),
                        TimeOfDayKey = Etl.MapTimeOfDayKey(date.TimeOfDay),
                        Price = price.Close,
                        Volume = price.Volume,
                        Symbol = symbol,
                        ExternalKey = price.ExternalKey
                    };
                    result[price.ExternalKey] = x;
                }
            });
            await Task.WhenAll(tasks);
            return result.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
