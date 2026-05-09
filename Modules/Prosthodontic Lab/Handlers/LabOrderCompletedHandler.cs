using ClinicalDentistSystem.Shared.Contracts.Lab;
using clinical.APIs.Modules.ProsthodonticLab.Services;
using clinical.APIs.Shared.Data;
using ClinicalDentistSystem.Shared.Services;
using clinical.APIs.Shared.Utilities;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace clinical.APIs.Modules.ProsthodonticLab.Handlers;

public class LabOrderCompletedHandler(
    AppDbContext context,
    ILabFhirMappingService mappingService,
    IMediator mediator,
    IFhirValidationService validationService,
    ILogger<LabOrderCompletedHandler> logger)          // ← added
    : INotificationHandler<LabOrderCompletedEventTrigger>
{
    public async System.Threading.Tasks.Task Handle(LabOrderCompletedEventTrigger notification, CancellationToken cancellationToken)
    {
        var orderId = notification.OrderId;
        if (orderId <= 0)
        {
            logger.LogWarning("Lab order completed event fired with invalid OrderId={OrderId}.", orderId);
            return;
        }

        var prescription = await context.Prescriptions
            .FirstOrDefaultAsync(p => p.OrderID == orderId, cancellationToken);

        if (prescription == null)
        {
            logger.LogWarning("Lab order {OrderId} completed event fired but no prescription found.", orderId);
            return;
        }

        if (!string.Equals(prescription.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Lab order {OrderId} completed event fired but prescription status is '{Status}' — skipping.", orderId, prescription.Status);
            return;
        }

        var labTask = mappingService.MapPrescriptionToTask(prescription);
        labTask.Status = Hl7.Fhir.Model.Task.TaskStatus.Completed;

        var outcome = validationService.Validate(labTask);
        if (HasErrors(outcome))
        {
            // Validation failure is a data problem — retrying won't fix it
            // Log the field-level errors so the issue is visible
            var errors = string.Join(", ", outcome.Issue
                .Where(i => i.Severity is OperationOutcome.IssueSeverity.Error or OperationOutcome.IssueSeverity.Fatal)
                .Select(i => i.Diagnostics));

            logger.LogError("Lab order {OrderId} FHIR Task validation failed — event not published. Errors: {Errors}", orderId, errors);
            return;
        }

        logger.LogInformation("Publishing LabOrderCompletedEvent for order {OrderId}.", orderId);
        await mediator.Publish(new LabOrderCompletedEvent(labTask), cancellationToken);
    }

    private static bool HasErrors(OperationOutcome outcome)
        => outcome.Issue.Any(issue => issue.Severity is OperationOutcome.IssueSeverity.Error or OperationOutcome.IssueSeverity.Fatal);
}