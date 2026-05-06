using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
using clinical.APIs.Shared.Services;
using clinical.APIs.Modules.ProsthodonticLab.DTOs;
using clinical.APIs.Modules.DentalClinic.Models;
using AutoMapper;

namespace clinical.APIs.Modules.ProsthodonticLab.Controllers
{
    [ApiController]
    [Route("api/v1/prosthodonticlab/[controller]")]
    public class LabTechnicianAuthController(AppDbContext context, IJwtService jwtService, IPasswordHashService passwordHashService, IConfiguration configuration, IEmailValidationService emailValidationService, IMapper mapper) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] LabTechnicianRegisterRequest request)
        {
            var validRegistrationKey = configuration["RegistrationSettings:LabTechnicianRegistrationKey"];

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

            var technician = mapper.Map<LabTechnician>(request);
            technician.PasswordHash = passwordHashService.HashPassword(request.Password);
            technician.Email = request.Email.Trim().ToLowerInvariant();

            context.LabTechnicians.Add(technician);
            await context.SaveChangesAsync();

            var token = jwtService.GenerateToken(technician.LabTechnicianID, technician.Email, technician.Name, "LabTechnician");

            var response = mapper.Map<LabTechnicianLoginResponse>(technician);
            response.Token = token;
            return Ok(response);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LabTechnicianLoginRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var technician = await context.LabTechnicians.FirstOrDefaultAsync(t => t.Email == normalizedEmail);
            if (technician == null || !passwordHashService.VerifyPassword(request.Password, technician.PasswordHash))
            {
                return Unauthorized(new { error = "Invalid email or password." });
            }

            var token = jwtService.GenerateToken(technician.LabTechnicianID, technician.Email, technician.Name, "LabTechnician");

            var response = mapper.Map<LabTechnicianLoginResponse>(technician);
            response.Token = token;
            return Ok(response);
        }
    }
}
