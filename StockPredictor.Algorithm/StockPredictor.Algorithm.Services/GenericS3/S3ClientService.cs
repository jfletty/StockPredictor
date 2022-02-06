using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using StockPredictor.Algorithm.Domain;

namespace StockPredictor.Algorithm.Services.GenericS3
{
    public class S3ClientService
    {
        private readonly AmazonS3Client _s3Client;

        private const string _projectionOutputBucketName = "daily-projection-output";

        public S3ClientService()
        {
            _s3Client = new AmazonS3Client(RegionEndpoint.APSoutheast2);
        }

        public async Task<Result<bool>> UploadFilesAsync(string bucketName, int stockKey, string stockData)
        {
            try
            {
                var utility = new TransferUtility(_s3Client);
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(stockData));
                await utility.UploadAsync(stream, bucketName, $"{stockKey}.csv");

                return new Result<bool>(true);
            }
            catch (Exception e)
            {
                return new Result<bool>(e);
            }
        }

        public async Task<Result<List<KeyValuePair<string, string>>>> DownloadProjectionsAsync()
        {
            var result = new ConcurrentBag<KeyValuePair<string, string>>();
            try
            {
                var s3Objects = await GetObjectsInBucket(_projectionOutputBucketName);

                var tasks = s3Objects.Select(async s3Object =>
                {
                    var request = new GetObjectRequest
                    {
                        BucketName = _projectionOutputBucketName,
                        Key = s3Object.Key
                    };

                    using (var response = await _s3Client.GetObjectAsync(request))
                    await using (var responseStream = response.ResponseStream)
                    using (var reader = new StreamReader(responseStream))
                    {
                        result.Add(new KeyValuePair<string, string>(s3Object.Key, await reader.ReadToEndAsync()));
                    }
                });

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                return new Result<List<KeyValuePair<string, string>>>(e);
            }
            return new Result<List<KeyValuePair<string, string>>>(result.ToList());
        }

        public async Task<List<S3Object>> GetObjectsInBucket(string bucketName)
        {
            var request = new ListObjectsV2Request { BucketName = bucketName };

            var response = await _s3Client.ListObjectsV2Async(request);
            return response.S3Objects;
        }

        public async Task<Result<bool>> DeleteProjectionsAsync(IEnumerable<string> objectKeys)
        {
            try
            {
                var tasks = objectKeys.Select(item => new DeleteObjectRequest { BucketName = _projectionOutputBucketName, Key = item }).Select(async request =>
                {
                    await _s3Client.DeleteObjectAsync(request);
                });
                await Task.WhenAll(tasks);

                return new Result<bool>(true);
            }
            catch (Exception e)
            {
                return new Result<bool>(e);
            }
        }
    }
}
