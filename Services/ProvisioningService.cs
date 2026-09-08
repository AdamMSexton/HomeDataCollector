namespace HealthDataCollector.Services
{
    public class ProvisioningService : IProvisioningService
    {
        private DateTimeOffset? _enabledUntil;

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
    }
}
