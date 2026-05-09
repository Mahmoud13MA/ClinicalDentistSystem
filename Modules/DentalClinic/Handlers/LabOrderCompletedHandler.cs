using System.Text.Json;
using ClinicalDentistSystem.Shared.Contracts.Lab;
using ClinicalDentistSystem.Shared.Serialization;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Models;
using clinical.APIs.Shared.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace clinical.APIs.Modules.DentalClinic.Handlers;

public class LabOrderCompletedHandler(
    AppDbContext context,
    LocalQueueDbContext queueContext,
    FhirSerializationUtility fhirSerialization,
    ILogger<LabOrderCompletedHandler> logger) : INotificationHandler<LabOrderCompletedEvent>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async System.Threading.Tasks.Task Handle(LabOrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        var metadata = ExtractMetadata(notification);
        if (metadata == null)
        {
            logger.LogWarning("LabOrderCompletedEvent received with null Task payload — skipping.");
            return;
        }

        try
        {
            var exists = await context.LabDiagnosticReportMetadata
                .AnyAsync(x => x.OrderId == metadata.OrderId, cancellationToken);

            if (exists)
            {
                logger.LogInformation("Lab metadata for order {OrderId} already exists — skipping duplicate.", metadata.OrderId);
                return;
            }

            context.LabDiagnosticReportMetadata.Add(metadata);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Lab metadata saved for order {OrderId}.", metadata.OrderId);
        }
        catch (Exception ex) when (ConnectivityErrorDetector.IsConnectivityError(ex))
        {
            logger.LogWarning("DB connectivity error saving lab metadata for order {OrderId} — queuing for retry.", metadata.OrderId);
            await QueueFallbackAsync(notification.LabOrder, metadata, ex, cancellationToken);
        }
    }

    private static LabDiagnosticReportMetadata? ExtractMetadata(LabOrderCompletedEvent notification)
    {
        var task = notification.LabOrder;

        if (task is null)
            return null;

        return new LabDiagnosticReportMetadata
        {
            OrderId = task.Id ?? Guid.NewGuid().ToString("N"),
            CompletedDate = DateTime.UtcNow,
            ProstheticType = task.Description ?? "Lab Order",
            Status = task.Status?.ToString() ?? "unknown"
        };
    }

    private async System.Threading.Tasks.Task QueueFallbackAsync(
        Hl7.Fhir.Model.Task? labOrder,
        LabDiagnosticReportMetadata metadata,
        Exception exception,
        CancellationToken cancellationToken)
    {
        try
        {
            var idempotencyKey = $"lab-metadata-completed-{metadata.OrderId}";  // ← added

            var alreadyQueued = await queueContext.PendingOperations               // ← added duplicate check
                .AnyAsync(p => p.IdempotencyKey == idempotencyKey
                            && p.Status == PendingOperationStatus.Pending, cancellationToken);

            if (alreadyQueued)
            {
                logger.LogInformation("Lab metadata for order {OrderId} already queued — skipping duplicate.", metadata.OrderId);
                return;
            }

            var resourceJson = labOrder == null ? null : fhirSerialization.SerializeToJson(labOrder);
            var payload = JsonSerializer.Serialize(new
            {
                orderId = metadata.OrderId,
                completedDate = metadata.CompletedDate,
                prostheticType = metadata.ProstheticType,
                status = metadata.Status,
                fhirResource = resourceJson
            }, SerializerOptions);

            var pendingOp = new PendingOperation
            {
                HttpMethod = HttpMethods.Post,
                Route = "/api/v1/dentalclinic/lab/metadata",
                Payload = payload,
                IdempotencyKey = idempotencyKey,                                   
                Status = PendingOperationStatus.Pending,
                LastError = exception.GetBaseException().Message,
                LastAttemptAt = DateTime.UtcNow
            };

            queueContext.PendingOperations.Add(pendingOp);
            await queueContext.SaveChangesAsync(cancellationToken);

            logger.LogWarning("Queued lab metadata fallback operation {OperationId} for order {OrderId}.", pendingOp.Id, metadata.OrderId);
        }
        catch (Exception queueEx)                                                  
        {
            logger.LogError(queueEx, "Failed to enqueue lab metadata fallback for order {OrderId}.", metadata.OrderId);
        }
    }
}