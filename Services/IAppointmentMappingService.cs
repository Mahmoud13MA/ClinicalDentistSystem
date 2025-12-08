using clinical.APIs.Models;
using clinical.APIs.DTOs;

namespace clinical.APIs.Services
{
    public interface IAppointmentMappingService
    {
        AppointmentResponse MapToResponse(Appointment appointment);
        List<AppointmentResponse> MapToResponseList(List<Appointment> appointments);
    }
}
