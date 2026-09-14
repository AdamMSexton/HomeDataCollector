namespace HealthDataCollector.Models
{
    public class ProvisionResponseDto
    {
        public Guid SourceId { get; set; }
        public string ApiKey { get; set; } = string.Empty;
        public DateTimeOffset ApiKeyExpiration {  get; set; }
        public string CorrelationId { get; set; } = string.Empty;
    }
}
