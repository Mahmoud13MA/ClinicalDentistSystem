using ClinicalDentistSystem.Shared.Contracts.Lab;
using clinical.APIs.Shared.Data;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.ProsthodonticLab.Handlers;

public class LabOrderCreatedHandler(AppDbContext context, ILogger<LabOrderCreatedHandler> logger) : INotificationHandler<LabOrderCreatedEvent>
{
    public async System.Threading.Tasks.Task Handle(LabOrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        if (!int.TryParse(notification.LabOrder.Id, out var orderId))
        {
            logger.LogWarning("Lab order created event missing valid order id.");
            return;
        }

        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.OrderID == orderId, cancellationToken);

        if (order != null)
        {
            return;
        }

        var patientId = ResolvePatientId(notification.LabOrder.Subject?.Reference);
        if (patientId <= 0)
        {
            logger.LogWarning("Lab order {OrderId} missing valid patient reference.", orderId);
            return;
        }

        var labTechnicianId = await ResolveLabTechnicianIdAsync(cancellationToken);
        if (labTechnicianId <= 0)
        {
            logger.LogWarning("Lab order {OrderId} cannot be queued without lab technician assignment.", orderId);
            return;
        }

        var newOrder = new clinical.APIs.Modules.DentalClinic.Models.Order
        {
            OrderID = orderId,
            PatientId = patientId,
            DentistId = 0,
            LabTechnicianID = labTechnicianId,
            OrderDate = DateTime.UtcNow,
            RequiredDate = DateTime.UtcNow,
            Priority = "Normal",
            ShippingMethod = "Standard",
            Status = notification.LabOrder.Status?.ToString() ?? "Pending",
            Notes = notification.LabOrder.Code is CodeableConcept concept && !string.IsNullOrWhiteSpace(concept.Text)
                ? concept.Text
                : "Lab Order"
        };

        context.Orders.Add(newOrder);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static int ResolvePatientId(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return 0;
        }

        var lastSegment = reference.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        return int.TryParse(lastSegment, out var patientId) ? patientId : 0;
    }

    private async Task<int> ResolveLabTechnicianIdAsync(CancellationToken cancellationToken)
    {
        return await context.LabTechnicians
            .Select(l => l.LabTechnicianID)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
