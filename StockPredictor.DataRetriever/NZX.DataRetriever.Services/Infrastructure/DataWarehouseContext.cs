using StockPredictor.DataRetriever.Domain.Tables.Dimension;
using StockPredictor.DataRetriever.Domain.Tables.Fact;
using StockPredictor.DataRetriever.Domain.Tables.Stage;
using StockPredictor.DataRetriever.Domain.Tables.StoredProcedure;
using Microsoft.EntityFrameworkCore;

namespace StockPredictor.DataRetriever.Services.Infrastructure
{
    public class DataWarehouseContext : DbContext
    {
        public DataWarehouseContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<Stock> Stock { get; set; }
        public virtual DbSet<Date> Date { get; set; }
        public virtual DbSet<Refresh> Refresh { get; set; }
        public virtual DbSet<Exchange> Exchange { get; set; }
        public virtual DbSet<Symbol> Symbol { get; set; }
        public virtual DbSet<StockPrice> StockPrice { get; set; }
        public virtual DbSet<DailyPriceRecap> DailyPriceRecap { get; set; }

        public virtual DbSet<RefreshModel> RefreshModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StockPrice>()
                .HasKey(x => new { x.StockKey, x.DateKey, x.TimeOfDayKey });

            modelBuilder.Entity<Symbol>()
                .HasKey(x => new { x.SymbolKey, x.ExchangeKey });

            modelBuilder.Entity<DailyPriceRecap>()
                .HasKey(x => new { x.StockKey, x.DateKey });

            modelBuilder.Entity<Refresh>()
                .HasKey(x => new { x.ExternalKey, x.RefreshType });
        }
    }
}
