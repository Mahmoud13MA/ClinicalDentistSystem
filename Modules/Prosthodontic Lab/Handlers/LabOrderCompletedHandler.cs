using ClinicalDentistSystem.Shared.Contracts.Lab;
using clinical.APIs.Modules.ProsthodonticLab.Services;
using clinical.APIs.Shared.Data;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.ProsthodonticLab.Handlers;

public class LabOrderCompletedHandler(AppDbContext context, ILabFhirMappingService mappingService, IMediator mediator)
    : INotificationHandler<LabOrderCompletedEventTrigger>
{
    public async System.Threading.Tasks.Task Handle(LabOrderCompletedEventTrigger notification, CancellationToken cancellationToken)
    {
        var orderId = notification.OrderId;
        if (orderId <= 0)
        {
            return;
        }

        var prescription = await context.Prescriptions
            .FirstOrDefaultAsync(p => p.OrderID == orderId, cancellationToken);

        if (prescription == null)
        {
            return;
        }

        if (!string.Equals(prescription.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var labTask = mappingService.MapPrescriptionToTask(prescription);
        labTask.Status = Hl7.Fhir.Model.Task.TaskStatus.Completed;

        await mediator.Publish(new LabOrderCompletedEvent(labTask), cancellationToken);
    }
}
