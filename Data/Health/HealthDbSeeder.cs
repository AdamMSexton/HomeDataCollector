using Microsoft.EntityFrameworkCore;

namespace HealthDataCollector.Data.Health
{
    public static class HealthDbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<HealthDbContext>();

            await db.Database.MigrateAsync();
        }
    }
}