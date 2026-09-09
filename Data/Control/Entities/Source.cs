namespace HealthDataCollector.Data.Control.Entities
{
    public class Source
    {
        public Guid SourceId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string DeviceType { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? LastSeenAt { get; set; }
        
        public ICollection<ApiKey> ApiKeys { get; set; } = [];
    }
}
