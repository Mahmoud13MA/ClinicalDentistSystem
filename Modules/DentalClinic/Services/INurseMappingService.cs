using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Models;

namespace clinical.APIs.Modules.DentalClinic.Services
{
    public interface INurseMappingService
    {
        NurseResponse MapToResponse(Nurse nurse);
        List<NurseResponse> MapToResponseList(List<Nurse> nurses);
    }
}
