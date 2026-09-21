namespace HealthDataCollector.Models
{
    public class WeightMeasurementDto
    {
        public ulong MeasurementId { get; set; }
        public DateTimeOffset TransmittedAt { get; set; }
        public DateTimeOffset MeasuredAt { get; set; }
        public double WeightKg { get; set; }
        public int? ImpedanceOhm { get; set; }
        public byte UserId { get; set; }
    }
}