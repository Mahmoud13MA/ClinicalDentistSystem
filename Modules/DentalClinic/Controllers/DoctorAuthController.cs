using Microsoft.AspNetCore.Mvc;
using clinical.APIs.Modules.DentalClinic.Models;
using Microsoft.EntityFrameworkCore;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
using clinical.APIs.Shared.Services;
using clinical.APIs.Modules.DentalClinic.DTOs;

namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [ApiController]
    [Route("api/v1/clinic/[controller]")]
    public class DoctorAuthController(AppDbContext context, IJwtService jwtService , IPasswordHashService passwordHashService , IConfiguration configuration , IEmailValidationService emailValidationService) : ControllerBase
    {
      

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] DoctorRegisterRequest request)
        {
           

            var validRegistrationKey = configuration["RegistrationSettings:DoctorRegistrationKey"];

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

            var doctor = new Doctor
            {
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHashService.HashPassword(request.Password)
            };

            context.Doctors.Add(doctor);
            await context.SaveChangesAsync();

            var token = jwtService.GenerateToken(doctor.ID, doctor.Email, doctor.Name, "Doctor");

            return Ok(new DoctorLoginResponse
            {
                Token = token,
                DoctorId = doctor.ID,
                Name = doctor.Name,
                Email = doctor.Email,
                Phone = doctor.Phone
            });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] DoctorLoginRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Login data is required." });
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.Email == normalizedEmail);
            if (doctor == null || !passwordHashService.VerifyPassword(request.Password, doctor.PasswordHash))
            {
                return Unauthorized(new { error = "Invalid email or password." });
            }

            var token = jwtService.GenerateToken(doctor.ID, doctor.Email, doctor.Name, "Doctor");

            return Ok(new DoctorLoginResponse
            {
                Token = token,
                DoctorId = doctor.ID,
                Name = doctor.Name,
                Email = doctor.Email,
                Phone = doctor.Phone
            });
        }
    }
}
