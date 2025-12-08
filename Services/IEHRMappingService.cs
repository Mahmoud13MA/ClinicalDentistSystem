using clinical.APIs.Models;
using clinical.APIs.DTOs;

namespace clinical.APIs.Services
{
    public interface IEHRMappingService
    {
        EHRResponse MapToResponse(EHR ehr);
        List<EHRResponse> MapToResponseList(List<EHR> ehrs);
    }
}
