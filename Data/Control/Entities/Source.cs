namespace HealthDataCollector.Data.Control.Entities
{
    public class Source
    {
        public Guid SourceId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string DeviceType { get; set; } = string.Empty;

        public byte[] ApiKeyHash { get; set; }

        public DateTimeOffset? ApiExpiration { get; set; }

        public DateTimeOffset? ApiKeyRotatedAt { get; set; }

        public bool Enabled { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? LastSeenAt { get; set; }
    }
}
