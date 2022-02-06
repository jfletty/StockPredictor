using Microsoft.Extensions.Logging;
using RestSharp;

namespace StockPredictor.DataRetriever.Domain.Extensions
{
    public static class LogExtensions
    {
        public static void LogErrorMessage(this ILogger logger, string message, params object[] args)
        {
            var client = new RestClient("");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-type", "application/json");
            request.AddParameter("application/json", $"{{\"text\":\"{message}!\"}}", ParameterType.RequestBody);
            client.Post(request);
            logger.Log(LogLevel.Error, message, args);
        }
    }
}
