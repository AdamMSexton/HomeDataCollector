namespace HomeDataCollector.Models
{
    public class ApiKeyIssueDto
    {
        public Guid SourceId { get; set; }
        public string ApiKey { get; set; } = string.Empty;
        public DateTimeOffset ApiKeyExpiration {  get; set; }
        public string CorrelationId { get; set; } = string.Empty;
    }
}
