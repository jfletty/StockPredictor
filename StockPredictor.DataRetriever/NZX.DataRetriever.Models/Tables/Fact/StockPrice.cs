using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.DataRetriever.Domain.Tables.Fact
{
    [Table("StockPrice", Schema = "Fact")]
    public class StockPrice
    {
        public static readonly IEqualityComparer<StockPrice> EqualityComparer = new StockPriceEqualityComparer();
        
        [Key, Column(Order = 0)]
        public int StockKey { get; set; }
        [Key, Column(Order = 1)]
        public short DateKey { get; set; }
        [Key, Column(Order = 2)]
        public short TimeOfDayKey { get; set; }

        public decimal Price { get; set; }
        public long Volume { get; set; }

        public string ExternalKey { get; set; }

        [NotMapped] public string Symbol { get; set; }
    }

    // ReSharper disable PossibleNullReferenceException
    public class StockPriceEqualityComparer : IEqualityComparer<StockPrice>
    {
        public bool Equals(StockPrice x, StockPrice y)
        {
            return x.StockKey == y.StockKey
                   && x.DateKey == y.DateKey
                   && x.TimeOfDayKey == y.TimeOfDayKey
                   && x.ExternalKey == y.ExternalKey
                   && Math.Round(x.Price, 2) == Math.Round(y.Price, 2)
                   && x.Volume == y.Volume;
        }

        public int GetHashCode(StockPrice obj)
        {
            throw new NotImplementedException();
        }
    }
    // ReSharper restore PossibleNullReferenceException
}
