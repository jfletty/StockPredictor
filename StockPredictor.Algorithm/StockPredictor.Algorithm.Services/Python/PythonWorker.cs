using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StockPredictor.Algorithm.Domain.Enums;
using StockPredictor.Algorithm.Services.GenericS3;
using StockPredictor.Algorithm.Services.Services;

namespace StockPredictor.Algorithm.Services.Python
{
    public class PythonWorker
    {
        private readonly ProjectionSettingService _projectionService;
        private readonly ILogger<PythonWorker> _logger;
        private readonly S3ClientService _s3ClientService;

        public PythonWorker(
            ProjectionSettingService projectionService,
            ILogger<PythonWorker> logger,
            S3ClientService s3ClientService)
        {
            _projectionService = projectionService;
            _logger = logger;
            _s3ClientService = s3ClientService;
        }

        public async Task RunTrain()
        {
            while (true)
            {
                var work = await GetWorkFromS3("daily-model-input");

                if (!work.Any()) return;
                var tasks = work.Select(async file => { await PythonRunner("train.py", file, UpdateType.DailyModel); });
                await Task.WhenAll(tasks);
            }
        }

        public async Task RunPredict()
        {
            while (true)
            {
                var work = await GetWorkFromS3("daily-projection-input");
                if (!work.Any()) return;

                var tasks = work.Select(async file =>
                {
                    await PythonRunner("predict.py", file, UpdateType.DailyPrediction);
                });
                await Task.WhenAll(tasks);
            }
        }

        private async Task<List<string>> GetWorkFromS3(string bucketName)
        {
            var objects = await _s3ClientService.GetObjectsInBucket(bucketName);
            return objects.Where(x => x.Key.Contains(".csv")).Select(x => x.Key).ToList();
        }

        private async Task PythonRunner(string pythonFileName, string workFileName, UpdateType updateType)
        {
            try
            {
                var process = GetProcess(pythonFileName, workFileName);
                var logs = new List<string>();
                var errors = new List<string>();

                process.Start();
                while (process.StandardOutput.Peek() > -1)
                {
                    logs.Add(await process.StandardOutput.ReadLineAsync());
                }

                while (process.StandardError.Peek() > -1)
                {
                    errors.Add(await process.StandardError.ReadLineAsync());
                }

                process.WaitForExit();

                foreach (var error in errors.Where(x => !x.Contains("DeprecationWarning") && !x.Contains("import imp")))
                {
                    _logger.LogError(error);
                    if (logs.Contains(error))
                    {
                        logs.Remove(logs.First(x => x == error));
                    }

                    if (LogContainsStockKey(error))
                    {
                        var stockKey = GetStockKeyFromLog(error);
                        await _projectionService.UpdateSetting(updateType, stockKey, new Exception(error));
                    }
                }

                foreach (var log in logs)
                {
                    if (log.Contains("exception"))
                    {
                        _logger.LogError(log);
                        var stockKey = GetStockKeyFromLog(log);
                        await _projectionService.UpdateSetting(updateType, stockKey, new Exception(log));
                    }
                    else
                    {
                        if (LogContainsStockKey(log))
                        {
                            var stockKey = GetStockKeyFromLog(log);
                            await _projectionService.UpdateSetting(updateType, stockKey);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"PythonRunner() - {pythonFileName}");
            }
        }

        private static Process GetProcess(string pythonFileName, string workFileName)
        {
            var process = new Process
            {
                StartInfo =
                    new ProcessStartInfo
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Arguments = workFileName
                    }
            };
            // Needs to be updated if running on windows is desired
            process.StartInfo.Arguments = $"/app/Python/{pythonFileName} {workFileName}";
            process.StartInfo.FileName = "python3";

            return process;
        }

        private static int GetStockKeyFromLog(string log)
        {
            return Convert.ToInt32(Regex.Match(log, @"\d+").Value);
        }

        private static bool LogContainsStockKey(string log)
        {
            return log.Where(char.IsDigit).ToArray().Any() && !log.Contains("line");
        }
    }
}