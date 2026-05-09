using ClinicalDentistSystem.Shared.Contracts.Diagnostics;
using ClinicalDentistSystem.Shared.Services;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.Extensions.Logging;

namespace clinical.APIs.Modules.DentalClinic.Handlers;
public class CreateServiceRequestHandler(IMediator mediator, ILogger<CreateServiceRequestHandler> logger, IFhirValidationService validationService)
    : IRequestHandler<CreateServiceRequestCommand, ServiceRequest>
{
    public async Task<ServiceRequest> Handle(CreateServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = request.Request;

        if (serviceRequest is null)
            throw new ArgumentException("ServiceRequest payload was null.");

        serviceRequest.Id ??= Guid.NewGuid().ToString("N");

        var outcome = validationService.Validate(serviceRequest);
        if (HasErrors(outcome))
        {
            var errors = string.Join(", ", outcome.Issue
                .Where(i => i.Severity is OperationOutcome.IssueSeverity.Error or OperationOutcome.IssueSeverity.Fatal)
                .Select(i => i.Diagnostics));

            logger.LogWarning("FHIR validation failed for ServiceRequest {Id}: {Errors}", serviceRequest.Id, errors);
            throw new ArgumentException(errors); 
        }

        logger.LogInformation("Publishing RadiologyRequestCreatedEvent for ServiceRequest {Id}", serviceRequest.Id);
        await mediator.Publish(new RadiologyRequestCreatedEvent(serviceRequest), cancellationToken);
        return serviceRequest;
    }

    private static bool HasErrors(OperationOutcome outcome)
        => outcome.Issue.Any(i => i.Severity is OperationOutcome.IssueSeverity.Error or OperationOutcome.IssueSeverity.Fatal);
}