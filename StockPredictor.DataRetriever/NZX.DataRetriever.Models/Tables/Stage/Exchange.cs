using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.DataRetriever.Domain.Tables.Stage
{
    [Table("Exchange", Schema = "Stage")]
    public class Exchange
    {
        [Key]
        public string ExchangeKey { get; set; }
        
        public string TimeZone { get; set; }
    }
}
