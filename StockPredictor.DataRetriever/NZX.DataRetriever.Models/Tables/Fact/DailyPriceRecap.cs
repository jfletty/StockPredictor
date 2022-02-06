using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace StockPredictor.DataRetriever.Domain.Tables.Fact
{
    [Table("DailyPriceRecap", Schema = "Fact")]
    public class DailyPriceRecap
    {
        public static readonly IEqualityComparer<DailyPriceRecap> EqualityComparer = new DailyPriceRecapEqualityComparer();

        [Key, Column(Order = 0)]
        public int StockKey { get; set; }
        [Key, Column(Order = 1)]
        public short DateKey { get; set; }

        public decimal? PreMarketChange { get; set; }
        public decimal? PreMarketChangePercent { get; set; }

        public decimal? PostMarketChange { get; set; }
        public decimal? PostMarketChangePercent { get; set; }

        public decimal? RegularMarketPrice { get; set; }
        public decimal? RegularMarketLow { get; set; }
        public decimal? RegularMarketHigh { get; set; }
        public long? RegularMarketVolume { get; set; }

        public decimal? RegularMarketOpen { get; set; }
        public decimal? RegularMarketClose { get; set; }

        public long? MarketCap { get; set; }

        [NotMapped]
        public string Symbol { get; set; }
    }

    // ReSharper disable PossibleNullReferenceException
    public class DailyPriceRecapEqualityComparer : IEqualityComparer<DailyPriceRecap>
    {
        public bool Equals(DailyPriceRecap x, DailyPriceRecap y)
        {
            return x.StockKey == y.StockKey
                   && x.DateKey == y.DateKey
                   && Math.Round(x.PreMarketChange.GetValueOrDefault(0), 2) == Math.Round(y.PreMarketChange.GetValueOrDefault(0), 2)
                   && Math.Round(x.PreMarketChangePercent.GetValueOrDefault(0), 2) == Math.Round(y.PreMarketChangePercent.GetValueOrDefault(0), 2)
                   && Math.Round(x.PostMarketChange.GetValueOrDefault(0), 2) == Math.Round(y.PostMarketChange.GetValueOrDefault(0), 2)
                   && Math.Round(x.PostMarketChangePercent.GetValueOrDefault(0), 2) == Math.Round(y.PostMarketChangePercent.GetValueOrDefault(0), 2)
                   && Math.Round(x.RegularMarketLow.GetValueOrDefault(0), 2) == Math.Round(y.RegularMarketLow.GetValueOrDefault(0), 2)
                   && Math.Round(x.RegularMarketHigh.GetValueOrDefault(0), 2) == Math.Round(y.RegularMarketHigh.GetValueOrDefault(0), 2)
                   && x.RegularMarketVolume == y.RegularMarketVolume
                   && Math.Round(x.RegularMarketOpen.GetValueOrDefault(0), 2) == Math.Round(y.RegularMarketOpen.GetValueOrDefault(0), 2)
                   && x.MarketCap == y.MarketCap;
        }

        public int GetHashCode(DailyPriceRecap obj)
        {
            throw new NotImplementedException();
        }
    }
    // ReSharper restore PossibleNullReferenceException
}
