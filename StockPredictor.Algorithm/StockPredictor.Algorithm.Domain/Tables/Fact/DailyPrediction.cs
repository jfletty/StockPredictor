using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.Algorithm.Domain.Tables.Fact
{
    [Table("DailyPrediction", Schema = "Fact")]
    public class DailyPrediction
    {
        public short DateKey { get; set; }
        public int StockKey { get; set; }

        public DateTime GeneratedAtDateTime { get; set; }

        public decimal PredictedClose { get; set; }
    }
}
