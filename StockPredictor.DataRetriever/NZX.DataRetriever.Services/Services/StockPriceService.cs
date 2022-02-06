using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Domain.Tables.Fact;
using Microsoft.EntityFrameworkCore;
using StockPredictor.DataRetriever.Services.DataWarehousing;
using StockPredictor.DataRetriever.Services.Infrastructure;

namespace StockPredictor.DataRetriever.Services.Services
{
    public class StockPriceService
    {
        private readonly DataContextProvider _dataContextProvider;

        public StockPriceService()
        {
            _dataContextProvider = new DataContextProvider();
        }

        public async Task DeleteAndInsertAsync(Dictionary<string, StockPrice> apiStockPrice)
        {
            var grouped = apiStockPrice.GroupBy(x => x.Value.StockKey);

            var tasks = grouped.Select(async prices =>
            {
                var apiStockData = apiStockPrice.Where(x => x.Value.StockKey == prices.Key).ToList();

                var earliest = apiStockData.Select(x => x.Value).OrderBy(x => x.DateKey).First().DateKey;
                var last = apiStockData.Select(x => x.Value).OrderByDescending(x => x.DateKey).First().DateKey;

                using (var context = _dataContextProvider.DataWarehouse())
                {
                    var dbStockPrices = await context.StockPrice.AsNoTracking()
                        .Where(x => x.StockKey == prices.Key && x.DateKey >= earliest && x.DateKey <= last)
                        .ToDictionaryAsync(x => x.ExternalKey);

                    Etl.Merge(dbStockPrices, prices.ToDictionary(x => x.Key, x => x.Value),
                        StockPrice.EqualityComparer, out var deletes,
                        out var inserts);

                    context.StockPrice.RemoveRange(deletes);
                    await context.SaveChangesAsync();
                    await context.StockPrice.AddRangeAsync(inserts);
                    await context.SaveChangesAsync();
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}
