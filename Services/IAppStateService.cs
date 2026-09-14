using HealthDataCollector.Models;

namespace HealthDataCollector.Services
{
    public interface IAppStateService
    {
        bool IsEnabled();

        void Enable();

        void Disable();
        Task<ServiceResult<bool>> LoadSettingsAsync();
    }
}
