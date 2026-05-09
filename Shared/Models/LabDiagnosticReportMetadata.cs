// Shared/Models/LabDiagnosticReportMetadata.cs

using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Shared.Models;

public class LabDiagnosticReportMetadata
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(64)]
    public string OrderId { get; set; } = string.Empty;  

    [Required]
    public DateTime CompletedDate { get; set; }  
    [Required]
    [StringLength(200)]
    public string ProstheticType { get; set; } = string.Empty;  

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;
}