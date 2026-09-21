using HealthDataCollector.Models;

namespace HealthDataCollector.Services
{
    public class WeightService : IWeightService
    {
        public async Task<ServiceResult<MeasurementAcceptedDto>> IntakeNewWeightMeasurementAsync(string correlationId, DateTimeOffset receivedAt, Guid Source, WeightMeasurementDto receivedWeight)
        {
            return ServiceResult<MeasurementAcceptedDto>.Ok(null);
        }
    }
}
