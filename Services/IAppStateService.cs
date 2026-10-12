using HomeDataCollector.Enums;
using HomeDataCollector.Models;

namespace HomeDataCollector.Services
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
