using clinical.APIs.Models;
using clinical.APIs.DTOs;

namespace clinical.APIs.Services
{
    public interface IDoctorMappingService
    {
        DoctorResponse MapToResponse(Doctor doctor);
        List<DoctorResponse> MapToResponseList(List<Doctor> doctors);
    }
}
