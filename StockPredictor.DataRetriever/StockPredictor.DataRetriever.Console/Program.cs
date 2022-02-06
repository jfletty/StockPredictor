using System;
using System.Threading.Tasks;
using StockPredictor.DataRetriever.Services.ApiIntegrators;
using StockPredictor.DataRetriever.Services.DataWarehousing;
using StockPredictor.DataRetriever.Services.Infrastructure;
using StockPredictor.DataRetriever.Services.Interfaces;
using StockPredictor.DataRetriever.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SlackLogger;
using StockPredictor.DataRetriever.Domain.Configuration;
using StockPredictor.DataRetriever.Domain.Extensions;

namespace StockPredictor.DataRetriever.Console
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

        private static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .UseWindowsService()
                    .ConfigureProgramServices()
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.AddSlack(options =>
                        {
                            options.WebhookUrl = "";
                            options.LogLevel = LogLevel.Information;
                            options.NotificationLevel = LogLevel.None;
                        });
                    });

        private static IHostBuilder ConfigureProgramServices(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((context, services) =>
                {
                    services.RegisterConfig<RestConfiguration>(context.Configuration, "RestConfiguration");
                    services.RegisterConfig<RefreshFrequencyConfig>(context.Configuration, "RefreshFrequency");

                    services.AddScoped<Etl>();
                    services.AddScoped<DataWarehousePipeline>();
                    
                    services.AddScoped<IRefreshesService, RefreshesService>();
                    services.AddScoped<RestClientExtensions>();
                    
                    services.AddScoped<SymbolService>();
                    services.AddScoped<SymbolRetriever>();
                    
                    services.AddScoped<StockService>();
                    services.AddScoped<StockRetriever>();
                    
                    services.AddScoped<DailyPriceService>();
                    services.AddScoped<DailyPriceRetriever>();
                    
                    services.AddScoped<StockPriceService>();
                    services.AddScoped<PriceRetriever>();
                    
                    services.AddScoped<HistoricDailyService>();
                    services.AddScoped<HistoricDailyRetriever>();

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