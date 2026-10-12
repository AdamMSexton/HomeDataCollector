using HomeDataCollector.Models;

namespace HomeDataCollector.Services
{
    public interface ISourceService
    {
        bool IsEnabled();

        void Enable();

        void Disable();
        Task<ServiceResult<ApiKeyIssueDto>> ProvisionNewDevice(string correlationId, ProvisionRequestDto request);
        Task<ServiceResult<Guid>> ValidateApiKeyAsync(string providedApiKey);
        Task<ServiceResult<ApiKeyIssueDto>> StartApiRenewalAsync(string correlationId, string providedApiKey);
    }
}
