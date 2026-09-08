using Microsoft.EntityFrameworkCore;

namespace HealthDataCollector.Data.Control
{
    public class ControlDbContext : DbContext
    {
        public ControlDbContext(DbContextOptions<ControlDbContext> options)
            : base(options)
        {
        }
    }
}
