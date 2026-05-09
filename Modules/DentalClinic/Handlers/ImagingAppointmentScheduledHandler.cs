using ClinicalDentistSystem.Shared.Contracts.Radiology;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace clinical.APIs.Modules.DentalClinic.Handlers;

public class ImagingAppointmentScheduledHandler(
    AppDbContext context,
    ILogger<ImagingAppointmentScheduledHandler> logger)
    : INotificationHandler<ImagingAppointmentScheduledEvent>
{
    public async System.Threading.Tasks.Task Handle(
        ImagingAppointmentScheduledEvent notification,
        CancellationToken cancellationToken)
    {
        var alreadyExists = await context.ImagingAppointmentMetadata
            .AnyAsync(x => x.ImagingId == notification.ImagingId, cancellationToken);

        if (alreadyExists)
        {
            logger.LogInformation("Imaging appointment metadata {ImagingId} already exists — skipping.", notification.ImagingId);
            return;
        }

        var metadata = new ImagingAppointmentMetadata
        {
            ImagingId = notification.ImagingId,
            PatientId = notification.PatientId,
            Modality = notification.Modality,
            ScheduledAt = notification.ScheduledAt,
            Status = "Scheduled"
        };

        try
        {
            context.ImagingAppointmentMetadata.Add(metadata);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Imaging appointment {ImagingId} scheduled for Patient={PatientId}, Modality={Modality}.",
                notification.ImagingId, notification.PatientId, notification.Modality);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save imaging appointment metadata for {ImagingId}.", notification.ImagingId);
        }
    }
}