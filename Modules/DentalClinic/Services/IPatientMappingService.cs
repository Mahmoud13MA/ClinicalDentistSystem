using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Models;

namespace clinical.APIs.Modules.DentalClinic.Services
{
    public interface IPatientMappingService
    {
        PatientResponse MapToResponse(Patient patient);
        List<PatientResponse> MapToResponseList(List<Patient> patients);
    }
}
