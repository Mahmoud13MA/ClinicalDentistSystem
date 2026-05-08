using System.Text.Json;
using ClinicalDentistSystem.Shared.Contracts.Lab;
using clinical.APIs.Modules.ProsthodonticLab.Models;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Models;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.DentalClinic.Handlers;

public class LabOrderCompletedHandler(
    AppDbContext context,
    LocalQueueDbContext queueContext,
    ILogger<LabOrderCompletedHandler> logger) : INotificationHandler<LabOrderCompletedEvent>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async System.Threading.Tasks.Task Handle(LabOrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        var metadata = ExtractMetadata(notification);
        if (metadata == null)
        {
            return;
        }

        try
        {
            var exists = await context.LabDiagnosticReportMetadata
                .AnyAsync(x => x.ReportId == metadata.ReportId, cancellationToken);
            if (exists)
            {
                return;
            }

            context.LabDiagnosticReportMetadata.Add(metadata);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsConnectivityError(ex))
        {
            await QueueFallbackAsync(metadata, ex, cancellationToken);
        }
    }

    private static DiagnosticReportMetadata? ExtractMetadata(LabOrderCompletedEvent notification)
    {
        var resource = notification.LabOrder;
        var status = resource switch
        {
            Hl7.Fhir.Model.Task task => task.Status?.ToString() ?? "unknown",
            Hl7.Fhir.Model.DeviceRequest request => request.Status?.ToString() ?? "unknown",
            _ => "unknown"
        };

        var title = resource switch
        {
            Hl7.Fhir.Model.Task task => task.Description ?? "Lab Task",
            Hl7.Fhir.Model.DeviceRequest request when request.Code is CodeableConcept concept && !string.IsNullOrWhiteSpace(concept.Text)
                => concept.Text,
            _ => "Lab Order"
        };

        var reportDate = DateTime.UtcNow;

        return new DiagnosticReportMetadata
        {
            ReportId = resource.Id ?? Guid.NewGuid().ToString("N"),
            ReportDate = reportDate,
            Title = title,
            Status = status
        };
    }

    private async System.Threading.Tasks.Task QueueFallbackAsync(
        DiagnosticReportMetadata metadata,
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
            Route = "/api/v1/dentalclinic/lab/metadata",
            Payload = payload,
            Status = PendingOperationStatus.Pending,
            LastError = exception.GetBaseException().Message,
            LastAttemptAt = DateTime.UtcNow
        };

        queueContext.PendingOperations.Add(pendingOp);
        await queueContext.SaveChangesAsync(cancellationToken);

        logger.LogWarning("Queued lab metadata fallback operation {OperationId} due to connectivity outage.", pendingOp.Id);
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
