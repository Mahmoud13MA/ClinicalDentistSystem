using Microsoft.AspNetCore.Mvc;
using clinical.APIs.Data;
using clinical.APIs.Models;
using clinical.APIs.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using clinical.APIs.DTOs;

namespace clinical.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorAuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public DoctorAuthController(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
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
                // Check if email already exists
                var existingDoctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == request.Email);
                if (existingDoctor != null)
                {
                    return BadRequest(new { error = "Email already registered." });
                }

                // Create new doctor
                var doctor = new Doctor
                {
                    Name = request.Name,
                    Phone = request.Phone,
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password)
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
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == request.Email);
                if (doctor == null || !VerifyPassword(request.Password, doctor.PasswordHash))
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

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }
    }
}
