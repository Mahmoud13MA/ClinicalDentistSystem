using Hl7.Fhir.Model;
using MediatR;

namespace ClinicalDentistSystem.Shared.Contracts.Lab;

public record LabOrderCompletedEvent(Resource LabOrder) : INotification;
