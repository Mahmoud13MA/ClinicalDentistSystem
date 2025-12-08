using clinical.APIs.Models;
using clinical.APIs.DTOs;

namespace clinical.APIs.Services
{
    public interface INurseMappingService
    {
        NurseResponse MapToResponse(Nurse nurse);
        List<NurseResponse> MapToResponseList(List<Nurse> nurses);
    }
}
