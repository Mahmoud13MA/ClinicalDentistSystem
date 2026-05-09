using Hl7.Fhir.Model;
using MediatR;

namespace ClinicalDentistSystem.Shared.Contracts.Diagnostics;

// Query to retrieve test/radiology results
public record GetDiagnosticReportQuery(string ReportId) : IRequest<DiagnosticReport?>;
