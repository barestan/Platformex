using Microsoft.EntityFrameworkCore;

namespace Demo.Infrastructure.Data
{
    public class DemoContext : DbContext
    {
        public DemoContext(DbContextOptions<DemoContext> options) : base(options)
        {
        }

        public DbSet<CarModel> Cars { get; set; }
    }
}