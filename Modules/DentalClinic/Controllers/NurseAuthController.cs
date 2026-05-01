using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
using clinical.APIs.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [ApiController]
    [Route("api/v1/clinic/[controller]")]
    public class NurseAuthController(AppDbContext context, IJwtService jwtService, IPasswordHashService passwordHashService, IConfiguration configuration , IEmailValidationService emailValidationService)  : ControllerBase
    {
       
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] NurseRegisterRequest request)
        {
           
            var validRegistrationKey = configuration["RegistrationSettings:NurseRegistrationKey"];

            if (string.IsNullOrEmpty(validRegistrationKey))
            {
                return StatusCode(500, new { error = "Server configuration error. Contact system administrator." });
            }

            if (request.RegistrationKey != validRegistrationKey)
            {
                return Unauthorized(new { error = "Invalid registration key. Contact your clinic administrator for the correct key." });
            }

            var isEmailUsed = await emailValidationService.IsEmailUsedAsync(request.Email);
            if (isEmailUsed)
            {
                return BadRequest(new { error = "Email already registered." });
            }

            var nurse = new Nurse
            {
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHashService.HashPassword(request.Password)
            };

            context.Nurses.Add(nurse);
            await context.SaveChangesAsync();

            var token = jwtService.GenerateToken(nurse.NURSE_ID, nurse.Email, nurse.Name, "Nurse");

            return Ok(new NurseLoginResponse
            {
                Token = token,
                NurseId = nurse.NURSE_ID,
                Name = nurse.Name,
                Email = nurse.Email,
                Phone = nurse.Phone
            });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] NurseLoginRequest request)
        {
           

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var nurse = await context.Nurses.FirstOrDefaultAsync(n => n.Email == normalizedEmail);
            if (nurse == null || !passwordHashService.VerifyPassword(request.Password, nurse.PasswordHash))
            {
                return Unauthorized(new { error = "Invalid email or password." });
            }

            var token = jwtService.GenerateToken(nurse.NURSE_ID, nurse.Email, nurse.Name, "Nurse");

            return Ok(new NurseLoginResponse
            {
                Token = token,
                NurseId = nurse.NURSE_ID,
                Name = nurse.Name,
                Email = nurse.Email,
                Phone = nurse.Phone
            });
        }
    }
}
