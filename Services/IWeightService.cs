using HealthDataCollector.Models;

namespace HealthDataCollector.Services
{
    public interface IWeightService
    {
        Task<ServiceResult<MeasurementAcceptedDto>> IntakeNewWeightMeasurementAsync(string correlationId, DateTimeOffset receivedAt, Guid sourceId, WeightMeasurementDto receivedWeight);
    }
}
