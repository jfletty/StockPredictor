using System;

namespace StockPredictor.DataRetriever.Domain.Tables.Api
{
    public class ApiCompany
    {
        public string IssuerCode { get; set; }
        public string IssuerName { get; set; }
        public string IssuerShortName { get; set; }
        public string Country { get; set; }
        public int EndOfFinancialYear { get; set; }
        public char CompanyType { get; set; }
        public char ListingStatus { get; set; }
        public DateTime FirstListedDate { get; set; }
        public DateTime LastListedDate { get; set; }
        public string SectorCode { get; set; }
        public char SuspensionStatus { get; set; }
    }
}
// Security Issuer Details

