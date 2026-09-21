using HealthDataCollector.Data.Health;
using HealthDataCollector.Data.Health.Entities;
using HealthDataCollector.Models;

namespace HealthDataCollector.Services
{
    public class WeightService : IWeightService
    {
        private readonly HealthDbContext _db;

        public WeightService(HealthDbContext db)
        {
            _db = db;
        }

        public async Task<ServiceResult<MeasurementAcceptedDto>> IntakeNewWeightMeasurementAsync(string correlationId, DateTimeOffset receivedAt, Guid sourceId, WeightMeasurementDto receivedWeight)
        {
            MeasurementAcceptedDto returnPacket = new();

            returnPacket.CorrelationId = correlationId;
            returnPacket.ReceivedAt = receivedAt;

            // Filter out erroneous/trash readings
            if ((receivedWeight.WeightKg < 0) || (receivedWeight.WeightKg >= 700))
            {
                return ServiceResult<MeasurementAcceptedDto>.Fail(Enums.ServiceError.Validation, "Invalid Weight.");
            }

            // Filter out erroneous/trash readings
            if ((receivedWeight.ImpedanceOhm < 1) || (receivedWeight.ImpedanceOhm > 65534))
            {
                return ServiceResult<MeasurementAcceptedDto>.Fail(Enums.ServiceError.Validation, "Invalid Impedence.");
            }

            // Use time transmitted from sender to when received here to determine if the clock on distant end is off signifigantly.
            // If the difference is more than a minute then use the delta to offset teh measurement time
            DateTimeOffset? adjustedMeasurementTime = null;
            TimeSpan transmissionDeltaT = receivedAt - receivedWeight.TransmittedAt;
            if (Math.Abs(transmissionDeltaT.TotalSeconds) >= 10)
            {
                adjustedMeasurementTime = receivedWeight.MeasuredAt.Add(transmissionDeltaT);
                adjustedMeasurementTime = adjustedMeasurementTime.Value.ToUniversalTime();
            }


            // Prepare new weight record
            DbWeightRecord newWeightRecord = new()
            {
                SourceId = sourceId,
                MeasurementId = receivedWeight.MeasurementId,
                UserId = receivedWeight.UserId,
                WeightKg = receivedWeight.WeightKg,
                ImpedanceOhm = receivedWeight.ImpedanceOhm,
                ReceivedAt = receivedAt,
                MeasuredAt = adjustedMeasurementTime ?? receivedWeight.MeasuredAt.ToUniversalTime(),
            };

            try
            {
                await _db.AddAsync(newWeightRecord);

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return ServiceResult<MeasurementAcceptedDto>.Fail(Enums.ServiceError.Database, ex.Message);
            }

            // Determine if we believe there is more than a 10 second clock difference, if so calculate and add to return packet.
            int? deltaTSeconds = null;
            if (Math.Abs(transmissionDeltaT.TotalSeconds) > 10)
            {
                deltaTSeconds = (int)transmissionDeltaT.TotalSeconds;
            }

            // Prepare return packet
            MeasurementAcceptedDto weightAccepted = new()
            {
                MeasurementId = newWeightRecord.MeasurementId,
                ReceivedAt = receivedAt,
                CorrelationId = correlationId,
                PreceivedClockOffsetSeconds = deltaTSeconds
            };


            return ServiceResult<MeasurementAcceptedDto>.Ok(weightAccepted);
        }
    }
}
