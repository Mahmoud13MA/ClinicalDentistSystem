using Hl7.Fhir.Model;
using MediatR;

namespace ClinicalDentistSystem.Shared.Contracts.Lab;

// Command sent by a clinical module to order a lab test
// Returns the created ServiceRequest with its assigned ID and status
public record CreateLabOrderCommand(ServiceRequest LabOrderRequest) : IRequest<ServiceRequest>;
