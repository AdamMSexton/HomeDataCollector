using HealthDataCollector.Enums;
using HealthDataCollector.Models;

namespace HealthDataCollector.Services
{
    public interface IAppStateService
    {
        bool IsEnabled();

        void Enable();

        void Disable();
        
        Task<ServiceResult<bool>> LoadSettingsAsync();

        T GetSetting<T>(SettingName keyName);
    }
}
