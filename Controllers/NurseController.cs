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

        public NurseController(AppDbContext context, INurseMappingService mappingService)
        {
            _context = context;
            _mappingService = mappingService;
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
        public async Task<IActionResult> CreateNurse([FromBody] Nurse nurse)
        {
            if (nurse == null)
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
                    hint = "Required fields: Name, Phone"
                });
            }

            try
            {
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
