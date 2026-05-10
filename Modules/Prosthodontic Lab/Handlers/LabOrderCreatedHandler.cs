using System.Text.Json;
using ClinicalDentistSystem.Shared.Contracts.Lab;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Models;
using clinical.APIs.Shared.Utilities;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace clinical.APIs.Modules.ProsthodonticLab.Handlers;

public class LabOrderCreatedHandler(
    AppDbContext context,
    LocalQueueDbContext queueContext,
    ILogger<LabOrderCreatedHandler> logger)
    : INotificationHandler<LabOrderCreatedEvent>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async System.Threading.Tasks.Task Handle(LabOrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.LabOrder == null)
        {
            logger.LogWarning("Lab order created event missing DeviceRequest payload.");
            return;
        }

        // FHIR ID is a GUID string — used only for logging and fallback queue key
        var fhirId = notification.LabOrder.Id;
        if (string.IsNullOrWhiteSpace(fhirId))
        {
            logger.LogWarning("Lab order created event missing FHIR id.");
            return;
        }

        var patientId = ResolvePatientId(notification.LabOrder.Subject?.Reference, out var referenceError);
        if (patientId <= 0)
        {
            logger.LogWarning("Lab order {FhirId} missing valid patient reference: {Reason}", fhirId, referenceError);
            await EnqueueFallbackAsync(notification.LabOrder, fhirId, cancellationToken);
            return;
        }

        var labTechnicianId = await ResolveLabTechnicianIdAsync(cancellationToken);
        if (labTechnicianId <= 0)
        {
            logger.LogWarning("Lab order {FhirId} cannot be assigned — no lab technicians available.", fhirId);
            await EnqueueFallbackAsync(notification.LabOrder, fhirId, cancellationToken);
            return;
        }

        var dentistId = ResolveDentistId(notification.LabOrder.Requester?.Reference);
        if (dentistId <= 0)
            logger.LogWarning("Lab order {FhirId} has missing or unresolvable Requester — DentistId will be 0.", fhirId);

        var newOrder = new clinical.APIs.Modules.DentalClinic.Models.Order
        {
            // OrderID not set — EF auto-generates the int PK
            PatientId = patientId,
            DentistId = dentistId,
            LabTechnicianID = labTechnicianId,
            OrderDate = DateTime.UtcNow,
            RequiredDate = DateTime.UtcNow,
            Priority = "Normal",
            ShippingMethod = "Standard",
            Status = notification.LabOrder.Status?.ToString() ?? "Pending",
            Notes = notification.LabOrder.Code is CodeableConcept concept
                    && !string.IsNullOrWhiteSpace(concept.Text)
                ? concept.Text
                : "Lab Order"
        };

        try
        {
            context.Orders.Add(newOrder);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Lab order {FhirId} created for Patient={PatientId}, Dentist={DentistId}, LabTechnician={LabTechnicianId}",
                fhirId, patientId, dentistId, labTechnicianId);
        }
        catch (Exception ex) when (ConnectivityErrorDetector.IsConnectivityError(ex))
        {
            logger.LogWarning("DB connectivity error saving lab order {FhirId} — queuing for retry.", fhirId);
            await EnqueueFallbackAsync(notification.LabOrder, fhirId, cancellationToken);
        }
    }

    private async System.Threading.Tasks.Task EnqueueFallbackAsync(
        DeviceRequest deviceRequest,
        string fhirId,
        CancellationToken cancellationToken)
    {
        try
        {
            var idempotencyKey = $"lab-order-created-{fhirId}";

            var alreadyQueued = await queueContext.PendingOperations
                .AnyAsync(p => p.IdempotencyKey == idempotencyKey
                            && p.Status == PendingOperationStatus.Pending, cancellationToken);

            if (alreadyQueued)
            {
                logger.LogInformation("Lab order {FhirId} already queued, skipping duplicate.", fhirId);
                return;
            }

            var serializer = new FhirJsonSerializer();
            var fhirJson = serializer.SerializeToString(deviceRequest);

            var operation = new PendingOperation
            {
                HttpMethod = "POST",
                Route = "api/v1/prosthodonticlab/order/retry",  // ← updated route
                Payload = JsonSerializer.Serialize(new
                {
                    fhirId,
                    fhirResource = fhirJson
                }, SerializerOptions),
                IdempotencyKey = idempotencyKey
            };

            queueContext.PendingOperations.Add(operation);
            await queueContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Lab order {FhirId} queued for retry.", fhirId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue lab order {FhirId} for retry.", fhirId);
        }
    }

    private static int ResolvePatientId(string? reference, out string error)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            error = "Subject reference is empty.";
            return 0;
        }

        if (!reference.StartsWith("Patient/", StringComparison.OrdinalIgnoreCase))
        {
            error = "Subject reference is not a Patient reference.";
            return 0;
        }

        var lastSegment = reference.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        if (!int.TryParse(lastSegment, out var patientId))
        {
            error = "Subject reference patient id is not numeric.";
            return 0;
        }

        error = string.Empty;
        return patientId;
    }

    private static int ResolveDentistId(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return 0;

        if (!reference.StartsWith("Practitioner/", StringComparison.OrdinalIgnoreCase))
            return 0;

        var lastSegment = reference.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        return int.TryParse(lastSegment, out var dentistId) ? dentistId : 0;
    }

    // Least busy technician — fewest active (non-completed) orders
    private async Task<int> ResolveLabTechnicianIdAsync(CancellationToken cancellationToken)
    {
        return await context.LabTechnicians
            .OrderBy(lt => context.Orders
                .Count(o => o.LabTechnicianID == lt.LabTechnicianID
                         && o.Status != "Completed"))
            .Select(lt => lt.LabTechnicianID)
            .FirstOrDefaultAsync(cancellationToken);
    }
}