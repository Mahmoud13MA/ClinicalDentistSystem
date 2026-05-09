
using Hl7.Fhir.Model;
using MediatR;
namespace ClinicalDentistSystem.Shared.Contracts.Lab;

public record LabOrderCompletedEvent(Hl7.Fhir.Model.Task LabOrder) : INotification;