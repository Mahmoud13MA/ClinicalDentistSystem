using Hl7.Fhir.Model;
using MediatR;

namespace ClinicalDentistSystem.Shared.Contracts.Lab;

// Query used to get the final generated report from the Lab module
public record GetLabResultsQuery(string DiagnosticReportId) : IRequest<DiagnosticReport>;
