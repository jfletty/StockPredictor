namespace StockPredictor.DataRetriever.Domain.Tables.Api.Price
{
    public class ApiPrice
    {
        public int MaxAge { get; set; }

        public PriceDetail PreMarketChangePercent { get; set; }
        public PriceDetail PreMarketChange { get; set; }
        public long PreMarketTime { get; set; }
        public PriceDetail PreMarketPrice { get; set; }
        public string PreMarketSource { get; set; }

        public PriceDetail PostMarketChangePercent { get; set; }
        public PriceDetail PostMarketChange { get; set; }
        public long PostMarketTime { get; set; }
        public PriceDetail PostMarketPrice { get; set; }
        public string PostMarketSource { get; set; }

        public PriceDetail RegularMarketChangePercent { get; set; }
        public PriceDetail RegularMarketChange { get; set; }
        public long RegularMarketTime { get; set; }
        public PriceDetail RegularMarketPrice { get; set; }
        public PriceDetail PriceHint { get; set; }
        public PriceDetail RegularMarketDayHigh { get; set; }
        public PriceDetail RegularMarketDayLow { get; set; }
        public PriceDetail RegularMarketVolume { get; set; }

        public PriceDetail RegularMarketPreviousClose { get; set; }
        public string RegularMarketSource { get; set; }
        public PriceDetail RegularMarketOpen { get; set; }

        public string Exchange { get; set; }
        public int ExchangeDataDelayedBy { get; set; }
        public string MarketState { get; set; }
        public string QuoteType { get; set; }
        public string Symbol { get; set; }

        public PriceDetail MarketCap { get; set; }

        public string Currency { get; set; }
        public string LongName { get; set; }

    }

    public class PriceDetail
    {
        public decimal? Raw { get; set; }
    }
}
