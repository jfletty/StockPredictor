using System;
using System.Globalization;
using StockPredictor.Algorithm.Domain.Tables.Fact;

namespace StockPredictor.Algorithm.Domain.Tables.StoredProcedure
{
    public class DailyProjectionOutput
    {
        public int StockKey { get; set; }
        public decimal? PreMarketChange { get; set; }
        public decimal? PostMarketChange { get; set; }
        public string RegularMarketClose { get; set; }
        public decimal? RegularMarketHighLowChange { get; set; }
        public decimal? RegularMarketDayChange { get; set; }
        public long? MarketCap { get; set; }
        public short DateKey { get; set; }
        public short DateNumberInYear { get; set; }
        public byte WeekDay { get; set; }
        public byte DayNumberInMonth { get; set; }
        public byte MonthNumber { get; set; }
        public short Year { get; set; }
        public byte Quarter { get; set;}
        public byte FinancialQuarter { get; set; }
        public bool IsFirstDayOfMonth { get; set; }
        public bool IsLastDayOfMonth { get; set; }
        public bool IsFirstDayOfFinancialYear { get; set; }
        public bool IsLastDayOfFinancialYear { get; set; }
        public bool IsFirstDayOfWeek { get; set; }

        public DailyPrediction AsPrediction()
        {
            return new DailyPrediction
            {
                DateKey = DateKey,
                StockKey = StockKey,
                GeneratedAtDateTime = DateTime.UtcNow,
                PredictedClose = decimal.Parse(RegularMarketClose, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint)
            };
        }
    }
}   