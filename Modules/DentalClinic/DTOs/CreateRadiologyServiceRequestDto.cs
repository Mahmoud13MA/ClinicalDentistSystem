using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs;

public class CreateRadiologyServiceRequestDto
{
    [Required]
    public int PatientId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ImagingType { get; set; } = string.Empty; // e.g. "X-Ray", "CT", "MRI"
}