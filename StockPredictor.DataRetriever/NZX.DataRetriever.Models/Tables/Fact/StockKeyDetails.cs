using System;

namespace StockPredictor.DataRetriever.Domain.Tables.Fact
{
    public class StockKeyDetails
    {
        public int StockKey { get; set; }
        public int StaffCount { get; set; }

        public int AuditRisk { get; set; }
        public int BoardRisk { get; set; }
        public int CompensationRisk { get; set; }
        public int ShareHolderRightsRisk { get; set; }
        public int OverallRisk { get; set; }

        public int CompanyAge { get; set; }

        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
        public DateTime? ValidTo { get; set; }
    }
}
