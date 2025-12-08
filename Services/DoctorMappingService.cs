using clinical.APIs.Models;
using clinical.APIs.DTOs;

namespace clinical.APIs.Services
{
    public class DoctorMappingService : IDoctorMappingService
    {
        public DoctorResponse MapToResponse(Doctor doctor)
        {
            if (doctor == null)
                return null;

            return new DoctorResponse
            {
                ID = doctor.ID,
                Name = doctor.Name,
                Phone = doctor.Phone,
                Email = doctor.Email
            };
        }

        public List<DoctorResponse> MapToResponseList(List<Doctor> doctors)
        {
            if (doctors == null)
                return new List<DoctorResponse>();

            return doctors.Select(d => MapToResponse(d)).ToList();
        }
    }
}
