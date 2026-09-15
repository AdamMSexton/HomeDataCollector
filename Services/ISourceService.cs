using HealthDataCollector.Models;

namespace HealthDataCollector.Services
{
    public interface ISourceService
    {
        bool IsEnabled();

        void Enable();

        void Disable();
        Task<ServiceResult<ProvisionResponseDto>> ProvisionNewDevice(string correlationId, ProvisionRequestDto request);
    }
}
