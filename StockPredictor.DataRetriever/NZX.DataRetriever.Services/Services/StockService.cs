using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Domain.Tables.Dimension;
using Microsoft.EntityFrameworkCore;
using StockPredictor.DataRetriever.Services.Infrastructure;

namespace StockPredictor.DataRetriever.Services.Services
{
    public class StockService
    {
        private readonly DataContextProvider _dataContextProvider;

        public StockService()
        {
            _dataContextProvider = new DataContextProvider();
        }

        public async Task<List<KeyValuePair<string, Exception>>> CreateOrUpdateAsync(IEnumerable<Stock> apiStocks)
        {
            var result = new List<KeyValuePair<string, Exception>>();

            using (var context = _dataContextProvider.DataWarehouse())
            {
                var dbStocks = await context.Stock.ToListAsync();
                foreach (var item in apiStocks)
                {
                    try
                    {
                        var singleStock = dbStocks.FirstOrDefault(x => x.SymbolKey == item.SymbolKey);
                        if (singleStock == null)
                        {
                            await context.AddAsync(item);
                        }
                        else
                        {
                            CheckAndUpdateAsync(singleStock, item);
                        }

                        result.Add(new KeyValuePair<string, Exception>(item.Symbol, null));
                    }
                    catch (Exception e)
                    {
                        result.Add(new KeyValuePair<string, Exception>(item.Symbol, e));
                    }
                }

                await context.SaveChangesAsync();
            }

            return new List<KeyValuePair<string, Exception>>(result);
        }
        
        private static void CheckAndUpdateAsync(Stock source, Stock target)
        {
            if (target.Equals(source)) return;
            source.Update(target);
        }
    }
}
