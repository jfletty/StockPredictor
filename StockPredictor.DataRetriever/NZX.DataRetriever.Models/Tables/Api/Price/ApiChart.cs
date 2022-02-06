using System.Collections.Generic;
using System.Linq;

namespace StockPredictor.DataRetriever.Domain.Tables.Api.Price
{
    public class ApiStockPrice
    {
        public ApiChart Chart { get; set; }
    }

    public class ApiChart
    {
        public List<ApiPriceResult> Result { get; set; }
        public ApiError Error { get; set; }

        public List<MappedPrices> GetFromList()
        {
            var data = Result?.FirstOrDefault() ?? new ApiPriceResult();
            if (data.Timestamp == null)
                return new List<MappedPrices>();

            return data.Timestamp.Select((t,
                x) => new MappedPrices
                {
                    Symbol = data.Meta.Symbol,
                    Timestamp = t.GetValueOrDefault(),
                    Close = data.Indicators.Quote[0]
                            .Close[x] ==
                        null
                    ? 0
                    : data.Indicators.Quote[0]
                        .Close[x]
                        .Value,
                    Volume = data.Indicators.Quote[0]
                             .Volume[x] ==
                         null
                    ? 0
                    : data.Indicators.Quote[0]
                        .Volume[x]
                        .Value,
                    Open = data.Indicators.Quote[0]
                             .Open[x] ==
                         null
                    ? 0
                    : data.Indicators.Quote[0]
                        .Open[x]
                        .Value,
                    ExternalKey = $"{data.Meta.Symbol}.{t.GetValueOrDefault()}"
                }).Where(x => x.Timestamp % 10 == 0 && x.Close != 0 && x.Volume != 0).ToList();
        }
    }

    public class ApiPriceResult
    {
        public ApiMeta Meta { get; set; }
        public List<long?> Timestamp { get; set; }
        public ApiIndicators Indicators { get; set; }
    }

    public class ApiMeta
    {
        public string Symbol { get; set; }
    }

    public class ApiIndicators
    {
        public List<ApiPriceQuote> Quote { get; set; }
    }

    public class ApiPriceQuote
    {
        public List<decimal?> Low { get; set; }
        public List<decimal?> Close { get; set; }
        public List<decimal?> Open { get; set; }
        public List<decimal?> High { get; set; }
        public List<long?> Volume { get; set; }
    }

    public class MappedPrices
    {
        public string Symbol { get; set; }
        public long Timestamp { get; set; }
        public decimal Close { get; set; }
        public decimal Low { get; set; }
        public decimal High { get; set; }
        public decimal Open { get; set; }
        public long Volume { get; set; }
        public string ExternalKey { get; set; }
    }
}
