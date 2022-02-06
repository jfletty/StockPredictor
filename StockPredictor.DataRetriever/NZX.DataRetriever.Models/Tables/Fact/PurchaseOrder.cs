using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.DataRetriever.Domain.Tables.Fact
{
    [Table("PurchaseOrder", Schema = "Fact")]
    public class PurchaseOrder
    {
        public int InvestorKey { get; set; }
        public int StockKey { get; set; }
        public int Date { get; set; }
        public int TimeOfDay { get; set; }
        [Key]
        public Guid ExternalId { get; set; }
        public decimal Quantity { get; set; }
        public double Price { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}
