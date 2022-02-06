using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StockPredictor.Algorithm.Services.GenericS3;

namespace StockPredictor.Algorithm.Services.Preparation
{
    public class S3Saver
    {
        private readonly S3ClientService _s3Service;
        private readonly ILogger<S3Saver> _logger;

        public S3Saver(S3ClientService s3Service, ILogger<S3Saver> logger)
        {
            _s3Service = s3Service;
            _logger = logger;
        }

        public async Task SaveAsync(string bucketName, int stockKey, string data)
        {
            var result = await _s3Service.UploadFilesAsync(bucketName, stockKey, data);

            if (result.HasError)
            {
                _logger.LogError(result.Error, $"_s3Service.UploadFilesAsync(). Bucket = {bucketName}");
                return;
            }

            _logger.LogInformation($"Successfully Uploaded Projection/Model  Input Data to S3. Bucket = {bucketName}");
        }
    }
}
