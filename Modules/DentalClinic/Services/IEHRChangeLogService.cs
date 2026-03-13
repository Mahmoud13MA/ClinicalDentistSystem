using clinical.APIs.Modules.DentalClinic.Models;
using System.Security.Claims;

namespace clinical.APIs.Modules.DentalClinic.Services
{
    public interface IEHRChangeLogService
    {
        Task LogChangesAsync(EHR oldEhr, EHR newEhr, int doctorId, string doctorName, int appointmentId);
        Task LogCreationAsync(EHR ehr, int doctorId, string doctorName, int appointmentId);
    }
}
