using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using StockPredictor.DataRetriever.Domain.Configuration;
using StockPredictor.DataRetriever.Domain.Helpers;

namespace StockPredictor.DataRetriever.Domain.Extensions
{
    public class RestClientExtensions
    {
        private readonly RestConfiguration _restConfiguration;

        public RestClientExtensions(RestConfiguration restConfiguration)
        {
            _restConfiguration = restConfiguration;
        }

        public async Task<Result<T>> GetAsync<T>(string remainingUri, string endpoint, Dictionary<string, string> parameters = null, List<KeyValuePair<string, string>> headers = null)
        {
            Result<T> result = null;
            try
            {
                await PollyHelper.WaitAndRetry<Exception>((exception, span) =>
               {
                   // Log Error
               }, async () =>
               {
                   var client = new RestClient(_restConfiguration.BaseUri + remainingUri);
                   var request = new RestRequest(endpoint);

                   if (headers != null)
                       request.AddHeaders(headers);

                   if (parameters != null)
                   {
                       foreach (var (name, value) in parameters)
                       {
                           request.AddParameter(name, value);
                       }
                   }

                   var temp = await client.GetAsync<T>(request);
                   result = new Result<T>(temp);
               });
            }
            catch (Exception e)
            {
                result = new Result<T>(e);
            }
            return result;
        }
    }
}