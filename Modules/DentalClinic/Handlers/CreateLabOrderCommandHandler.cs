using ClinicalDentistSystem.Shared.Contracts.Lab;
using ClinicalDentistSystem.Shared.Services;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.Extensions.Logging;

namespace clinical.APIs.Modules.DentalClinic.Handlers;

public class CreateLabOrderCommandHandler(
    IMediator mediator,                    // ← added back
    IFhirValidationService validationService,
    ILogger<CreateLabOrderCommandHandler> logger)
    : IRequestHandler<CreateLabOrderCommand, ServiceRequest>
{
    public async Task<ServiceRequest> Handle(CreateLabOrderCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = request.LabOrderRequest;

        if (serviceRequest is null)
            throw new ArgumentException("Lab order ServiceRequest payload was null.");

        serviceRequest.Id ??= Guid.NewGuid().ToString("N");
        serviceRequest.Requester = new ResourceReference($"Practitioner/{request.RequestingDoctorId}");

        var serviceRequestOutcome = validationService.Validate(serviceRequest);
        if (HasErrors(serviceRequestOutcome))
        {
            var errors = GetErrors(serviceRequestOutcome);
            logger.LogWarning("FHIR validation failed for lab ServiceRequest {Id}: {Errors}", serviceRequest.Id, errors);
            throw new ArgumentException(errors);
        }

        var deviceRequest = MapToDeviceRequest(serviceRequest);

        var deviceRequestOutcome = validationService.Validate(deviceRequest);
        if (HasErrors(deviceRequestOutcome))
        {
            var errors = GetErrors(deviceRequestOutcome);
            logger.LogWarning("FHIR validation failed for lab DeviceRequest {Id}: {Errors}", deviceRequest.Id, errors);
            throw new ArgumentException(errors);
        }

        logger.LogInformation("Publishing LabOrderCreatedEvent for DeviceRequest {Id}", deviceRequest.Id);
        await mediator.Publish(new LabOrderCreatedEvent(deviceRequest), cancellationToken); // ← publish

        return serviceRequest;
    }

    private static DeviceRequest MapToDeviceRequest(ServiceRequest serviceRequest)
    {
        return new DeviceRequest
        {
            Id = serviceRequest.Id,
            Status = serviceRequest.Status ?? RequestStatus.Active,
            Intent = RequestIntent.Order,
            Subject = serviceRequest.Subject,
            Code = serviceRequest.Code,
            Requester = serviceRequest.Requester
        };
    }

    private static bool HasErrors(OperationOutcome outcome)
        => outcome.Issue.Any(i => i.Severity is OperationOutcome.IssueSeverity.Error or OperationOutcome.IssueSeverity.Fatal);

    private static string GetErrors(OperationOutcome outcome)
        => string.Join(", ", outcome.Issue
            .Where(i => i.Severity is OperationOutcome.IssueSeverity.Error or OperationOutcome.IssueSeverity.Fatal)
            .Select(i => i.Diagnostics));
}