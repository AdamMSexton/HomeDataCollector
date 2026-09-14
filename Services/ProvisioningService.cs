using HealthDataCollector.Data.Control;
using HealthDataCollector.Data.Control.Entities;
using HealthDataCollector.Enums;
using HealthDataCollector.Models;
using System.Security.Cryptography;

namespace HealthDataCollector.Services
{
    public class ProvisioningService : IProvisioningService
    {
        
        private readonly ControlDbContext _db;
        private readonly IAppStateService _ass;

        public ProvisioningService(ControlDbContext db, IAppStateService ass) 
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

        public async Task<ServiceResult<ProvisionResponseDto>> ProvisionNewClient(string correlationId, ProvisionRequestDto request)
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

            var provisioningResult = new ProvisionResponseDto
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
                return ServiceResult<ProvisionResponseDto>.Fail(Enums.ServiceError.Database, ex.Message);
            }

            return ServiceResult<ProvisionResponseDto>.Ok(provisioningResult);
        }
    }
}
