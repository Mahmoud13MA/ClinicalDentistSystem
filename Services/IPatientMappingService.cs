using clinical.APIs.Models;
using clinical.APIs.DTOs;

namespace clinical.APIs.Services
{
    public interface IPatientMappingService
    {
        PatientResponse MapToResponse(Patient patient);
        List<PatientResponse> MapToResponseList(List<Patient> patients);
    }
}
