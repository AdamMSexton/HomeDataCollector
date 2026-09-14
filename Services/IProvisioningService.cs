using HealthDataCollector.Models;

namespace HealthDataCollector.Services
{
    public interface IProvisioningService
    {
        bool IsEnabled();

        void Enable();

        void Disable();
        Task<ServiceResult<ProvisionResponseDto>> ProvisionNewClient(string correlationId, ProvisionRequestDto request);
    }
}
