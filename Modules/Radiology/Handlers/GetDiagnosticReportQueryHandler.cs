using ClinicalDentistSystem.Shared.Contracts.Diagnostics;
using ClinicalDentistSystem.Shared.Contracts.Radiology;
using Hl7.Fhir.Model;
using MediatR;

namespace clinical.APIs.Modules.Radiology.Handlers;

public class GetDiagnosticReportQueryHandler(IRadiologyModule radiologyModule)
    : IRequestHandler<GetDiagnosticReportQuery, DiagnosticReport?>
{
    public async Task<DiagnosticReport?> Handle(GetDiagnosticReportQuery request, CancellationToken cancellationToken)
    {
        var report = await radiologyModule.GetReportAsync(request.ReportId, cancellationToken);
        return report;
    }
}
