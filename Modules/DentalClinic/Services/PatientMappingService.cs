using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Models;

namespace clinical.APIs.Modules.DentalClinic.Services
{
    public class PatientMappingService : IPatientMappingService
    {
        public PatientResponse MapToResponse(Patient patient)
        {
            if (patient == null)
                return null;

            return new PatientResponse
            {
                Patient_ID = patient.Patient_ID,
                First = patient.First,
                Middle = patient.Middle,
                Last = patient.Last,
                Gender = patient.Gender,
                DOB = patient.DOB,
                Phone = patient.Phone
            };
        }

        public List<PatientResponse> MapToResponseList(List<Patient> patients)
        {
            if (patients == null)
                return new List<PatientResponse>();

            return patients.Select(p => MapToResponse(p)).ToList();
        }
    }
}
