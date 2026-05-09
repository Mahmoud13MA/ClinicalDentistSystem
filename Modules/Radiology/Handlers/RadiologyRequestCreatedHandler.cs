using ClinicalDentistSystem.Shared.Contracts.Diagnostics;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Radiology.Models;
using System.Text.Json;
using SystemTask = System.Threading.Tasks.Task; // ← alias fixes ambiguity

namespace clinical.APIs.Modules.Radiology.Handlers;

public class RadiologyRequestCreatedHandler(
    AppDbContext context,
    LocalQueueDbContext queueContext,
    ILogger<RadiologyRequestCreatedHandler> logger)
    : INotificationHandler<RadiologyRequestCreatedEvent>
{
    public async SystemTask Handle(RadiologyRequestCreatedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Request == null)
        {
            logger.LogWarning("Radiology request event missing ServiceRequest payload.");
            return;
        }

        var patientId = ResolvePatientId(notification.Request, out var referenceError);
        if (patientId == null)
        {
            logger.LogWarning("Radiology request missing valid patient reference: {Reason}", referenceError);
            return;
        }

        var modality = notification.Request.Code?.Text ?? "X-Ray";

        var patientExists = await context.RadiologyPatients
            .AnyAsync(p => p.PatientID == patientId.Value, cancellationToken);

        if (!patientExists)
        {
            logger.LogWarning("Patient {PatientId} not found — queuing for retry.", patientId);
            await EnqueueFallbackAsync(notification.Request, modality, patientId.Value, cancellationToken);
            return;
        }

        var radiologistId = await ResolveRadiologistIdAsync(modality, cancellationToken);
        var equipmentId = await ResolveEquipmentIdAsync(modality, cancellationToken);

        if (radiologistId == null || equipmentId == null)
        {
            logger.LogWarning(
                "No available radiologist or equipment for modality {Modality} — queuing for retry.",
                modality);
            await EnqueueFallbackAsync(notification.Request, modality, patientId.Value, cancellationToken);
            return;
        }

        var appointment = new ImagingAppointment
        {
            PatientID = patientId.Value,
            RadiologistID = radiologistId.Value,
            EquipmentID = equipmentId.Value,
            Datetime = DateTime.UtcNow,
            Type = modality
        };

        context.ImagingAppointments.Add(appointment);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Scheduled imaging appointment for Patient={PatientId}, Radiologist={RadiologistId}, Equipment={EquipmentId}, Modality={Modality}",
            patientId, radiologistId, equipmentId, modality);
    }

    private async SystemTask EnqueueFallbackAsync(  // ← SystemTask fixes return type error
        ServiceRequest request,
        string modality,
        int patientId,
        CancellationToken cancellationToken)
    {
        try
        {
            var idempotencyKey = $"radiology-request-{request.Id}";

            var alreadyQueued = await queueContext.PendingOperations
                .AnyAsync(p => p.IdempotencyKey == idempotencyKey
                            && p.Status == PendingOperationStatus.Pending, cancellationToken);

            if (alreadyQueued)
            {
                logger.LogInformation("Radiology request {Id} already queued, skipping duplicate.", request.Id);
                return;
            }

            var operation = new PendingOperation
            {
                HttpMethod = "POST",
                Route = "api/v1/radiology/imagingappointment",
                Payload = JsonSerializer.Serialize(new
                {
                    PatientID = patientId,
                    Type = modality,
                    Datetime = DateTime.UtcNow
                }),
                IdempotencyKey = idempotencyKey
            };

            queueContext.PendingOperations.Add(operation);
            await queueContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Queued radiology request {Id} for retry.", request.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue radiology request {Id} for retry.", request.Id);
        }
    }

    private static int? ResolvePatientId(ServiceRequest request, out string error)
    {
        var reference = request.Subject?.Reference;
        if (string.IsNullOrWhiteSpace(reference))
        {
            error = "Subject reference is empty.";
            return null;
        }

        if (!reference.StartsWith("Patient/", StringComparison.OrdinalIgnoreCase))
        {
            error = "Subject reference is not a Patient reference.";
            return null;
        }

        var lastSegment = reference.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        if (!int.TryParse(lastSegment, out var patientId))
        {
            error = "Subject reference patient id is not numeric.";
            return null;
        }

        error = string.Empty;
        return patientId;
    }

    private async System.Threading.Tasks.Task<int?> ResolveRadiologistIdAsync(string modality, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        return await context.Radiologists
            .OrderBy(r => context.ImagingAppointments
                .Count(a => a.RadiologistID == r.RadiologistID
                         && a.Datetime >= today
                         && a.Type == modality))
            .Select(r => (int?)r.RadiologistID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async System.Threading.Tasks.Task<int?> ResolveEquipmentIdAsync(string modality, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        return await context.Equipment
            .OrderBy(e => context.ImagingAppointments
                .Count(a => a.EquipmentID == e.EquipmentID
                         && a.Datetime >= today
                         && a.Type == modality))
            .Select(e => (int?)e.EquipmentID)
            .FirstOrDefaultAsync(cancellationToken);
    }
}