using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StockPredictor.Algorithm.Services.GenericS3;

namespace StockPredictor.Algorithm.Services.CleanUp
{
    public class S3Retriever
    {
        private readonly S3ClientService _s3Service;
        private readonly ILogger<S3Retriever> _logger;

        public S3Retriever(S3ClientService s3Service, ILogger<S3Retriever> logger)
        {
            _s3Service = s3Service;
            _logger = logger;
        }

        public async Task<List<KeyValuePair<string, string>>> DownloadAllAsync()
        {
            var result = await _s3Service.DownloadProjectionsAsync();

            if (result.HasError)
            {
                _logger.LogError(result.Error, "_s3Service.DownloadProjectionsAsync");
                return null;
            }

            _logger.LogInformation("Successfully Downloaded Projection Results from S3");
            return result.SuccessResult;
        }

        public async Task DeleteItemsAsync(IEnumerable<string> objectKeys)
        {
            var result = await _s3Service.DeleteProjectionsAsync(objectKeys);

            if (result.HasError)
            {
                _logger.LogError(result.Error, "_s3Service.DeleteProjectionsAsync");
                return;
            }

            _logger.LogInformation("Successfully Deleted Projection Results from S3");
        }
    }
}
