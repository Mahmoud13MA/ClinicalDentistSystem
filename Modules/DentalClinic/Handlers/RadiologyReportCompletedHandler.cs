using System.Text.Json;
using ClinicalDentistSystem.Shared.Contracts.Diagnostics;
using ClinicalDentistSystem.Shared.Serialization;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Models;
using clinical.APIs.Shared.Utilities;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.DentalClinic.Handlers;

public class RadiologyReportCompletedHandler(
    AppDbContext context,
    LocalQueueDbContext queueContext,
    FhirSerializationUtility fhirSerialization,
    ILogger<RadiologyReportCompletedHandler> logger) : INotificationHandler<RadiologyReportCompletedEvent>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async System.Threading.Tasks.Task Handle(RadiologyReportCompletedEvent notification, CancellationToken cancellationToken)
    {
        var metadata = ExtractMetadata(notification);
        if (metadata == null)
        {
            logger.LogWarning("RadiologyReportCompletedHandler: ExtractMetadata returned null — report may be missing or have a null ID. Event dropped.");
            return;
        }

        try
        {
            var exists = await context.DiagnosticReportMetadata
                .AnyAsync(x => x.ReportId == metadata.ReportId, cancellationToken);

            if (exists)
            {
                logger.LogWarning("RadiologyReportCompletedHandler: DiagnosticReportMetadata for ReportId {ReportId} already exists. Skipping.", metadata.ReportId);
                return;
            }

            context.DiagnosticReportMetadata.Add(metadata);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("RadiologyReportCompletedHandler: Saved DiagnosticReportMetadata for ReportId {ReportId}.", metadata.ReportId);
        }
        catch (Exception ex) when (ConnectivityErrorDetector.IsConnectivityError(ex))
        {
            await QueueFallbackAsync(notification, metadata, ex, cancellationToken);
        }
    }

    private DiagnosticReportMetadata? ExtractMetadata(RadiologyReportCompletedEvent notification)
    {
        var report = notification.Report;

        if (report == null)
        {
            logger.LogWarning("RadiologyReportCompletedHandler: Received event with null Report.");
            return null;
        }

        // 🔴 Fix: null report.Id would generate a new GUID every time,
        // breaking the idempotency check on every retry.
        if (string.IsNullOrWhiteSpace(report.Id))
        {
            logger.LogWarning("RadiologyReportCompletedHandler: Report.Id is null or empty — cannot guarantee idempotency. Event dropped.");
            return null;
        }

        var reportDate = report.Effective is FhirDateTime effective
            ? effective.ToDateTimeOffset(TimeSpan.Zero).UtcDateTime
            : DateTime.UtcNow;

        var title = report.Code?.Text
            ?? report.Code?.Coding?.FirstOrDefault()?.Display
            ?? "Diagnostic Report";

        var status = report.Status?.ToString() ?? "unknown";

        return new DiagnosticReportMetadata
        {
            ReportId = report.Id,
            ReportDate = reportDate,
            Title = title,
            Status = status
        };
    }

    private async System.Threading.Tasks.Task QueueFallbackAsync(
        RadiologyReportCompletedEvent notification,
        DiagnosticReportMetadata metadata,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var reportJson = notification.Report == null
            ? null
            : fhirSerialization.SerializeToJson(notification.Report);

        // 🟡 Fix: idempotency key so BackgroundSyncService retries
        // don't insert duplicate rows via the controller.
        var idempotencyKey = $"diagnostic-report-metadata-{metadata.ReportId}";

        var payload = JsonSerializer.Serialize(new
        {
            reportId = metadata.ReportId,
            reportDate = metadata.ReportDate,
            title = metadata.Title,
            status = metadata.Status,
            fhirReport = reportJson
        }, SerializerOptions);

        var pendingOp = new PendingOperation
        {
            HttpMethod = HttpMethods.Post,
            Route = "/api/v1/dentalclinic/diagnosticreports/metadata/retry",
            Payload = payload,
            IdempotencyKey = idempotencyKey,
            Status = PendingOperationStatus.Pending,
            LastError = exception.GetBaseException().Message,
            LastAttemptAt = DateTime.UtcNow
        };

       
        try
        {
            queueContext.PendingOperations.Add(pendingOp);
            await queueContext.SaveChangesAsync(cancellationToken);

            logger.LogWarning(
                "RadiologyReportCompletedHandler: Queued fallback operation {OperationId} for ReportId {ReportId} due to connectivity outage.",
                pendingOp.Id,
                metadata.ReportId);
        }
        catch (Exception queueEx)
        {
            logger.LogError(
                queueEx,
                "RadiologyReportCompletedHandler: Failed to queue fallback for ReportId {ReportId}. Original error: {OriginalError}",
                metadata.ReportId,
                exception.GetBaseException().Message);
        }
    }
}