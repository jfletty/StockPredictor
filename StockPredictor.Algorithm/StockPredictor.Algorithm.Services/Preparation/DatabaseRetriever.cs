using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StockPredictor.Algorithm.Domain.Tables.Dimension;
using StockPredictor.Algorithm.Domain.Tables.StoredProcedure;
using StockPredictor.Algorithm.Services.CsvMapping;
using StockPredictor.Algorithm.Services.Infrastructure;

namespace StockPredictor.Algorithm.Services.Preparation
{
    public class DatabaseRetriever
    {
        private readonly DataContextProvider _dataContextProvider;

        public DatabaseRetriever(DataContextProvider dataContextProvider)
        {
            _dataContextProvider = dataContextProvider;
        }

        public async Task<List<StocksRequiringModels>> GetStocksRequiringModel()
        {
            List<StocksRequiringModels> result;
            using (var context = _dataContextProvider.DataWarehouse())
            {
                result = await context.StocksRequiringModels.FromSqlRaw(
                    "EXEC Stage.GetStocksRequiringModels").ToListAsync();
            }

            return result;
        }

        public async Task<List<StocksRequiringProjections>> GetStocksRequiringProjections()
        {
            List<StocksRequiringProjections> result;
            using (var context = _dataContextProvider.DataWarehouse())
            {
                result = await context.StocksRequiringProjections.FromSqlRaw(
                    "EXEC Stage.GetStocksRequiringDailyProjection").ToListAsync();
            }

            return result;
        }

        public async Task<string> GetHistoricDataAsync(int stockKey)
        {
            string result;
            using (var context = _dataContextProvider.DataWarehouse())
            {
                var stockKeyParameter = new SqlParameter("@stockKey", stockKey);
                var data = await context.DailyProjectionInput.FromSqlRaw(
                    // ReSharper disable once FormatStringProblem
                    "EXEC Stage.GetDailyModelInput @stockKey", stockKeyParameter).ToListAsync();

                foreach (var item in data)
                {
                    item.UpdateZeros();
                }

                result = !data.Any() ? null : Csv.SerializeToString(data);
            }

            return result;
        }

        public async Task<List<Date>> GetFutureDates(int futureDays)
        {
            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddDays(futureDays);
            using (var context = _dataContextProvider.DataWarehouse())
            {
                return await context.Date.Where(x => x.DateTime >= startDate && x.DateTime <= endDate)
                    .ToListAsync();
            }
        }
    }
}
