using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.DataRetriever.Domain.Tables.Dimension
{
    [Table("Sentiment", Schema = "Dim")]
    public class Sentiment
    {
        [Key]
        public int SentimentKey { get; set; }
        public string SentimentValue { get; set; }
    }
}
