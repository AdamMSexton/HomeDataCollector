using HomeDataCollector.Enums;

namespace HomeDataCollector.Data.Control.Entities
{
    public class ApiKey
    {
        public long Id {  get; set; }
        public Guid SourceId { get; set; }
        public Source Source { get; set; } = null!;
        public byte[] ApiKeyHash { get; set; } = new byte[32];
        
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }

        public DateTimeOffset? RenewedAt { get; set; }

        public KeyStatus Status { get; set; }


    }
}
