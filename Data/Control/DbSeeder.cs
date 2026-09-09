using HealthDataCollector.Data.Control.Entities;
using HealthDataCollector.Enums;
using Microsoft.EntityFrameworkCore;

namespace HealthDataCollector.Data.Control
{
    public class DbSeeder
    {
        public static async Task SeedAsync(ControlDbContext db)
        {
            if (!await db.Settings.AnyAsync(x => x.Key == "HealthDbConnectionString"))
            {
                db.Settings.Add(new Setting
                {
                    Key = "HealthDbConnectionString",
                    Value = "",
                    DataType = SettingDataType.String
                });
            }

            if (!await db.Settings.AnyAsync(x => x.Key == "ApiKeyLifetimeDays"))
            {
                db.Settings.Add(new Setting
                {
                    Key = "ApiKeyLifetimeDays",
                    Value = "90",
                    DataType = SettingDataType.Integer,
                    MinValue = "1",
                    MaxValue = "365"
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
