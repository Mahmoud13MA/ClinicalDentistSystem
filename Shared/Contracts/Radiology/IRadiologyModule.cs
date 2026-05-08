using Hl7.Fhir.Model;

namespace ClinicalDentistSystem.Shared.Contracts.Radiology;

public interface IRadiologyModule
{
    Task<DiagnosticReport?> GetReportAsync(string reportId, CancellationToken cancellationToken = default);
}
