using HomeDataCollector.Models;

namespace HomeDataCollector.Services
{
    public interface IWeightService
    {
        Task<ServiceResult<MeasurementAcceptedDto>> IntakeNewWeightMeasurementAsync(string correlationId, DateTimeOffset receivedAt, Guid sourceId, WeightMeasurementDto receivedWeight);
    }
}
