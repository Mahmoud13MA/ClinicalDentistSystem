using ClinicalDentistSystem.Shared.Contracts.Diagnostics;
using clinical.APIs.Shared.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Radiology.Models;

namespace clinical.APIs.Modules.Radiology.Handlers;

public class RadiologyRequestCreatedHandler(AppDbContext context) : INotificationHandler<RadiologyRequestCreatedEvent>
{
    public async System.Threading.Tasks.Task Handle(RadiologyRequestCreatedEvent notification, CancellationToken cancellationToken)
    {
        var patientId = ResolvePatientId(notification.Request);
        if (patientId == null)
        {
            return;
        }

        var patientExists = await context.RadiologyPatients
            .AnyAsync(p => p.PatientID == patientId.Value, cancellationToken);

        if (!patientExists)
        {
            return;
        }

        var radiologistId = await context.Radiologists
            .Select(r => (int?)r.RadiologistID)
            .FirstOrDefaultAsync(cancellationToken);

        var equipmentId = await context.Equipment
            .Select(e => (int?)e.EquipmentID)
            .FirstOrDefaultAsync(cancellationToken);

        if (radiologistId == null || equipmentId == null)
        {
            return;
        }

        var appointment = new ImagingAppointment
        {
            PatientID = patientId.Value,
            RadiologistID = radiologistId.Value,
            EquipmentID = equipmentId.Value,
            Datetime = DateTime.UtcNow,
            Type = notification.Request.Code?.Text ?? "X-Ray"
        };

        context.ImagingAppointments.Add(appointment);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static int? ResolvePatientId(Hl7.Fhir.Model.ServiceRequest request)
    {
        if (request.Subject?.Reference == null)
        {
            return null;
        }

        var reference = request.Subject.Reference;
        var lastSegment = reference.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        return int.TryParse(lastSegment, out var patientId) ? patientId : null;
    }
}
