using Hl7.Fhir.Model;

namespace ClinicalDentistSystem.Shared.Contracts.Patients;

public interface IDentalClinicPatientFhirMapper
{
    Patient MapToFhirPatient(clinical.APIs.Modules.DentalClinic.Models.Patient patient);
}
