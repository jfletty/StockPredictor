using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.DataRetriever.Domain.Tables.Fact
{
    [Table("StockDividend", Schema = "Fact")]
    public class StockDividend
    {
        public int StockKey { get; set; }
        public int DateKey { get; set; }

        public double DividendPerShare { get; set; }
    }
}