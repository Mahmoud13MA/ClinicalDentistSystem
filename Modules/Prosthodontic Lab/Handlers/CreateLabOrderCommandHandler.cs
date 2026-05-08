using ClinicalDentistSystem.Shared.Contracts.Lab;
using clinical.APIs.Modules.ProsthodonticLab.Services;
using Hl7.Fhir.Model;
using MediatR;

namespace clinical.APIs.Modules.ProsthodonticLab.Handlers;

public class CreateLabOrderCommandHandler(ILabFhirMappingService mappingService, IMediator mediator)
    : IRequestHandler<CreateLabOrderCommand, ServiceRequest>
{
    public async Task<ServiceRequest> Handle(CreateLabOrderCommand request, CancellationToken cancellationToken)
    {
        var deviceRequest = mappingService.MapServiceRequestToDeviceRequest(request.LabOrderRequest);
        await mediator.Publish(new LabOrderCreatedEvent(deviceRequest), cancellationToken);

        return request.LabOrderRequest;
    }
}
