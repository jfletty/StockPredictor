using System.Collections.Generic;
using StockPredictor.DataRetriever.Domain.Tables.Api.Price;

namespace StockPredictor.DataRetriever.Domain.Tables.Api.Stock
{
    public class ApiStock
    {
        public ApiQuoteSummary QuoteSummary { get; set; }
    }

    public class ApiQuoteSummary
    {
        public List<ApiStockResult> Result { get; set; }
        public ApiError Error { get; set; }
    }

    public class ApiStockResult
    {
        public ApiAssetProfile AssetProfile { get; set; }
        public ApiPrice Price { get; set; }
    }
}
