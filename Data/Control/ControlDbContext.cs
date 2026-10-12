using HomeDataCollector.Data.Control.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeDataCollector.Data.Control
{
    public class ControlDbContext : DbContext
    {
        public ControlDbContext(DbContextOptions<ControlDbContext> options)
            : base(options)
        {
        }

        public DbSet<Source> Sources => Set<Source>();
        public DbSet<Setting> Settings => Set<Setting>();
        public DbSet<ApiKey> ApiKeys => Set<ApiKey>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Source>(entity =>
            {
                entity.HasKey(x => x.SourceId);

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.DeviceType)
                    .HasMaxLength(50)
                    .IsRequired();
                
                entity.Property(x => x.CreatedAt)
                    .IsRequired();
            });

            modelBuilder.Entity<ApiKey>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.SourceId)
                    .IsRequired();

                entity.HasOne(x => x.Source)
                .WithMany(x => x.ApiKeys)
                .HasForeignKey(x => x.SourceId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.ApiKeyHash)
                    .IsUnique();

                entity.Property(x => x.Status)
                    .IsRequired();
            });

            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Key)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(x => x.Key)
                    .IsUnique();
            });
        }
    }
}
