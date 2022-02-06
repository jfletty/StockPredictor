using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Domain.Tables.Dimension;
using StockPredictor.DataRetriever.Domain.Tables.Stage;
using StockPredictor.DataRetriever.Services.ApplicationServices;
using StockPredictor.DataRetriever.Services.Infrastructure;

namespace StockPredictor.DataRetriever.Services.DataWarehousing
{
    public class Etl
    {
        private readonly DataContextProvider _dataContextProvider;
        
        private Map<Symbol, (string, string), int> SymbolMap { get; } = new Map<Symbol, (string, string), int>(x => (x.SymbolValue, x.ExchangeValue), x => x.SymbolKey);
        private Map<Symbol, int, string> SymbolExchangeMap { get; } = new Map<Symbol, int, string>(x => x.SymbolKey, x => x.ExchangeKey);
        private Map<Stock, int, int> StockMap { get; } = new Map<Stock, int, int>(x => x.SymbolKey, x => x.StockKey);
        private Map<Date, DateTime, short> DateMap { get; } = new Map<Date, DateTime, short>(x => x.DateTime, x => x.DateKey);
        private Map<Exchange, string, string> ExchangeMap { get; } = new Map<Exchange, string, string>(x => x.ExchangeKey, x => x.TimeZone);

        public Etl()
        {
            _dataContextProvider = new DataContextProvider();
        }

        public int MapSymbol(string symbolWithExchange)
        {
            var split = symbolWithExchange.Split('.');
            return SymbolMap.MapOnly((split[0], split.Length == 1 ? null : split[1]));
        }

        public string MapSymbolExchange(string symbolWithExchange)
        {
            var symbolKey = MapSymbol(symbolWithExchange);
            var exchangeKey = SymbolExchangeMap.MapOnly(symbolKey);

            return ExchangeMap.MapOnly(exchangeKey);
        }

        public async Task<int> MapOrCreateStockAsync(string symbolWithExchange)
        {
            var symbol = MapSymbol(symbolWithExchange);
            
            using (var context = _dataContextProvider.DataWarehouse())
            {
                return await StockMap.MapOrCreate(symbol, async () =>
                {
                    var stock = new Stock
                    {
                        SymbolKey = symbol
                    };
                    await context.Stock.AddAsync(stock);
                    await context.SaveChangesAsync();
                    
                    return stock;
                });
            }
        }

        public short MapDate(DateTime dateTime)
        {
            return DateMap.MapOnly(dateTime.Date);
        }

        public async Task PrepareDimensionsForListings()
        {
            using (var context = _dataContextProvider.DataWarehouse())
            {
                await SymbolMap.LoadAsync(context.Symbol);
            }
        }

        public static short MapTimeOfDayKey(TimeSpan timeOfDay)
        {
            return (short)Math.Floor(timeOfDay.TotalMinutes);
        }
        
        public async Task PrepareDimensionsForPrices()
        {
            using (var context = _dataContextProvider.DataWarehouse())
            {
                await DateMap.LoadAsync(context.Date);
                await SymbolMap.LoadAsync(context.Symbol);
                await StockMap.LoadAsync(context.Stock);
                await ExchangeMap.LoadAsync(context.Exchange.Where(x => x.TimeZone != null));
                await SymbolExchangeMap.LoadAsync(context.Symbol);
            }
        }

        public static void Merge<TKey, TValue>(
            Dictionary<TKey, TValue> source,
            Dictionary<TKey, TValue> target,
            IEqualityComparer<TValue> comparer,
            out List<TValue> deletes,
            out List<TValue> inserts
        )
        {
            deletes = new List<TValue>();
            inserts = new List<TValue>();

            foreach (var (key, value) in target)
            {
                if (source.TryGetValue(key, out var sourceValue))
                {
                    if (!comparer.Equals(value, sourceValue))
                    {
                        deletes.Add(sourceValue);
                        inserts.Add(value);
                    }
                }
                else
                {
                    inserts.Add(value);
                }
            }
        }
    }
}
