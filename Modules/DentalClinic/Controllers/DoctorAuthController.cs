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
    [Route("api/[controller]")]
    public class DoctorAuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IConfiguration _configuration;
        private readonly IEmailValidationService _emailValidationService;

        public DoctorAuthController(
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

        // POST: api/DoctorAuth/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] DoctorRegisterRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Registration data is required." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    error = "Validation failed",
                    details = errors
                });
            }

            try
            {
                // Validate registration key
                var validRegistrationKey = _configuration["RegistrationSettings:DoctorRegistrationKey"];
                
                if (string.IsNullOrEmpty(validRegistrationKey))
                {
                    return StatusCode(500, new { error = "Server configuration error. Contact system administrator." });
                }

                if (request.RegistrationKey != validRegistrationKey)
                {
                    return Unauthorized(new { error = "Invalid registration key. Contact your clinic administrator for the correct key." });
                }

                // Check if email already exists
                var isEmailUsed = await _emailValidationService.IsEmailUsedAsync(request.Email);
                if (isEmailUsed)
                {
                    return BadRequest(new { error = "Email already registered." });
                }

                // Create new doctor
                var doctor = new Doctor
                {
                    Name = request.Name,
                    Phone = request.Phone,
                    Email = request.Email.Trim().ToLowerInvariant(),
                    PasswordHash = _passwordHashService.HashPassword(request.Password)
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                var token = _jwtService.GenerateToken(doctor.ID, doctor.Email, doctor.Name, "Doctor");

                return Ok(new DoctorLoginResponse
                {
                    Token = token,
                    DoctorId = doctor.ID,
                    Name = doctor.Name,
                    Email = doctor.Email,
                    Phone = doctor.Phone
                });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }

        // POST: api/DoctorAuth/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] DoctorLoginRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Login data is required." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    error = "Validation failed",
                    details = errors
                });
            }

            try
            {
                var normalizedEmail = request.Email.Trim().ToLowerInvariant();
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == normalizedEmail);
                if (doctor == null || !_passwordHashService.VerifyPassword(request.Password, doctor.PasswordHash))
                {
                    return Unauthorized(new { error = "Invalid email or password." });
                }

                var token = _jwtService.GenerateToken(doctor.ID, doctor.Email, doctor.Name, "Doctor");

                return Ok(new DoctorLoginResponse
                {
                    Token = token,
                    DoctorId = doctor.ID,
                    Name = doctor.Name,
                    Email = doctor.Email,
                    Phone = doctor.Phone
                });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }
    }
}
