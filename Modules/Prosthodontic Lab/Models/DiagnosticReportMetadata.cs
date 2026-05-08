using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.ProsthodonticLab.Models;

public class DiagnosticReportMetadata
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(64)]
    public string ReportId { get; set; } = string.Empty;

    [Required]
    public DateTime ReportDate { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;
}
