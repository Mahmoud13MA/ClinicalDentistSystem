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
            return;

        try
        {
            var exists = await context.DiagnosticReportMetadata
                .AnyAsync(x => x.ReportId == metadata.ReportId, cancellationToken);
            if (exists)
                return;

            context.DiagnosticReportMetadata.Add(metadata);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ConnectivityErrorDetector.IsConnectivityError(ex)) // ← shared utility
        {
            await QueueFallbackAsync(notification, metadata, ex, cancellationToken);
        }
    }

    private static DiagnosticReportMetadata? ExtractMetadata(RadiologyReportCompletedEvent notification) // ← shared model
    {
        var report = notification.Report;
        if (report == null)
            return null;

        var reportDate = report.Effective is FhirDateTime effective
            ? effective.ToDateTimeOffset(TimeSpan.Zero).UtcDateTime
            : DateTime.UtcNow;

        var title = report.Code?.Text
            ?? report.Code?.Coding?.FirstOrDefault()?.Display
            ?? "Diagnostic Report";

        var status = report.Status?.ToString() ?? "unknown";

        return new DiagnosticReportMetadata // ← shared model
        {
            ReportId = report.Id ?? Guid.NewGuid().ToString("N"),
            ReportDate = reportDate,
            Title = title,
            Status = status
        };
    }

    private async System.Threading.Tasks.Task QueueFallbackAsync(
        RadiologyReportCompletedEvent notification,
        DiagnosticReportMetadata metadata, // ← shared model
        Exception exception,
        CancellationToken cancellationToken)
    {
        var reportJson = notification.Report == null
            ? null
            : fhirSerialization.SerializeToJson(notification.Report);

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
            Route = "/api/v1/dentalclinic/diagnosticreports/metadata",
            Payload = payload,
            Status = PendingOperationStatus.Pending,
            LastError = exception.GetBaseException().Message,
            LastAttemptAt = DateTime.UtcNow
        };

        queueContext.PendingOperations.Add(pendingOp);
        await queueContext.SaveChangesAsync(cancellationToken);

        logger.LogWarning("Queued diagnostic report metadata fallback operation {OperationId} due to connectivity outage.", pendingOp.Id);
    }
}