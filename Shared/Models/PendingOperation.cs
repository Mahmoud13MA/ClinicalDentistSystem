
using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Shared.Models
{
    public enum PendingOperationStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2
    }

    public class PendingOperation
    {
        [Key]
        public Guid Id { get; init; } = Guid.NewGuid();

        [MaxLength(10)]
        public required string HttpMethod { get; init; }

        [MaxLength(512)]
        public required string Route { get; init; }

        public required string Payload { get; init; }

        [MaxLength(128)]
        public string? IdempotencyKey { get; init; }

        public PendingOperationStatus Status { get; set; } = PendingOperationStatus.Pending;

        public int RetryCount { get; set; }

        [MaxLength(1000)]
        public string? LastError { get; set; }

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public DateTime? LastAttemptAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
