using Hl7.Fhir.Model;

namespace ClinicalDentistSystem.Shared.Services;

public interface IFhirValidationService
{
    OperationOutcome Validate(Resource resource);
}
