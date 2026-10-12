using HomeDataCollector.Data.Control.Entities;
using HomeDataCollector.Enums;
using Microsoft.EntityFrameworkCore;

namespace HomeDataCollector.Data.Control
{
    public class ControlDbSeeder
    {
        public static async Task SeedAsync(ControlDbContext db)
        {
            if (!await db.Settings.AnyAsync(x => x.Key == "HealthDbConnectionString"))
            {
                db.Settings.Add(new Setting
                {
                    Key = SettingName.HealthDbConnectionString.ToString(),
                    Value = "",
                    DataType = SettingDataType.String
                });
            }

            if (!await db.Settings.AnyAsync(x => x.Key == "ApiKeyLifetimeDays"))
            {
                db.Settings.Add(new Setting
                {
                    Key = SettingName.ApiKeyLifetimeDays.ToString(),
                    Value = "90",
                    DataType = SettingDataType.Integer,
                    MinValue = "1",
                    MaxValue = "365"
                });
            }

            if (!await db.Settings.AnyAsync(x => x.Key == "ApiKeyRenewalWindowDays"))
            {
                db.Settings.Add(new Setting
                {
                    Key = SettingName.ApiKeyRenewalWindowDays.ToString(),
                    Value = "7",
                    DataType = SettingDataType.Integer,
                    MinValue = "1",
                    MaxValue = "90"
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
