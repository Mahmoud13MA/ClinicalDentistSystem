using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Shared.Models
{
    public class ProcessedRequest
    {
        [Key]
        [MaxLength(128)]
        public required string IdempotencyKey { get; init; }

        [MaxLength(10)]
        public required string HttpMethod { get; init; }

        [MaxLength(512)]
        public required string Route { get; init; }

        public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
    }
}