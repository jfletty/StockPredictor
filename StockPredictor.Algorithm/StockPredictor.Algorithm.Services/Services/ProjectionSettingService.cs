using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StockPredictor.Algorithm.Domain.Enums;
using StockPredictor.Algorithm.Domain.Tables.Stage;
using StockPredictor.Algorithm.Services.Infrastructure;

namespace StockPredictor.Algorithm.Services.Services
{
    public class ProjectionSettingService
    {
        private readonly DataContextProvider _dataContextProvider;

        public ProjectionSettingService(
            DataContextProvider dataContextProvider
            )
        {
            _dataContextProvider = dataContextProvider;
        }

        public async Task StartProjection(UpdateType updateType, IEnumerable<int> stockKeys)
        {
            var settings = stockKeys.Select(stockKey => new ProjectionSetting
            {
                StartDateTime = DateTime.UtcNow, 
                UpdateType = updateType, 
                StockKey = stockKey,
                Status = JobStatus.Pending
            }).ToList();
            
            using (var context = _dataContextProvider.DataWarehouse())
            {
                await context.AddRangeAsync(settings);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateSetting(UpdateType updateType, int stockKey, Exception error = null)
        {
            using (var context = _dataContextProvider.DataWarehouse())
            {
                var setting = await context.ProjectionSetting
                    .FirstOrDefaultAsync(x =>  x.UpdateType == updateType && 
                                      x.StockKey == stockKey && 
                                      x.Status == JobStatus.Pending);
                if (setting != null)
                {
                    setting.UpdateAfterCompletion(error);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
