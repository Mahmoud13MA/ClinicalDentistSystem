
using clinical.APIs.Modules.DentalClinic.DTOs;

namespace clinical.APIs.Modules.DentalClinic.Services
{
    public interface IProfileManagementService
    {

        Task<(bool IsSuccess, string ErrorMessage)> UpdateDoctorInfoAsync(int id, UpdateStaffInfoRequest request);

        Task<(bool IsSuccess, string ErrorMessage)> UpdateNurseInfoAsync(int id, UpdateStaffInfoRequest request);

        Task<(bool IsSuccess, string ErrorMessage)> UpdatePatientInfoAsync(int id, UpdatePatientInfoRequest request);

        Task<(bool IsSuccess, string ErrorMessage)> DeletePatientAsync(int id);
    }

}

 