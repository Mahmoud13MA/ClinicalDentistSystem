using Microsoft.EntityFrameworkCore;
using clinical.APIs.Shared.Models;

namespace clinical.APIs.Shared.Data
{
    public class LocalQueueDbContext(DbContextOptions<LocalQueueDbContext> options) : DbContext(options)
    {
        public DbSet<PendingOperation> PendingOperations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PendingOperation>(entity =>
            {
                entity.Property(x => x.HttpMethod).HasMaxLength(10).IsRequired();
                entity.Property(x => x.Route).HasMaxLength(512).IsRequired();
                entity.Property(x => x.IdempotencyKey).HasMaxLength(128);
                entity.Property(x => x.LastError).HasMaxLength(1000);

                entity.HasIndex(x => x.Status);
                entity.HasIndex(x => x.CreatedAt);
                entity.HasIndex(x => x.IdempotencyKey);
            });
        }
    }
}