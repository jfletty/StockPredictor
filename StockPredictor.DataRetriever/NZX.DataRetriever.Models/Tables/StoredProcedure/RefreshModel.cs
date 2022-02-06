using System.ComponentModel.DataAnnotations;

namespace StockPredictor.DataRetriever.Domain.Tables.StoredProcedure
{
    public class RefreshModel
    {
        [Key]
        public string ExchangeKey { get; set; }
    }
}
