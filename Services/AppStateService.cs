using HealthDataCollector.Models;
using HealthDataCollector.Enums;
using HealthDataCollector.Data.Control.Entities;
using HealthDataCollector.Data.Control;
using Microsoft.EntityFrameworkCore;

namespace HealthDataCollector.Services
{
    public class AppStateService : IAppStateService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        
        private DateTimeOffset? _enabledUntil;

        private readonly Dictionary<string, Setting> _settings = [];

        public AppStateService(IServiceScopeFactory scopeFactory) 
        {
            _scopeFactory = scopeFactory;
        }


        public bool IsEnabled()
        {
            if ((_enabledUntil.HasValue) && (DateTimeOffset.UtcNow < _enabledUntil.Value))
            {
                return true;
            }

            return false;
        }

        public void Enable()
        {
            _enabledUntil = DateTimeOffset.UtcNow.AddMinutes(5);
        }

        public void Disable()
        {
            _enabledUntil = null;
        }

        public async Task<ServiceResult<bool>> LoadSettingsAsync()
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var db = scope.ServiceProvider.GetRequiredService<ControlDbContext>();

            try
            {
                var settings = await db.Settings
                    .AsNoTracking()
                    .ToListAsync();
            
                _settings.Clear();
                      
                foreach (var setting in settings)
                {
                    _settings[setting.Key] = setting;
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail(ServiceError.Database, ex.Message);
            }

            return ServiceResult<bool>.Ok(true);

        }

        public T GetSetting<T>(SettingName keyName)
        {
            string key = keyName.ToString();

            if (!_settings.TryGetValue(key, out var setting))
            {
                throw new KeyNotFoundException(
                    $"Setting '{key}' was not found.");
            }

            if (setting.Value is null)
            {
                throw new InvalidOperationException(
                    $"Setting '{key}' has no value.");
            }

            SettingDataType requestedType = typeof(T) switch
            {
                var t when t == typeof(string) => SettingDataType.String,
                var t when t == typeof(int) => SettingDataType.Integer,
                var t when t == typeof(decimal) => SettingDataType.Decimal,
                var t when t == typeof(bool) => SettingDataType.Boolean,

                _ => throw new NotSupportedException(
                    $"Setting type '{typeof(T).Name}' is not supported.")
            };

            if (setting.DataType != requestedType)
            {
                throw new InvalidOperationException(
                    $"Setting '{key}' is stored as {setting.DataType}, " +
                    $"but was requested as {requestedType}.");
            }

            return (T)Convert.ChangeType(setting.Value, typeof(T));
        }
    }
}
