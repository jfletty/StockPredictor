using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace StockPredictor.DataRetriever.Services.Infrastructure
{
    public class DataContextProvider
    {
        private const string ConnectionString = "";

        public DataWarehouseContext DataWarehouse()
        {
            var dw = DataWarehouseConnectionString();
            var stage = new DbContextOptionsBuilder<DataWarehouseContext>();
            stage.UseSqlServer(dw, x => x.EnableRetryOnFailure());
            stage.EnableSensitiveDataLogging();

            return new DataWarehouseContext(stage.Options);
        }

        private static string DataWarehouseConnectionString()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString) { TrustServerCertificate = true };
            return builder.ConnectionString;
        }
    }
}
