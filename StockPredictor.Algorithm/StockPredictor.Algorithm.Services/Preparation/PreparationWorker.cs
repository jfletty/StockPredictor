using System.Linq;
using System.Threading.Tasks;
using StockPredictor.Algorithm.Domain.Configuration;
using StockPredictor.Algorithm.Domain.Enums;
using StockPredictor.Algorithm.Services.Services;

namespace StockPredictor.Algorithm.Services.Preparation
{
    public class PreparationWorker
    {
        private readonly DatabaseRetriever _dbRetriever;
        private readonly S3Saver _s3Saver;
        private readonly PredictionCalculator _predictionCalculator;
        private readonly ProjectionSettingService _projectionService;
        private readonly PredictionWorkerConfig _predictionConfig;
        private const string _modelInputBucketName = "daily-model-input";
        private const string _projectionInputBucketName = "daily-projection-input";

        public PreparationWorker(
            DatabaseRetriever dbRetriever,
            S3Saver s3Saver,
            PredictionCalculator predictionCalculator,
            ProjectionSettingService projectionService,
            PredictionWorkerConfig predictionConfig)
        {
            _dbRetriever = dbRetriever;
            _s3Saver = s3Saver;
            _predictionCalculator = predictionCalculator;
            _projectionService = projectionService;
            _predictionConfig = predictionConfig;
        }

        public async Task UploadModelAsync()
        {
            while (true)
            {
                var work = await _dbRetriever.GetStocksRequiringModel();
                
                if (work == null || !work.Any()) return;
                work = work.Take(_predictionConfig.BatchSize).ToList();
                await _projectionService.StartProjection(UpdateType.DailyModel, work.Select(x => x.StockKey));

                var tasks = work.Select(async stock =>
                {
                    var data = await _dbRetriever.GetHistoricDataAsync(stock.StockKey);
                    await _s3Saver.SaveAsync(_modelInputBucketName, stock.StockKey, data);
                });
                await Task.WhenAll(tasks);
            }
        }

        public async Task UploadProjectionAsync()
        {
            while (true)
            {
                var work = await _dbRetriever.GetStocksRequiringProjections();
                if (work == null || !work.Any()) return;
                work = work.Take(_predictionConfig.BatchSize).ToList();

                var data = await _predictionCalculator.GenerateMockData(work);
                await _projectionService.StartProjection(UpdateType.DailyPrediction, data.Select(x => x.Key));
                
                var tasks = data.Select(async item =>
                {
                    var (stockKey, stringValues) = item;
                    await _s3Saver.SaveAsync(_projectionInputBucketName, stockKey, stringValues);
                });
                await Task.WhenAll(tasks);
            }
        }
    }
}
