
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
            modelBuilder.Entity<IPOData>()
                .HasKey(i => i.Id);

            // Important: Indicate that the ID is NOT database-generated
            modelBuilder.Entity<IPOData>()
                .Property(i => i.Id)
                .ValueGeneratedNever();

            // MarketData Table
            modelBuilder.Entity<MarketData>()
               .HasKey(i => i.ISIN);

            // Important: Indicate that the ID is NOT database-generated
            modelBuilder.Entity<MarketData>()
                .Property(i => i.ISIN)
                .ValueGeneratedNever();

            // BStocks Table
            modelBuilder.Entity<BStockData>()
               .HasKey(i => i.Id);

            // Important: Indicate that the ID is NOT database-generated
            modelBuilder.Entity<BStockData>()
                .Property(i => i.Id)
                .ValueGeneratedNever();
        }

        public DbSet<IPOData> Ipo {  get; set; }

        public DbSet<MarketData> Market { get; set; }

        public DbSet<BStockData> BStocks { get; set; }
    }
}
