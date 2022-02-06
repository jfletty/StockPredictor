using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.DataRetriever.Domain.Tables.Dimension
{
    [Table("Investor", Schema = "Dim")]
    public class Investor
    {
        [Key]
        public int InvestorKey { get; set; }
        public string InvestorName { get; set; }
    }
}
