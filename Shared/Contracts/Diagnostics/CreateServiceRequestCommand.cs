using Hl7.Fhir.Model;
using MediatR;

namespace ClinicalDentistSystem.Shared.Contracts.Diagnostics;

// Command to request a service (e.g., ordering a dental X-Ray)
public record CreateServiceRequestCommand(ServiceRequest Request) : IRequest<ServiceRequest>;
