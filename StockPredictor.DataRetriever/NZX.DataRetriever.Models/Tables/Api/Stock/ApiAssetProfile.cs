using System.Collections.Generic;

namespace StockPredictor.DataRetriever.Domain.Tables.Api.Stock
{
    public class ApiAssetProfile
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }

        public string Website { get; set; }
        public string Industry { get; set; }
        public string Sector { get; set; }

        public int? FullTimeEmployees { get; set; } = 0;
        public List<CompanyOfficers> CompanyOfficers { get; set; }

        public int AuditRisk { get; set; }
        public int BoardRisk { get; set; }
        public int CompensationRisk { get; set; }
        public int ShareHolderRightsRisk { get; set; }
        public int OverallRisk { get; set; }
        public long GovernanceEpochDate { get; set; }
        public long CompensationAsOfEpochDate { get; set; }
        public int MaxAge { get; set; }
        public string Symbol { get; set; }
    }

    public class CompanyOfficers
    {
        public int MaxAge { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Title { get; set; }
        public TotalPay TotalPay { get; set; }
    }

    public class TotalPay
    {
        public long Raw { get; set; }
    }
}
