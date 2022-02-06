using System;

namespace StockPredictor.Algorithm.Domain
{
    public class Result<T>
    {
        public T SuccessResult { get; set; }
        public Exception Error { get; set; }
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
