namespace ClinicalDentistSystem.Shared.Contracts.Diagnostics;

public record DiagnosticReportMetadataContract(
    string ReportId,
    DateTime ReportDate,
    string Title,
    string Status);
