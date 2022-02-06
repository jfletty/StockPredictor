namespace StockPredictor.Algorithm.Domain.Tables.StoredProcedure
{
    public class DailyProjectionInput
    {
        public int StockKey { get; set; }
        public decimal? PreMarketChange { get; set; } = -99999;
        public decimal? PostMarketChange { get; set; } = -99999;
        public decimal? RegularMarketClose { get; set; } = -99999;
        public decimal? RegularMarketHighLowChange { get; set; } = -99999;
        public decimal? RegularMarketDayChange { get; set; } = -99999;
        public long? MarketCap { get; set; } = -99999;
        public short DateKey { get; set; }
        public short DateNumberInYear { get; set; }
        public byte WeekDay { get; set; }
        public byte DayNumberInMonth { get; set; }
        public byte MonthNumber { get; set; }
        public short Year { get; set; }
        public byte Quarter { get; set; }
        public byte FinancialQuarter { get; set; }
        public bool IsFirstDayOfMonth { get; set; }
        public bool IsLastDayOfMonth { get; set; }
        public bool IsFirstDayOfFinancialYear { get; set; }
        public bool IsLastDayOfFinancialYear { get; set; }
        public bool IsFirstDayOfWeek { get; set; }

        public void UpdateZeros()
        {
            PreMarketChange = PreMarketChange == 0 ? -99999 : PreMarketChange;
            PostMarketChange = PostMarketChange == 0 ? -99999 : PostMarketChange;
            RegularMarketClose = RegularMarketClose == 0 ? -99999 : RegularMarketClose;
            RegularMarketHighLowChange = RegularMarketHighLowChange == 0 ? -99999 : RegularMarketHighLowChange;
            RegularMarketDayChange = RegularMarketDayChange == 0 ? -99999 : RegularMarketDayChange;
            MarketCap = MarketCap == 0 ? -99999 : MarketCap;
        }
    }
}