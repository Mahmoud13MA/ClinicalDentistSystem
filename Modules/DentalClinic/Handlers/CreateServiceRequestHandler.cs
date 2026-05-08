using ClinicalDentistSystem.Shared.Contracts.Diagnostics;
using MediatR;

namespace clinical.APIs.Modules.DentalClinic.Handlers;

public class CreateServiceRequestHandler(IMediator mediator) : IRequestHandler<CreateServiceRequestCommand, Hl7.Fhir.Model.ServiceRequest>
{
    public async Task<Hl7.Fhir.Model.ServiceRequest> Handle(CreateServiceRequestCommand request, CancellationToken cancellationToken)
    {
        await mediator.Publish(new RadiologyRequestCreatedEvent(request.Request), cancellationToken);
        return request.Request;
    }
}
