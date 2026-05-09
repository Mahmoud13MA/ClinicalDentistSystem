using ClinicalDentistSystem.Shared.Contracts.Patients;
using Hl7.Fhir.Model;

namespace clinical.APIs.Modules.DentalClinic.Services;

public class DentalClinicPatientFhirMapper : IDentalClinicPatientFhirMapper
{
    public Patient MapToFhirPatient(clinical.APIs.Modules.DentalClinic.Models.Patient patient)
    {
        ArgumentNullException.ThrowIfNull(patient);

        var name = new HumanName
        {
            Family = patient.Last,
            Given = new[] { patient.First, patient.Middle }.Where(value => !string.IsNullOrWhiteSpace(value)).ToList()
        };

        return new Patient
        {
            Id = patient.Patient_ID.ToString(),
            Name = new List<HumanName> { name },
            Gender = MapGender(patient.Gender),
            BirthDate = patient.DOB.ToString("yyyy-MM-dd"),
            Telecom = string.IsNullOrWhiteSpace(patient.Phone)
                ? new List<ContactPoint>()
                : new List<ContactPoint> { new() { System = ContactPoint.ContactPointSystem.Phone, Value = patient.Phone } }
        };
    }

    private static AdministrativeGender? MapGender(string? gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
        {
            return null;
        }

        return gender.Trim().ToLowerInvariant() switch
        {
            "male" => AdministrativeGender.Male,
            "female" => AdministrativeGender.Female,
            "other" => AdministrativeGender.Other,
            "unknown" => AdministrativeGender.Unknown,
            _ => null
        };
    }
}
