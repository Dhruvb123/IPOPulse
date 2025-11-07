
using Microsoft.EntityFrameworkCore;

namespace IPOPulse.DBContext
{
    public class AppDBContext: DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }
    }
}
