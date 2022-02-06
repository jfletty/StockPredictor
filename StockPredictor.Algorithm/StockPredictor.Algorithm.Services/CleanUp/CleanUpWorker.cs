using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StockPredictor.Algorithm.Domain.Tables.StoredProcedure;
using StockPredictor.Algorithm.Services.CsvMapping;

namespace StockPredictor.Algorithm.Services.CleanUp
{
    public class CleanUpWorker
    {
        private readonly S3Retriever _s3Retriever;
        private readonly DatabaseSaver _databaseSaver;
        private readonly ILogger<CleanUpWorker> _logger;

        public CleanUpWorker(S3Retriever s3Retriever, DatabaseSaver databaseSaver, ILogger<CleanUpWorker> logger)
        {
            _s3Retriever = s3Retriever;
            _databaseSaver = databaseSaver;
            _logger = logger;
        }

        public async Task SaveProjectionsAsync()
        {
            while (true)
            {
                var result = await _s3Retriever.DownloadAllAsync();
                if (result == null || !result.Any()) return;

                var mappedData = Csv.DeserializeFromString<DailyProjectionOutput>(result.Select(x => x.Value));

                if (mappedData.HasError)
                {
                    _logger.LogError(mappedData.Error, "CleanUpWorker.SaveProjectionsAsync()");
                    return;
                }

                var predictions = mappedData.SuccessResult
                    .Where(x => Convert.ToDecimal(x.RegularMarketClose) > 0).Select(item => item.AsPrediction()).ToList();
                predictions = predictions.Where(x => Math.Round(x.PredictedClose, 5) != (decimal) 9.99999).ToList();
                await _databaseSaver.SaveAsync(predictions);
                _logger.LogInformation($"Successfully saved to the db. count: {predictions.Count}");

                await _s3Retriever.DeleteItemsAsync(result.Select(x => x.Key));
            }
        }
    }
}
