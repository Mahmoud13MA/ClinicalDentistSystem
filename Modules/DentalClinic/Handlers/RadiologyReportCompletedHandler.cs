using System.Net;
using System.Text.Json;
using ClinicalDentistSystem.Shared.Contracts.Diagnostics;
using ClinicalDentistSystem.Shared.Contracts.Lab;
using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Models;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.DentalClinic.Handlers;

public class RadiologyReportCompletedHandler(
    AppDbContext context,
    LocalQueueDbContext queueContext,
    ILogger<RadiologyReportCompletedHandler> logger) : INotificationHandler<RadiologyReportCompletedEvent>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async System.Threading.Tasks.Task Handle(RadiologyReportCompletedEvent notification, CancellationToken cancellationToken)
    {
        var metadata = ExtractMetadata(notification);
        if (metadata == null)
        {
            return;
        }

        try
        {
            var exists = await context.DiagnosticReportMetadata
                .AnyAsync(x => x.ReportId == metadata.ReportId, cancellationToken);
            if (exists)
            {
                return;
            }

            context.DiagnosticReportMetadata.Add(metadata);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsConnectivityError(ex))
        {
            await QueueFallbackAsync(notification, metadata, ex, cancellationToken);
        }
    }

    private static clinical.APIs.Modules.DentalClinic.Models.DiagnosticReportMetadata? ExtractMetadata(RadiologyReportCompletedEvent notification)
    {
        var report = notification.Report;
        if (report == null)
        {
            return null;
        }

        var reportDate = report.Effective is FhirDateTime effective
            ? effective.ToDateTimeOffset(TimeSpan.Zero).UtcDateTime
            : DateTime.UtcNow;
        var title = report.Code?.Text ?? report.Code?.Coding?.FirstOrDefault()?.Display ?? "Diagnostic Report";
        var status = report.Status?.ToString() ?? "unknown";

        return new clinical.APIs.Modules.DentalClinic.Models.DiagnosticReportMetadata
        {
            ReportId = report.Id ?? Guid.NewGuid().ToString("N"),
            ReportDate = reportDate,
            Title = title,
            Status = status
        };
    }

    private async System.Threading.Tasks.Task QueueFallbackAsync(
        RadiologyReportCompletedEvent notification,
        clinical.APIs.Modules.DentalClinic.Models.DiagnosticReportMetadata metadata,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            reportId = metadata.ReportId,
            reportDate = metadata.ReportDate,
            title = metadata.Title,
            status = metadata.Status
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

    private static bool IsConnectivityError(Exception ex)
    {
        var rootEx = ex.GetBaseException();

        return rootEx switch
        {
            SqlException sqlEx => sqlEx.Number is 53 or -2 or -1 or 20 or 64 or 233 or 4060,
            TimeoutException => true,
            DbUpdateException dbEx => IsDbUpdateConnectivityError(dbEx),
            _ => false
        };
    }

    private static bool IsDbUpdateConnectivityError(DbUpdateException dbEx)
    {
        var innerEx = dbEx.InnerException;

        if (innerEx is SqlException sqlEx)
        {
            return sqlEx.Number is 53 or -2 or -1 or 20 or 64 or 233 or 4060;
        }

        return innerEx is TimeoutException;
    }
}
