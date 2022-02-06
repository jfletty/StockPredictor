using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Domain;
using StockPredictor.DataRetriever.Domain.Tables.Stage;
using Microsoft.EntityFrameworkCore;
using StockPredictor.DataRetriever.Services.Infrastructure;

namespace StockPredictor.DataRetriever.Services.Services
{
    public class SymbolService
    {
        private readonly DataContextProvider _dataContextProvider;

        public SymbolService()
        {
            _dataContextProvider = new DataContextProvider();
        }

        public async Task<Result<int>> CreateOrUpdateAsync(List<Symbol> apiSymbols)
        {
            if (!apiSymbols.Any()) return new Result<int>(1);
            apiSymbols = apiSymbols.Distinct().ToList();
            Result<int> result;

            using (var context = _dataContextProvider.DataWarehouse())
            {
                var dbSymbols = await context.Symbol.Where(x => x.ExchangeKey == apiSymbols.First().ExchangeKey)
                    .ToListAsync();
                var doAdd = new List<Symbol>();
                try
                {
                    foreach (var item in apiSymbols)
                    {
                        var singleSymbol = dbSymbols.FirstOrDefault(x =>
                            x.SymbolValue == item.SymbolValue && x.ExchangeValue == item.ExchangeValue);

                        if (singleSymbol == null)
                        {
                            doAdd.Add(item);

                        }
                        else
                        {
                            CheckAndUpdateAsync(item, singleSymbol);
                        }
                    }

                    await context.AddRangeAsync(doAdd);
                    await context.SaveChangesAsync();

                    result = new Result<int>(1);
                }
                catch (Exception e)
                {
                    result = new Result<int>(e);
                }
            }

            return result;
        }

        private static void CheckAndUpdateAsync(Symbol item, Symbol dbSymbol)
        {
            if (item.Equals(dbSymbol)) return;
            dbSymbol.Update(item);
        }

        public async Task DisableSymbolAsync(IEnumerable<KeyValuePair<string, Exception>> symbols)
        {
            using (var context = _dataContextProvider.DataWarehouse())
            {
                var activeSymbols = await GetActiveAsync(context);

                foreach (var (symbol, _) in symbols)
                {
                    var split = symbol.Split('.');
                    var item = activeSymbols.FirstOrDefault(x =>
                        x.SymbolValue == split[0] && x.ExchangeValue == (split.Length == 1 ? null : split[1]));

                    if (item != null)
                    {
                        item.IsDisabled = true;
                        item.DisabledDate = DateTime.UtcNow;
                    }
                }

                await context.SaveChangesAsync();
            }
        }

        private static async Task<List<Symbol>> GetActiveAsync(DataWarehouseContext context)
        {
            return await context.Symbol.Where(x => !x.IsDisabled).ToListAsync();
        }

        public async Task DisableSymbolsForInactiveExchanges()
        {
            using (var context = _dataContextProvider.DataWarehouse())
            {
                await context.RefreshModel.FromSqlRaw(
                    "EXEC Stage.DisableNotReadySymbols").ToListAsync();
            }
        }
    }
}
