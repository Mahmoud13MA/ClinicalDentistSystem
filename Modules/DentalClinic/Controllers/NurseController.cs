using clinical.APIs.Modules.DentalClinic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
using clinical.APIs.Shared.Services;
using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Services;

namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class NurseController : Controller
    {
        private readonly AppDbContext _context;
        private readonly INurseMappingService _mappingService;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IEmailValidationService _emailValidationService;
        private readonly IProfileManagementService _profileManagement;

        public NurseController(AppDbContext context, INurseMappingService mappingService, IPasswordHashService passwordHashService, IEmailValidationService emailValidationService, IProfileManagementService profileManagement)
        {
            _context = context;
            _mappingService = mappingService;
            _passwordHashService = passwordHashService;
            _emailValidationService = emailValidationService;
            _profileManagement = profileManagement;
        }

        // GET: /Nurse
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetNurses()
        {
            var nurses = await _context.Nurses.ToListAsync();

            if (nurses == null || nurses.Count == 0)
            {
                return NotFound(new { message = "No nurses found." });
            }

            var response = _mappingService.MapToResponseList(nurses);
            return Ok(response);
        }

        // GET: /Nurse/{id}
        [HttpGet("{NURSE_ID}")]
        public async Task<IActionResult> GetNurseById(int NURSE_ID)
        {
            var nurse = await _context.Nurses.FirstOrDefaultAsync(n => n.NURSE_ID == NURSE_ID);

            if (nurse == null)
            {
                return NotFound(new { error = "Nurse not found.", nurse_ID = NURSE_ID });
            }

            var response = _mappingService.MapToResponse(nurse);
            return Ok(response);
        }

        // POST: /Nurse
        [Authorize(Policy = "DoctorOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateNurse([FromBody] NurseCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Nurse data is required.", hint = "Make sure you're sending a valid JSON body with nurse information." });
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
                    details = errors,
                    hint = "Required fields: Name, Phone, Email, Password (minimum 8 characters)"
                });
            }

            try
            {
                // Check if email already exists
                var emailExists = await _emailValidationService.IsEmailUsedAsync(request.Email);
                if (emailExists)
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

                var response = _mappingService.MapToResponse(nurse);
                return CreatedAtAction(nameof(GetNurseById), new { NURSE_ID = nurse.NURSE_ID }, response);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }

        // PUT: /Nurse/{id}
        [Authorize(Policy = "DoctorOnly")]
        [HttpPut("{NURSE_ID}")]
        public async Task<IActionResult> UpdateNurse(int NURSE_ID, [FromBody] UpdateStaffInfoRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Nurse data is required." });
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
                var result = await _profileManagement.UpdateNurseInfoAsync(NURSE_ID, request);

                if (!result.IsSuccess)
                {
                    if (result.ErrorMessage == "Nurse not found")
                        return NotFound(new { error = result.ErrorMessage, nurse_ID = NURSE_ID });

                    return BadRequest(new { error = result.ErrorMessage });
                }

                // Fetch updated nurse to return
                var updatedNurse = await _context.Nurses.FindAsync(NURSE_ID);
                var response = _mappingService.MapToResponse(updatedNurse);

                return Ok(new { message = "Nurse updated successfully.", nurse = response });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }

        // DELETE: /Nurse/{id}
        [Authorize(Policy ="DoctorOnly")]
        [HttpDelete("{NURSE_ID}")]
        public async Task<IActionResult> DeleteNurse(int NURSE_ID)
        {
            try
            {
                var nurse = await _context.Nurses.FindAsync(NURSE_ID);
                if (nurse == null)
                {
                    return NotFound(new { error = "Nurse not found.", nurse_ID = NURSE_ID });
                }

                _context.Nurses.Remove(nurse);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Nurse deleted successfully.", nurse_ID = NURSE_ID });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }
    }
}
