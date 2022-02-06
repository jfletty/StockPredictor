using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Domain.Tables.Fact;
using Microsoft.EntityFrameworkCore;
using StockPredictor.DataRetriever.Services.Infrastructure;

namespace StockPredictor.DataRetriever.Services.Services
{
    public class HistoricDailyService
    {
        private readonly DataContextProvider _dataContextProvider;

        public HistoricDailyService()
        {
            _dataContextProvider = new DataContextProvider();
        }

        public async Task<KeyValuePair<List<KeyValuePair<string, Exception>>, List<string>>> DeleteAndInsertAsync(
            Dictionary<int, List<DailyPriceRecap>> apiPriceRecap)
        {
            var grouped = apiPriceRecap.GroupBy(x => x.Key);
            var failedTasks = new ConcurrentBag<KeyValuePair<string, Exception>>();
            var passedTasks = new ConcurrentBag<string>();
            
            var tasks = grouped.Select(async prices =>
            {
                var rowedData = apiPriceRecap[prices.Key].ToDictionary(x => x.DateKey, x => x);
                try
                {
                    using (var context = _dataContextProvider.DataWarehouse())
                    {
                        var dbPriceRecaps = await context.DailyPriceRecap.AsNoTracking()
                            .Where(x => x.StockKey == prices.Key).Select(x => x.DateKey).ToListAsync();

                        var inserts = rowedData.Where(x => !dbPriceRecaps.Contains(x.Value.DateKey)).Select(x => x.Value);
                        
                        await context.DailyPriceRecap.AddRangeAsync(inserts);
                        await context.SaveChangesAsync();

                        passedTasks.Add(prices.First().Value.First().Symbol);
                    }
                }
                catch (Exception e)
                {
                    failedTasks.Add(new KeyValuePair<string, Exception>(prices.First().Value.First().Symbol, e));
                }
            });

            await Task.WhenAll(tasks);
            return new KeyValuePair<List<KeyValuePair<string, Exception>>, List<string>>(failedTasks.ToList(), passedTasks.ToList());
        }
    }
}
