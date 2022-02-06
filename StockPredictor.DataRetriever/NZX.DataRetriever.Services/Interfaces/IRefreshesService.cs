using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Domain.Enums;

namespace StockPredictor.DataRetriever.Services.Interfaces
{
    public interface IRefreshesService
    {
        Task<List<string>> IsRefreshRequiredAsync(JobType jobType);
        Task UpdateRefreshAsync(JobType refreshType, IEnumerable<KeyValuePair<string, Exception>> rows);
        Task UpdateRefreshAsync(JobType refreshType, IEnumerable<string> rows);
        Task BeginRefresh(JobType jobType, IEnumerable<string> externalKeys);
    }
}
