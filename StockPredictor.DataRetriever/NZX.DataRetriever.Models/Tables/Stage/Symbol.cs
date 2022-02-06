using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.DataRetriever.Domain.Tables.Stage
{
    [Table("Symbol", Schema = "Stage")]
    public class Symbol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SymbolKey { get; set; }
        public string ExchangeKey { get; set; }

        public string SymbolValue { get; set; }
        public string ExchangeValue { get; set; }

        public bool IsDisabled { get; set; }
        public DateTime? DisabledDate { get; set; }
        
        public void Update(Symbol symbol)
        {
            SymbolValue = symbol.SymbolValue;
            ExchangeKey = symbol.ExchangeKey;
            ExchangeValue = symbol.ExchangeValue;
        }
        
        public bool Equals(Symbol obj)
        {
            if (obj.SymbolValue != SymbolValue) return false;
            if (obj.ExchangeKey != ExchangeKey) return false;
            if (obj.ExchangeValue != ExchangeValue) return false;

            return true;
        }
    }
}
