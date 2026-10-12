namespace HomeDataCollector.Data.Health.Entities
{
    public class DbWeightRecord
    {
        public Guid SourceId { get; set; }
        public ulong MeasurementId { get; set; }
        public byte UserId {  get; set; }
        public double WeightKg { get; set; }
        public int? ImpedanceOhm { get; set; }
        public DateTimeOffset MeasuredAt { get; set; }
        public DateTimeOffset ReceivedAt { get; set; }

    }
}
