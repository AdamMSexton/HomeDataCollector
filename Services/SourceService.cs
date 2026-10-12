using HomeDataCollector.Data.Control;
using HomeDataCollector.Data.Control.Entities;
using HomeDataCollector.Enums;
using HomeDataCollector.Constants;
using HomeDataCollector.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace HomeDataCollector.Services
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

        private async Task<ServiceResult<bool>> CompleteApiRenewal(Guid sourceId,long apiKeyId)
        {
            // Get all keys belonging to sourceId
            var apiKeyList = await _db.ApiKeys.Where(x => x.SourceId == sourceId).ToListAsync();
            
            if (apiKeyList.Count == 0)
            {
                return ServiceResult<bool>.Fail(ServiceError.NotFound, ServiceMessages.SourceNotFound);
            }

            // Find the expected final active key
            var keyToActivate = apiKeyList.FirstOrDefault(x => x.Id == apiKeyId && x.Status == KeyStatus.ActiveRenewed);

            // Key not found
            if (keyToActivate is null)
            {
                return ServiceResult<bool>.Fail(ServiceError.ApiKeyNotFound,ServiceMessages.ApiKeyNotFound);
            }


            foreach (var apiKey in apiKeyList)
            {
                if ((apiKey.Status != KeyStatus.Revoked) && (apiKey.Status != KeyStatus.Expired))
                {
                    if ((apiKey.Status == KeyStatus.ActiveRenewed) && (apiKey.Id == keyToActivate.Id))
                    {
                        apiKey.Status = KeyStatus.Active;
                    }
                    else
                    {
                        apiKey.Status = KeyStatus.Revoked;
                    }
                }
            }

            await _db.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }

        private async Task<ServiceResult<ApiKey>> FindApiKey(byte[] providedApiKey)
        {
            // Check for null or incorrect byte length
            if ((providedApiKey.Length != 32) || (providedApiKey == null))
            {
                return ServiceResult<ApiKey>.Fail(ServiceError.Validation, ServiceMessages.ImproperFormat);
            }

            // Hash Api key bytes it search DB
            byte[] apiKeyHash = SHA256.HashData(providedApiKey);

            ApiKey? matchedApiKey = await _db.ApiKeys.FirstOrDefaultAsync(x => x.ApiKeyHash == apiKeyHash);

            if (matchedApiKey == null)
            {
                return ServiceResult<ApiKey>.Fail(ServiceError.ApiKeyNotFound, ServiceMessages.ApiKeyNotFound);
            }

            if ((matchedApiKey.Status == KeyStatus.Active) || (matchedApiKey.Status == KeyStatus.ActiveRenewed) || (matchedApiKey.Status == KeyStatus.RenewalOnly))
            {
                if (matchedApiKey.ExpiresAt <= DateTimeOffset.UtcNow)
                {
                    matchedApiKey.Status = KeyStatus.Expired;
                    await _db.SaveChangesAsync();
                }
            }

            return ServiceResult<ApiKey>.Ok(matchedApiKey);
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

            ApiKey newApiRecord = new()
            {
                SourceId = sourceId,
                ApiKeyHash = apiKeyHash,
                CreatedAt = now,
                ExpiresAt = now.AddDays(_ass.GetSetting<int>(SettingName.ApiKeyLifetimeDays)),
                RenewedAt = null,
                Status = HomeDataCollector.Enums.KeyStatus.Active
            };

            Source newSourceRecord = new()
            {
                SourceId = sourceId,
                Name = request.Name,
                DeviceType = request.DeviceType,
                Enabled = true,
                CreatedAt = now,
                LastSeenAt = now,
            };

            ApiKeyIssueDto provisioningResult = new()
            {
                SourceId = sourceId,
                ApiKey = apiKey,
                ApiKeyExpiration = newApiRecord.ExpiresAt,
                CorrelationId = correlationId
            };

            await using var transaction = await _db.Database.BeginTransactionAsync();

            newSourceRecord.ApiKeys.Add(newApiRecord);

            try
            {
                await _db.Sources.AddAsync(newSourceRecord);
                //_db.ApiKeys.Add(newApiRecord);

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
            byte[] apiKeyBytes;
            // Convert from Hex string to bytes
            try
            {
                apiKeyBytes = Convert.FromHexString(providedApiKey);
            }
            catch (FormatException)
            {
                return ServiceResult<Guid>.Fail(ServiceError.ApiKeyNotFound, ServiceMessages.ApiKeyNotFound);
            }

            // Get Api Record
            var findApiKeyResult = await FindApiKey(apiKeyBytes);
            if ((!findApiKeyResult.Success) || (findApiKeyResult.Data == null))
            {
                return ServiceResult<Guid>.Fail(findApiKeyResult.Error, findApiKeyResult.Message ?? "Unknown.");
            }

            ApiKey matchedApiKey = findApiKeyResult.Data;

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
                case KeyStatus.ActiveRenewed:
                    {
                        
                        var renewCompleteResult = await CompleteApiRenewal(matchedApiKey.SourceId, matchedApiKey.Id);
                        if (!renewCompleteResult.Success)
                        {
                            return ServiceResult<Guid>.Fail(renewCompleteResult.Error, renewCompleteResult.Message ?? "Unknown");
                        }
                        return ServiceResult<Guid>.Ok(matchedApiKey.SourceId);
                    }
                case KeyStatus.Active:
                    {
                        return ServiceResult<Guid>.Ok(matchedApiKey.SourceId);
                    }

            }

            // Only get there if an invalid api status was attached to the key in DB.
            return ServiceResult<Guid>.Fail(ServiceError.Internal, "Unknown Error. Status: '" + matchedApiKey.Status.ToString() + "' is unknown.");
        }
    
        public async Task<ServiceResult<ApiKeyIssueDto>> StartApiRenewalAsync(string correlationId, string providedApiKey)
        {
            byte[] apiKeyBytes;
            // Convert from Hex string to bytes
            try
            {
                apiKeyBytes = Convert.FromHexString(providedApiKey);
            }
            catch (FormatException)
            {
                return ServiceResult<ApiKeyIssueDto>.Fail(ServiceError.ApiKeyNotFound, ServiceMessages.ApiKeyNotFound);
            }

            // Get Api Record
            var findApiKeyResult = await FindApiKey(apiKeyBytes);
            if ((!findApiKeyResult.Success) || (findApiKeyResult.Data == null))
            {
                return ServiceResult<ApiKeyIssueDto>.Fail(findApiKeyResult.Error, findApiKeyResult.Message ?? "Unknown.");
            }

            var apiKeyToRenew = findApiKeyResult.Data;

            if (apiKeyToRenew.Status == KeyStatus.Expired)
            {
                return ServiceResult<ApiKeyIssueDto>.Fail(ServiceError.ApiKeyExpired, ServiceMessages.ApiKeyExpired);
            }

            if (apiKeyToRenew.Status == KeyStatus.Revoked)
            {
                return ServiceResult<ApiKeyIssueDto>.Fail(ServiceError.ApiKeyRevoked, ServiceMessages.ApiKeyRevoked);
            }

            

            DateTimeOffset renewalWindowOpen = apiKeyToRenew.ExpiresAt.AddDays(-1 * _ass.GetSetting<int>(SettingName.ApiKeyRenewalWindowDays));

            // Check to see if we are in renewal window.
            if (renewalWindowOpen >= DateTimeOffset.UtcNow)
            {
                return ServiceResult<ApiKeyIssueDto>.Fail(ServiceError.ApiKeyOutsideRenewalWin, ServiceMessages.ApiKeyTooEarlyRenew);
            }

            // Generate new API key
            byte[] newApiKeyBytes = RandomNumberGenerator.GetBytes(32);
            string newApiKeyHex = Convert.ToHexString(newApiKeyBytes);
            byte[] newApiKeyHash = SHA256.HashData(newApiKeyBytes);

            ApiKeyIssueDto newApiKeyReturnToDevice = new()
            {
                SourceId = apiKeyToRenew.SourceId,
                ApiKey = newApiKeyHex,
                ApiKeyExpiration = DateTimeOffset.UtcNow.AddDays(_ass.GetSetting<int>(SettingName.ApiKeyLifetimeDays)),
                CorrelationId = correlationId
            };

            ApiKey newApiKeyDbRecord = new()
            {
                SourceId = apiKeyToRenew.SourceId,
                ApiKeyHash = newApiKeyHash,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(_ass.GetSetting<int>(SettingName.ApiKeyLifetimeDays)),
                Status = KeyStatus.ActiveRenewed,
            };

            //Change old Key to Renewal only
            apiKeyToRenew.Status = KeyStatus.RenewalOnly;

            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                _db.ApiKeys.Add(newApiKeyDbRecord);
                //_db.ApiKeys.Update(apiKeyToRenew);

                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult<ApiKeyIssueDto>.Fail(Enums.ServiceError.Database, ex.Message);
            }

            return ServiceResult<ApiKeyIssueDto>.Ok(newApiKeyReturnToDevice);
        }
    }
}
