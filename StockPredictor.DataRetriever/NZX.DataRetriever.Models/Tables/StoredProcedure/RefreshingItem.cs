using System.ComponentModel.DataAnnotations;

namespace StockPredictor.DataRetriever.Domain.Tables.StoredProcedure
{
    public class RefreshingItem
    {
        [Key]
        public string ExternalKey { get; set; }
    }
}
