using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Domain.Configuration;
using StockPredictor.DataRetriever.Domain.Enums;
using StockPredictor.DataRetriever.Domain.Tables.Stage;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StockPredictor.DataRetriever.Services.Infrastructure;
using StockPredictor.DataRetriever.Services.Interfaces;

namespace StockPredictor.DataRetriever.Services.Services
{
    public class RefreshesService : IRefreshesService
    {
        private readonly DataContextProvider _dataContextProvider;
        private readonly RefreshFrequencyConfig _refreshFrequencyConfig;

        public RefreshesService(
            RefreshFrequencyConfig refreshFrequencyConfig)
        {
            _dataContextProvider = new DataContextProvider();
            _refreshFrequencyConfig = refreshFrequencyConfig;
        }

        public async Task<List<string>> IsRefreshRequiredAsync(JobType jobType)
        {
            return jobType switch
            {
                JobType.Symbol => await IsExchangeRefreshRequired(),
                JobType.Stock => await IsStockRefreshRequired(),
                JobType.Price => await IsPriceRefreshRequired(),
                JobType.DailyPrice => await IsDailyPriceRefreshRequired(),
                JobType.HistoricDaily => await IsHistoricRefreshRequired(),
                _ => throw new ArgumentOutOfRangeException(nameof(jobType), jobType, null)
            };
        }

        private async Task<List<string>> IsDailyPriceRefreshRequired()
        {
            List<string> result;
            using (var context = _dataContextProvider.DataWarehouse())
            {
                var exchanges = await context.RefreshModel.FromSqlRaw("EXEC Stage.GetStocksRequiringDailyRecapRefresh")
                    .ToListAsync();

                result = exchanges.Any()
                    ? exchanges.Select(c => c.ExchangeKey).ToList()
                    : new List<string>();
            }

            return result;
        }

        private async Task<List<string>> IsHistoricRefreshRequired()
        {
            List<string> result;
            using (var context = _dataContextProvider.DataWarehouse())
            {
                var exchanges = await context.RefreshModel.FromSqlRaw("EXEC Stage.GetRequiringHistoricRefresh")
                    .ToListAsync();

                result = exchanges.Any()
                    ? exchanges.Select(c => c.ExchangeKey).ToList()
                    : new List<string>();
            }

            return result;
        }
        
        private async Task<List<string>> IsExchangeRefreshRequired()
        {
            var frequencyParameter = new SqlParameter("@frequency", _refreshFrequencyConfig.Symbol);
            List<string> result;
            using (var context = _dataContextProvider.DataWarehouse())
            {
                var exchanges = await context.RefreshModel.FromSqlRaw(
                    "EXEC Stage.GetExchangesRequiringRefresh @frequency",
                    // ReSharper disable once FormatStringProblem
                    frequencyParameter).ToListAsync();

                result = exchanges.Any()
                    ? exchanges.Select(c => c.ExchangeKey).ToList()
                    : new List<string>();
            }

            return result;
        }

        private async Task<List<string>> IsStockRefreshRequired()
        {
            var frequencyParameter = new SqlParameter("@frequency", _refreshFrequencyConfig.Stock);
            List<string> result;

            using (var context = _dataContextProvider.DataWarehouse())
            {
                var exchanges = await context.RefreshModel.FromSqlRaw("EXEC Stage.GetStocksRequiringRefresh @frequency",
                    // ReSharper disable once FormatStringProblem
                    frequencyParameter).ToListAsync();

                result = exchanges.Any()
                    ? exchanges.Select(c => c.ExchangeKey).ToList()
                    : new List<string>();
            }

            return result;
        }

        private async Task<List<string>> IsPriceRefreshRequired()
        {
            var frequencyParameter = new SqlParameter("@frequency", _refreshFrequencyConfig.Price);
            List<string> result;

            using (var context = _dataContextProvider.DataWarehouse())
            {
                var exchanges = await context.RefreshModel.FromSqlRaw(
                    "EXEC Stage.GetStocksRequiringPriceRefresh @frequency",
                    // ReSharper disable once FormatStringProblem
                    frequencyParameter).ToListAsync();

                result = exchanges.Any()
                    ? exchanges.Select(c => c.ExchangeKey).ToList()
                    : new List<string>();
            }

            return result;
        }

        public async Task BeginRefresh(JobType refreshType, IEnumerable<string> externalKeys)
        {
            using (var context = _dataContextProvider.DataWarehouse())
            {
                var refreshes = await context.Refresh.Where(x =>
                        x.RefreshType == refreshType)
                    .ToListAsync();

                var inserts = new ConcurrentBag<Refresh>();

                Parallel.ForEach(externalKeys, externalKey =>
                {
                    var refresh = refreshes.FirstOrDefault(x => x.ExternalKey == externalKey);

                    if (refresh == null)
                    {
                        refresh = new Refresh
                        {
                            StartDateTime = DateTime.UtcNow,
                            RefreshType = refreshType,
                            ExternalKey = externalKey,
                            Status = JobStatus.Pending
                        };
                        inserts.Add(refresh);
                    }
                    else
                    {
                        refresh.ResetBeforeRefresh();
                    }
                });

                await context.AddRangeAsync(inserts);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateRefreshAsync(JobType refreshType, IEnumerable<KeyValuePair<string, Exception>> rows)
        {
            using (var context = _dataContextProvider.DataWarehouse())
            {
                var refreshes =
                    await context.Refresh.Where(x => x.RefreshType == refreshType && x.Status == JobStatus.Pending).ToListAsync();

                Parallel.ForEach(rows, row =>
                {
                    var (externalKey, exception) = row;
                    var refresh = refreshes.First(x => x.ExternalKey == externalKey);
                    refresh.UpdateAfterRefresh(exception);
                });

                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateRefreshAsync(JobType refreshType, IEnumerable<string> rows)
        {
            using (var context = _dataContextProvider.DataWarehouse())
            {
                var refreshes =
                    await context.Refresh.Where(x => x.RefreshType == refreshType && x.Status == JobStatus.Pending).ToListAsync();

                Parallel.ForEach(rows, externalKey =>
                {
                    var refresh = refreshes.First(x => x.ExternalKey == externalKey);
                    refresh.UpdateAfterRefresh();
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
