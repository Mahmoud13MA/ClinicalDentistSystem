using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Models;

namespace clinical.APIs.Modules.DentalClinic.Services
{
    public interface IAppointmentMappingService
    {
        AppointmentResponse MapToResponse(Appointment appointment);
        List<AppointmentResponse> MapToResponseList(List<Appointment> appointments);
    }
}
