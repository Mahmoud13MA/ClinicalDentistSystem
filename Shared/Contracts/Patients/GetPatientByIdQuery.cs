using Hl7.Fhir.Model;
using MediatR;
namespace ClinicalDentistSystem.Shared.Contracts.Patients;
// Query to request a standard FHIR Patient resource
public record GetPatientByIdQuery(string PatientId) : IRequest<Patient?>;