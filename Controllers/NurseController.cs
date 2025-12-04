using Microsoft.AspNetCore.Mvc;
using clinical.APIs.Data;
using clinical.APIs.Models;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NurseController : Controller
    {
        private readonly AppDbContext _context;

        public NurseController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Nurse
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetNurses()
        {
            var nurses = await _context.Nurses
                .Include(n => n.Appointments)
                .ToListAsync();

            if (nurses == null || nurses.Count == 0)
            {
                return NotFound(new { message = "No nurses found." });
            }

            return Ok(nurses);
        }

        // GET: /Nurse/{id}
        [HttpGet("{NURSE_ID}")]
        public async Task<IActionResult> GetNurseById(int NURSE_ID)
        {
            var nurse = await _context.Nurses
                .Include(n => n.Appointments)
                .FirstOrDefaultAsync(n => n.NURSE_ID == NURSE_ID);

            if (nurse == null)
            {
                return NotFound(new { error = "Nurse not found.", nurse_ID = NURSE_ID });
            }

            return Ok(nurse);
        }

        // POST: /Nurse
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

                return CreatedAtAction(nameof(GetNurseById), new { NURSE_ID = nurse.NURSE_ID }, nurse);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }

        // PUT: /Nurse/{id}
        [HttpPut("{NURSE_ID}")]
        public async Task<IActionResult> UpdateNurse(int NURSE_ID, [FromBody] Nurse nurse)
        {
            if (nurse == null)
            {
                return BadRequest(new { error = "Nurse data is required." });
            }

            if (NURSE_ID != nurse.NURSE_ID)
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
                existingNurse.Name = nurse.Name;
                existingNurse.Phone = nurse.Phone;

                _context.Nurses.Update(existingNurse);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Nurse updated successfully.", nurse = existingNurse });
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
