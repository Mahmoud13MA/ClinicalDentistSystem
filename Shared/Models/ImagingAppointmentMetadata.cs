using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Shared.Models;

public class ImagingAppointmentMetadata
{
    [Key]
    public int Id { get; set; }

    public int ImagingId { get; set; }
    public int PatientId { get; set; }

    [MaxLength(100)]
    public string Modality { get; set; } = string.Empty;

    public DateTime ScheduledAt { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Scheduled";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}