namespace clinical.APIs.Shared.Services
{
    public interface IEmailValidationService
    {
        Task<bool> IsEmailUsedAsync(string email, int? doctorId = null, int? nurseId = null, int? adminId = null);
    }
}
