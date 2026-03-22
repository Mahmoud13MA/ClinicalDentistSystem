using clinical.APIs.Shared.Data;
using Microsoft.EntityFrameworkCore;
namespace clinical.APIs.Shared.Services
{
    public class EmailValidationService(AppDbContext context) : IEmailValidationService
    {
      

        public async Task<bool> IsEmailUsedAsync(string email, int? doctorId = null, int? nurseId = null, int? adminId = null)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            var emailUsed = await context.Doctors.Where(d => d.Email == normalizedEmail && doctorId != d.ID).Select(d => d.Email).Concat(
                context.Nurses.Where(n => n.Email == normalizedEmail && n.NURSE_ID != nurseId).Select(n => n.Email)).Concat(
                context.Admins.Where(a=>a.Email==normalizedEmail&& a.Admin_ID!=adminId).Select(a=> a.Email)).AnyAsync();
            return emailUsed;
        }
    }
}
