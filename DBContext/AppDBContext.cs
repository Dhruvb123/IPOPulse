
using IPOPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace IPOPulse.DBContext
{
    public class AppDBContext: DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // IPO Table
            modelBuilder.Entity<IPOData>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Id).ValueGeneratedNever();
            });

            // MarketData Table
            modelBuilder.Entity<MarketData>(entity =>
            {
                entity.HasKey(i => i.ISIN);
                entity.Property(i => i.ISIN).ValueGeneratedNever();
            });

            // BStocks Table
            modelBuilder.Entity<BStockData>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Id).ValueGeneratedNever();
            });
        }

        public DbSet<IPOData> Ipo { get; set; }
        public DbSet<MarketData> Market { get; set; }
        public DbSet<BStockData> BStocks { get; set; }
        public DbSet<UserModel> Users { get; set; }

    }
}
