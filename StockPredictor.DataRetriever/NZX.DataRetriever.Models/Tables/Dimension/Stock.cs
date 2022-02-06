using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.DataRetriever.Domain.Tables.Dimension
{
    [Table("Stock", Schema = "Dim")]
    public class Stock
    {
        [Key]
        public int StockKey { get; set; }
        public int SymbolKey { get; set; }
        public string LongName { get; set; }
        public string Currency { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
        public string Website { get; set; }
        public string IndustrySector { get; set; }

        public bool IsBlackListed { get; set; }
        public DateTime? BlackListedDate { get; set; }

        [NotMapped]
        public string Symbol { get; set; }
        
        public void Update(Stock stock)
        {
            SymbolKey = stock.SymbolKey;
            LongName = stock.LongName;
            Currency = stock.Currency;
            StreetAddress = stock.StreetAddress;
            City = stock.City;
            PostCode = stock.PostCode;
            Country = stock.Country;
            Website = stock.Website;
            IndustrySector = stock.IndustrySector;
        }
        
        public bool Equals(Stock obj)
        {
            if (obj.SymbolKey != SymbolKey) return false;
            if (obj.LongName != LongName) return false;
            if (obj.StreetAddress != StreetAddress) return false;
            if (obj.Currency != Currency) return false;
            if (obj.City != City) return false;
            if (obj.PostCode != PostCode) return false;
            if (obj.Country != Country) return false;
            if (obj.Website != Website) return false;
            if (obj.IndustrySector != IndustrySector) return false;

            return true;
        }
    }
}
