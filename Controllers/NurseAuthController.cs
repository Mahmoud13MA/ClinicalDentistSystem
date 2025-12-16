using clinical.APIs.Data;
using clinical.APIs.DTOs;
using clinical.APIs.Models;
using clinical.APIs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NurseAuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IConfiguration _configuration;

        public NurseAuthController(
            AppDbContext context, 
            IJwtService jwtService, 
            IPasswordHashService passwordHashService,
            IConfiguration configuration)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHashService = passwordHashService;
            _configuration = configuration;
        }

        // POST: api/NurseAuth/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] NurseRegisterRequest request)
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
                var validRegistrationKey = _configuration["RegistrationSettings:NurseRegistrationKey"];
                
                if (string.IsNullOrEmpty(validRegistrationKey))
                {
                    return StatusCode(500, new { error = "Server configuration error. Contact system administrator." });
                }

                if (request.RegistrationKey != validRegistrationKey)
                {
                    return Unauthorized(new { error = "Invalid registration key. Contact your clinic administrator for the correct key." });
                }

                // Check if email already exists
                var existingNurse = await _context.Nurses.FirstOrDefaultAsync(n => n.Email == request.Email);
                if (existingNurse != null)
                {
                    return BadRequest(new { error = "Email already registered." });
                }

                // Create new nurse
                var nurse = new Nurse
                {
                    Name = request.Name,
                    Phone = request.Phone,
                    Email = request.Email,
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
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }

        // POST: api/NurseAuth/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] NurseLoginRequest request)
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
                var nurse = await _context.Nurses.FirstOrDefaultAsync(n => n.Email == request.Email);
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
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }
    }
}
