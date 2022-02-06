using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockPredictor.Algorithm.Domain.Tables.Fact;
using StockPredictor.Algorithm.Services.Infrastructure;

namespace StockPredictor.Algorithm.Services.CleanUp
{
    public class DatabaseSaver
    {
        private readonly DataContextProvider _dataContextProvider;
        private readonly ILogger<DatabaseSaver> _logger;

        public DatabaseSaver(
            DataContextProvider dataContextProvider, 
            ILogger<DatabaseSaver> logger)
        {
            _dataContextProvider = dataContextProvider;
            _logger = logger;
        }

        public async Task SaveAsync(IEnumerable<DailyPrediction> data)
        {
            var grouped = data.GroupBy(x => x.StockKey);

            var tasks = grouped.Select(async group =>
            {
                using (var context = _dataContextProvider.DataWarehouse())
                {
                    var currentPredictions =
                        await context.DailyPrediction.Where(x => x.StockKey == group.Key).ToListAsync();
                    try
                    {
                        var toAdd = new ConcurrentBag<DailyPrediction>();
                        Parallel.ForEach(group, prediction =>
                        {
                            var existing = currentPredictions.FirstOrDefault(x => x.DateKey == prediction.DateKey);

                            if (existing != null)
                            {
                                existing.PredictedClose = prediction.PredictedClose;
                                existing.GeneratedAtDateTime = prediction.GeneratedAtDateTime;
                            }
                            else
                            {
                                toAdd.Add(prediction);
                            }
                        });

                        await context.AddRangeAsync(toAdd);
                        await context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "DatabaseSaver.SaveAsync()");
                        throw;
                    }

                }
            });
            await Task.WhenAll(tasks);
        }
    }
}
