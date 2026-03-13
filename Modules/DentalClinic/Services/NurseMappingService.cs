using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Models;

namespace clinical.APIs.Modules.DentalClinic.Services
{
    public class NurseMappingService : INurseMappingService
    {
        public NurseResponse MapToResponse(Nurse nurse)
        {
            if (nurse == null)
                return null;

            return new NurseResponse
            {
                NURSE_ID = nurse.NURSE_ID,
                Name = nurse.Name,
                Phone = nurse.Phone,
                Email = nurse.Email
            };
        }

        public List<NurseResponse> MapToResponseList(List<Nurse> nurses)
        {
            if (nurses == null)
                return new List<NurseResponse>();

            return nurses.Select(n => MapToResponse(n)).ToList();
        }
    }
}
