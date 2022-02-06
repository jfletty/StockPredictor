using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace StockPredictor.DataRetriever.Domain.Tables.Api
{
    public class ApiSymbol
    {
        public string Currency { get; set; }
        public string Description { get; set; }
        public string DisplaySymbol { get; set; }
        public string Type { get; set; }
        public string Symbol { get; set; }

        [NotMapped] public string SymbolValue => Symbol.Split('.')[0];
        [NotMapped] public string ExchangeValue => Symbol.Split('.').Length == 1 ? null : Symbol.Split('.').Last();
    }
}