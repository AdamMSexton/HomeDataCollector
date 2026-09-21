using HealthDataCollector.Data.Health.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthDataCollector.Data.Health
{
    public class HealthDbContext : DbContext
    {
        public HealthDbContext (DbContextOptions<HealthDbContext> options)
            : base(options)
        {
        }

        public DbSet<DbWeightRecord> WeightMeasurements => Set<DbWeightRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbWeightRecord>(entity =>
            {

                // Composite primary key also provides our idempotency guarantee.
                entity.HasKey(x => new
                {
                    x.SourceId,
                    x.MeasurementId
                });

                entity.Property(x => x.SourceId)
                    .IsRequired();

                entity.Property(x => x.MeasurementId)
                    .IsRequired();

                entity.Property(x => x.UserId)
                    .IsRequired();

                entity.Property(x => x.WeightKg)
                    .IsRequired();

                entity.Property(x => x.ImpedanceOhm)
                    .IsRequired(false);

                entity.Property(x => x.MeasuredAt)
                    .IsRequired();

                entity.Property(x => x.ReceivedAt)
                    .IsRequired();

                // Basic sanity constraints.
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_WeightMeasurements_WeightKg",
                        "\"WeightKg\" > 0");

                    t.HasCheckConstraint(
                        "CK_WeightMeasurements_ImpedanceOhm",
                        "\"ImpedanceOhm\" IS NULL OR \"ImpedanceOhm\" > 0");
                });
            });
        }
    }
}
