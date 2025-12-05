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
    public class NurseAuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public NurseAuthController(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
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
                    PasswordHash = HashPassword(request.Password)
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
                if (nurse == null || !VerifyPassword(request.Password, nurse.PasswordHash))
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
