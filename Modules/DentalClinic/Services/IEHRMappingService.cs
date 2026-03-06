using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Models;

namespace clinical.APIs.Modules.DentalClinic.Services
{
    public interface IEHRMappingService
    {
        EHRResponse MapToResponse(EHR ehr);
        List<EHRResponse> MapToResponseList(List<EHR> ehrs);
    }
}
