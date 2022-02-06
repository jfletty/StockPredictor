using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace StockPredictor.DataRetriever.Domain.Extensions
{
    public abstract class ModuleExtensions
    {
        public void Configure(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Load(services, configuration, hostEnvironment);
        }

        protected abstract void Load(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment);
    }
}
