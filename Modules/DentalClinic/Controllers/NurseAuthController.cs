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
    [Route("api/[controller]")]
    public class NurseAuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IConfiguration _configuration;
        private readonly IEmailValidationService _emailValidationService;

        public NurseAuthController(
            AppDbContext context,
            IJwtService jwtService,
            IPasswordHashService passwordHashService,
            IConfiguration configuration,
            IEmailValidationService emailValidationService)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHashService = passwordHashService;
            _configuration = configuration;
            _emailValidationService = emailValidationService;
        }

        // POST: api/NurseAuth/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] NurseRegisterRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Registration data is required." });
            }

            var validRegistrationKey = _configuration["RegistrationSettings:NurseRegistrationKey"];

            if (string.IsNullOrEmpty(validRegistrationKey))
            {
                return StatusCode(500, new { error = "Server configuration error. Contact system administrator." });
            }

            if (request.RegistrationKey != validRegistrationKey)
            {
                return Unauthorized(new { error = "Invalid registration key. Contact your clinic administrator for the correct key." });
            }

            var isEmailUsed = await _emailValidationService.IsEmailUsedAsync(request.Email);
            if (isEmailUsed)
            {
                return BadRequest(new { error = "Email already registered." });
            }

            var nurse = new Nurse
            {
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email.Trim().ToLowerInvariant(),
                PasswordHash = _passwordHashService.HashPassword(request.Password)
            };

            _context.Nurses.Add(nurse);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(nurse.NURSE_ID, nurse.Email, nurse.Name, "Nurse");

            return Ok(new NurseLoginResponse
            {
                Token = token,
                NurseId = nurse.NURSE_ID,
                Name = nurse.Name,
                Email = nurse.Email,
                Phone = nurse.Phone
            });
        }

        // POST: api/NurseAuth/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] NurseLoginRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Login data is required." });
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var nurse = await _context.Nurses.FirstOrDefaultAsync(n => n.Email == normalizedEmail);
            if (nurse == null || !_passwordHashService.VerifyPassword(request.Password, nurse.PasswordHash))
            {
                return Unauthorized(new { error = "Invalid email or password." });
            }

            var token = _jwtService.GenerateToken(nurse.NURSE_ID, nurse.Email, nurse.Name, "Nurse");

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
