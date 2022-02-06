using Microsoft.EntityFrameworkCore;
using StockPredictor.Algorithm.Domain.Tables.Dimension;
using StockPredictor.Algorithm.Domain.Tables.Fact;
using StockPredictor.Algorithm.Domain.Tables.Stage;
using StockPredictor.Algorithm.Domain.Tables.StoredProcedure;

namespace StockPredictor.Algorithm.Services.Infrastructure
{
    public class DataWarehouseContext : DbContext
    {
        public DataWarehouseContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<Date> Date { get; set; }
        public virtual DbSet<TimeOfDay> TimeOfDay { get; set; }
        public virtual DbSet<DailyProjectionInput> DailyProjectionInput { get; set; }
        public virtual DbSet<ProjectionSetting> ProjectionSetting { get; set; }
        public virtual DbSet<StocksRequiringProjections> StocksRequiringProjections { get; set; }
        public virtual DbSet<StocksRequiringModels> StocksRequiringModels { get; set; }
        public virtual DbSet<DailyPrediction> DailyPrediction { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StocksRequiringProjections>()
                .HasKey(x => new { x.StockKey });

            modelBuilder.Entity<StocksRequiringModels>()
                .HasKey(x => new { x.StockKey });

            modelBuilder.Entity<DailyProjectionInput>()
                .HasKey(x => new { x.StockKey, x.DateKey });

            modelBuilder.Entity<DailyPrediction>()
                .HasKey(x => new { x.StockKey, x.DateKey });
        }
    }
}
