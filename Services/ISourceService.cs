using HealthDataCollector.Models;

namespace HealthDataCollector.Services
{
    public interface ISourceService
    {
        bool IsEnabled();

        void Enable();

        void Disable();
        Task<ServiceResult<ApiKeyIssueDto>> ProvisionNewDevice(string correlationId, ProvisionRequestDto request);
        Task<ServiceResult<Guid>> ValidateApiKeyAsync(string providedApiKey);
    }
}
