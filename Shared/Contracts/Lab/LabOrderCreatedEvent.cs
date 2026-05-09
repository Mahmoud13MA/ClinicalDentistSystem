using Hl7.Fhir.Model;
using MediatR;

namespace ClinicalDentistSystem.Shared.Contracts.Lab;

public record LabOrderCreatedEvent(DeviceRequest LabOrder) : INotification;
