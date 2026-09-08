namespace HealthDataCollector.Models
{
    public class MeasurementAcceptedDto
    {
        public ulong MeasurementId { get; set; }

        public DateTimeOffset ReceivedAt { get; set; }

        public string CorrelationId { get; set; } = string.Empty;
    }
}
