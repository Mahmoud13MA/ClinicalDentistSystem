using Hl7.Fhir.Model;
using MediatR;

namespace ClinicalDentistSystem.Shared.Contracts.Lab;

// Command to register a collected specimen linked to a lab order
public record SubmitTargetSpecimenCommand(Specimen CollectedSpecimen) : IRequest<Specimen>;
