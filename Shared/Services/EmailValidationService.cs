using clinical.APIs.Shared.Data;
using Microsoft.EntityFrameworkCore;
namespace clinical.APIs.Shared.Services
{
    public class EmailValidationService(AppDbContext context) : IEmailValidationService
    {
      

        public async Task<bool> IsEmailUsedAsync(string email, int? doctorId = null, int? nurseId = null, int? adminId = null , int? radiologistId = null)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            var emailUsed = await context.Doctors.Where(d => d.Email == normalizedEmail && doctorId != d.ID).Select(d => d.Email).Concat(
                context.Nurses.Where(n => n.Email == normalizedEmail && n.NURSE_ID != nurseId).Select(n => n.Email)).Concat(
                context.Admins.Where(a=>a.Email==normalizedEmail&& a.Admin_ID!=adminId).Select(a=> a.Email)).Concat(context.Radiologists.Where(r => r.Email == normalizedEmail && r.RadiologistID!= radiologistId).Select(r => r.Email)).AnyAsync();
            return emailUsed;
        }
    }
}
