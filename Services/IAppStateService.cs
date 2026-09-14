namespace HealthDataCollector.Services
{
    public interface IAppStateService
    {
        bool IsEnabled();

        void Enable();

        void Disable();
    }
}
