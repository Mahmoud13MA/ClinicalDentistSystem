using clinical.APIs.Data;
using clinical.APIs.Models;
using clinical.APIs.DTOs;
using clinical.APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class NurseController : Controller
    {
        private readonly AppDbContext _context;
        private readonly INurseMappingService _mappingService;
        private readonly IPasswordHashService _passwordHashService;

        public NurseController(AppDbContext context, INurseMappingService mappingService, IPasswordHashService passwordHashService)
        {
            _context = context;
            _mappingService = mappingService;
            _passwordHashService = passwordHashService;
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
                var emailExists = await _context.Nurses.AnyAsync(n => n.Email == request.Email);
                if (emailExists)
                {
                    return BadRequest(new { error = "Email already registered." });
                }

                var nurse = new Nurse
                {
                    Name = request.Name,
                    Phone = request.Phone,
                    Email = request.Email,
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
        public async Task<IActionResult> UpdateNurse(int NURSE_ID, [FromBody] NurseUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Nurse data is required." });
            }

            if (NURSE_ID != request.NURSE_ID)
            {
                return BadRequest(new { error = "Nurse ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
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
                // Check if nurse exists
                var existingNurse = await _context.Nurses.FindAsync(NURSE_ID);
                if (existingNurse == null)
                {
                    return NotFound(new { error = "Nurse not found.", nurse_ID = NURSE_ID });
                }

                // Check if email is being changed and if new email already exists
                if (existingNurse.Email != request.Email)
                {
                    var emailExists = await _context.Nurses
                        .AnyAsync(n => n.Email == request.Email && n.NURSE_ID != NURSE_ID);
                    if (emailExists)
                    {
                        return BadRequest(new { error = "Email already in use by another nurse." });
                    }
                }

                // Update nurse properties
                existingNurse.Name = request.Name;
                existingNurse.Phone = request.Phone;
                existingNurse.Email = request.Email;

                _context.Nurses.Update(existingNurse);
                await _context.SaveChangesAsync();

                var response = _mappingService.MapToResponse(existingNurse);
                return Ok(new { message = "Nurse updated successfully.", nurse = response });
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if nurse still exists
                if (!await _context.Nurses.AnyAsync(n => n.NURSE_ID == NURSE_ID))
                {
                    return NotFound(new { error = "Nurse not found during update.", nurse_ID = NURSE_ID });
                }
                throw;
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
