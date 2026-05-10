// DentalClinic/DTOs/DiagnosticReportRetryPayload.cs
namespace clinical.APIs.Modules.DentalClinic.DTOs;

public class DiagnosticReportRetryPayload
{
    public string ReportId { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}