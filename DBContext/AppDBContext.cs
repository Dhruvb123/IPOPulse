
using IPOPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace IPOPulse.DBContext
{
    public class AppDBContext: DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IPOData>()
                .HasKey(i => i.Id);

            // Important: Indicate that the ID is NOT database-generated
            modelBuilder.Entity<IPOData>()
                .Property(i => i.Id)
                .ValueGeneratedNever();
        }


        public DbSet<IPOData> Ipo {  get; set; }
    }
}
