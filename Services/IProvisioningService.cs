namespace HealthDataCollector.Services
{
    public interface IProvisioningService
    {
        bool IsEnabled();

        void Enable();

        void Disable();
    }
}
