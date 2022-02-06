using System;

namespace StockPredictor.DataRetriever.Domain
{
    public class Result<T>
    {
        public T SuccessResult { get; }
        public Exception Error { get; }
        public bool HasError => Error != null;
        
        public Result(T successResult)
        {
            SuccessResult = successResult;
        }

        public Result(Exception error)
        {
            Error = error;
        }
    }
}
