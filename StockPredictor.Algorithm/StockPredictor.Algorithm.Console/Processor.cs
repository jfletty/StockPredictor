using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockPredictor.Algorithm.Services.CleanUp;
using StockPredictor.Algorithm.Services.Preparation;
using StockPredictor.Algorithm.Services.Python;

namespace StockPredictor.Algorithm.Console
{
    public class Processor : BackgroundService
    {
        private readonly ILogger _logger;
        
        private readonly PreparationWorker _preparationWorker;
        private readonly PythonWorker _pythonWorker;
        private readonly CleanUpWorker _cleanUpWorker;

        public Processor(
            ILogger<Processor> logger,
            PreparationWorker preparationWorker, 
            PythonWorker pythonWorker, 
            CleanUpWorker cleanUpWorker)
        {
            _logger = logger;
            _preparationWorker = preparationWorker;
            _pythonWorker = pythonWorker;
            _cleanUpWorker = cleanUpWorker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.FromResult(true);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _preparationWorker.UploadModelAsync();
                    await _preparationWorker.UploadProjectionAsync();
                    
                    // TODO: Run these against multiple workers
                    await _pythonWorker.RunTrain();
                    await _pythonWorker.RunPredict();
                    
                    await _cleanUpWorker.SaveProjectionsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.StackTrace);
                }

                var sleeping = TimeSpan.FromMinutes(1);

                _logger.LogInformation(
                    $"Saver paused processing. Going to sleep. Next poll in {sleeping.TotalMinutes} minutes.");
                Thread.Sleep(sleeping);
            }
        }
    }
}
