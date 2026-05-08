namespace ClinicalDentistSystem.Shared.Contracts.Diagnostics;

public record DiagnosticReportMetadata(
    string ReportId,
    DateTime ReportDate,
    string Title,
    string Status);
