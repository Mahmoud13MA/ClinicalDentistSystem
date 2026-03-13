using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Models;

namespace clinical.APIs.Modules.DentalClinic.Services
{
    public interface IDoctorMappingService
    {
        DoctorResponse MapToResponse(Doctor doctor);
        List<DoctorResponse> MapToResponseList(List<Doctor> doctors);
    }
}
