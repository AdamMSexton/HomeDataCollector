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
    }
}
