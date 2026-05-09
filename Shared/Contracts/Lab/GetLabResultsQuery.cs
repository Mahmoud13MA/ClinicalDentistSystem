using Hl7.Fhir.Model;
using MediatR;

namespace ClinicalDentistSystem.Shared.Contracts.Lab;

public record GetLabResultsQuery(string OrderId) : IRequest<Hl7.Fhir.Model.Task?>;
