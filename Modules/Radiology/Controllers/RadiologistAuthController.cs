using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
using clinical.APIs.Shared.Services;
using clinical.APIs.Modules.Radiology.DTOs;
using Radiology.Models;
using AutoMapper;


namespace clinical.APIs.Modules.Radiology.Controllers
{
    [ApiController]
    [Route("api/v1/radiology/[controller]")]
    public class RadiologistAuthController(AppDbContext context, IJwtService jwtService, IPasswordHashService passwordHashService, IConfiguration configuration, IEmailValidationService emailValidationService, IMapper mapper) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RadiologistRegisterRequest request)
        {
            var validRegistrationKey = configuration["RegistrationSettings:RadiologistRegistrationKey"];

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
          

            var radiologist = mapper.Map<Radiologist>(request);
            radiologist.PasswordHash = passwordHashService.HashPassword(request.Password);
            radiologist.Email = request.Email.Trim().ToLowerInvariant();

            context.Radiologists.Add(radiologist);
            await context.SaveChangesAsync();

            var token = jwtService.GenerateToken(radiologist.RadiologistID, radiologist.Email, radiologist.Name, "Radiologist");

            var response = mapper.Map<RadiologistLoginResponse>(radiologist);
            response.Token = token;
            return Ok(response);
           
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] RadiologistLoginRequest request)
        {
            

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var radiologist = await context.Radiologists.FirstOrDefaultAsync(d => d.Email == normalizedEmail);
            if (radiologist == null || !passwordHashService.VerifyPassword(request.Password, radiologist.PasswordHash))
            {
                return Unauthorized(new { error = "Invalid email or password." });
            }

            var token = jwtService.GenerateToken(radiologist.RadiologistID, radiologist.Email, radiologist.Name, "Radiologist");

            var response = mapper.Map<RadiologistLoginResponse>(radiologist);
            response.Token = token;
            return Ok(response);
        }
    }
}