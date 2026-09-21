using Microsoft.EntityFrameworkCore;

namespace HealthDataCollector.Data.Health
{
    public static class HealthDbSeeder
    {
        public static async Task SeedAsync(HealthDbContext db)
        {
            // Health DB seed data
            await Task.CompletedTask;
        }
    }
}