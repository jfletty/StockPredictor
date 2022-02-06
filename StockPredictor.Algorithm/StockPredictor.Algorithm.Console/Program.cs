using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockPredictor.Algorithm.Services.CleanUp;
using StockPredictor.Algorithm.Services.GenericS3;
using StockPredictor.Algorithm.Services.Infrastructure;
using StockPredictor.Algorithm.Services.Preparation;
using StockPredictor.Algorithm.Services.Services;
using Serilog;
using SlackLogger;
using StockPredictor.Algorithm.Domain.Configuration;
using StockPredictor.Algorithm.Services.Python;

namespace StockPredictor.Algorithm.Console
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("Starting Up");
                var host = CreateHostBuilder(args).Build();
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .UseWindowsService()
                    .ConfigureProgramServices()
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.AddSlack(options =>
                        {
                            options.WebhookUrl = ""; 
                            options.NotificationLevel = LogLevel.None;
                        });
                    });

        private static IHostBuilder ConfigureProgramServices(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((context, services) =>
                {
                    services.AddScoped<DataContextProvider>();
                    services.AddScoped<S3ClientService>();
                    services.AddScoped<S3Saver>();
                    services.AddScoped<DatabaseRetriever>();
                    services.AddScoped<DatabaseSaver>();
                    services.AddScoped<S3Retriever>();
                    services.AddScoped<PredictionCalculator>();
                    services.AddScoped<ProjectionSettingService>();

                    services.AddScoped<PythonWorker>();
                    services.AddScoped<BackgroundWorker>();
                    services.AddScoped<PreparationWorker>();
                    
                    services.RegisterConfig<PredictionWorkerConfig>(context.Configuration, "Prediction");
                    services.AddHostedService<Processor>();
                });
        }

        private static void RegisterConfig<T>(this IServiceCollection services, IConfiguration configuration,
            string sectionName = null)
            where T : class, new()
        {
            var sectionConfig = configuration.GetSectionConfig<T>(sectionName);
            services.AddSingleton(sectionConfig);
        }

        private static T GetSectionConfig<T>(this IConfiguration configuration, string sectionName = null)
            where T : class, new()
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                sectionName = typeof(T).Name;
            }

            var serviceConfig = new T();
            configuration.GetSection(sectionName).Bind(serviceConfig);
            return serviceConfig;
        }
    }
}