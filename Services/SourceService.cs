using HealthDataCollector.Data.Control;
using HealthDataCollector.Data.Control.Entities;
using HealthDataCollector.Enums;
using HealthDataCollector.Constants;
using HealthDataCollector.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace HealthDataCollector.Services
{
    public class SourceService : ISourceService
    {
        
        private readonly ControlDbContext _db;
        private readonly IAppStateService _ass;

        public SourceService(ControlDbContext db, IAppStateService ass) 
        {
            _db = db;
            _ass = ass;
        }

        public bool IsEnabled()
        {
            return _ass.IsEnabled();
        }

        public void Enable()
        {
            _ass.Enable();
        }

        public void Disable()
        {
            _ass.Disable();
        }

        public async Task<ServiceResult<ApiKeyIssueDto>> ProvisionNewDevice(string correlationId, ProvisionRequestDto request)
        {
            var sourceId = Guid.NewGuid();

            // Generate API key bytes
            byte[] apiKeyBytes = RandomNumberGenerator.GetBytes(32);

            // Convert API key bytes to Hex string
            string apiKey = Convert.ToHexString(apiKeyBytes);

            // Hash API key bytes for storage
            byte[] apiKeyHash = SHA256.HashData(apiKeyBytes);

            DateTimeOffset now = DateTimeOffset.UtcNow;

            ApiKey newApiRecord = new ApiKey
            {
                SourceId = sourceId,
                ApiKeyHash = apiKeyHash,
                CreatedAt = now,
                ExpiresAt = now.AddDays(_ass.GetSetting<int>(SettingName.ApiKeyLifetimeDays)),             // <=====  Change this to sue the settings when settings up and running.
                RenewedAt = null,
                Status = HealthDataCollector.Enums.KeyStatus.Active
            };

            Source newSourceRecord = new Source
            {
                SourceId = sourceId,
                Name = request.Name,
                DeviceType = request.DeviceType,
                Enabled = true,
                CreatedAt = now,
                LastSeenAt = now,
            };

            var provisioningResult = new ApiKeyIssueDto
            {
                SourceId = sourceId,
                ApiKey = apiKey,
                ApiKeyExpiration = newApiRecord.ExpiresAt,
                CorrelationId = correlationId
            };

            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                _db.Sources.Add(newSourceRecord);
                _db.ApiKeys.Add(newApiRecord);

                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult<ApiKeyIssueDto>.Fail(Enums.ServiceError.Database, ex.Message);
            }

            return ServiceResult<ApiKeyIssueDto>.Ok(provisioningResult);
        }
    
        public async Task<ServiceResult<Guid>> ValidateApiKeyAsync(string providedApiKey)
        {
            // Convert from Hex string to bytes
            byte[] apiKeyBytes;
            try
            {
                apiKeyBytes = Convert.FromHexString(providedApiKey);
            }
            catch (FormatException)
            {
                return ServiceResult<Guid>.Fail(ServiceError.ApiKeyNotFound, ServiceMessages.ApiKeyNotFound);
            }

            // Hash Api key bytes ti search DB
            byte[] apiKeyHash = SHA256.HashData(apiKeyBytes);

            ApiKey? matchedApiKey =  await _db.ApiKeys.FirstOrDefaultAsync(x => x.ApiKeyHash == apiKeyHash);

            if (matchedApiKey == null)
            {
                return ServiceResult<Guid>.Fail(ServiceError.ApiKeyNotFound, ServiceMessages.ApiKeyNotFound);
            }

            if (matchedApiKey.Status == KeyStatus.Active)
            {
                if (matchedApiKey.ExpiresAt <= DateTimeOffset.UtcNow)
                {
                    matchedApiKey.Status = KeyStatus.Expired;
                    await _db.SaveChangesAsync();
                }
            }

            switch (matchedApiKey.Status)
            {
                case KeyStatus.Expired:
                    {
                        return ServiceResult<Guid>.Fail(ServiceError.ApiKeyExpired, ServiceMessages.ApiKeyExpired);
                    }
                case KeyStatus.Revoked:
                    {
                        return ServiceResult<Guid>.Fail(ServiceError.ApiKeyRevoked, ServiceMessages.ApiKeyRevoked);
                    }
                case KeyStatus.RenewalOnly:
                    {
                        return ServiceResult<Guid>.Fail(ServiceError.ApiKeyRenewed, ServiceMessages.ApiKeyRenewed);
                    }
                case KeyStatus.Active:
                    {
                        return ServiceResult<Guid>.Ok(matchedApiKey.SourceId);
                    }

            }

            // Only get there if an invalid api status was attached to the key in DB.
            return ServiceResult<Guid>.Fail(ServiceError.Internal, "Unknown Error. Status: '" + matchedApiKey.Status.ToString() + "' is unknown.");
        }
    
        public async Task<ServiceResult<ApiKeyIssueDto>> RenewApiKeyAsync(string providedApiKey)
        {

        }
    }
}
