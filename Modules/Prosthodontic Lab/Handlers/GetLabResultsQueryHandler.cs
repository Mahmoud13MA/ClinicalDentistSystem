using clinical.APIs.Shared.Data;
using ClinicalDentistSystem.Shared.Contracts.Lab;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.ProsthodonticLab.Handlers;

public class GetLabResultsQueryHandler(AppDbContext context)
    : IRequestHandler<GetLabResultsQuery, Hl7.Fhir.Model.Task?>
{
    public async System.Threading.Tasks.Task<Hl7.Fhir.Model.Task?> Handle(GetLabResultsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.OrderId))
            return null;

        var metadata = await context.LabDiagnosticReportMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.OrderId == request.OrderId, cancellationToken);

        if (metadata == null)
            return null;

        return new Hl7.Fhir.Model.Task
        {
            Id = metadata.OrderId,
            Status = Hl7.Fhir.Model.Task.TaskStatus.Completed,
            Description = metadata.ProstheticType,
            LastModified = metadata.CompletedDate.ToString("o")
        };
    }
}
