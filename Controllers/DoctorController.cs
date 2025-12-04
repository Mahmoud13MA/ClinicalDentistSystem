using clinical.APIs.Data;
using clinical.APIs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DoctorController : Controller
    {
        private readonly AppDbContext _context;

        public DoctorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetDoctor()
        {
            var doctors = await _context.Doctors.ToListAsync();
            if (doctors == null || doctors.Count == 0)
            {
                return NotFound();
            }
            return Ok(doctors);
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetDoctorById(int ID)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.ID == ID);
            if (doctor == null)
            {
                return NotFound();
            }
            return Ok(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctor([FromBody] Doctor doctor)
        {
            if (doctor == null)
            {
                return BadRequest(new { error = "Doctor data is required.", hint = "Make sure you're sending a valid JSON body with doctor information." });
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
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetDoctorById), new { ID = doctor.ID }, doctor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("{ID}")]
        public async Task<IActionResult> UpdateDoctor(int ID, [FromBody] Doctor doctor)
        {
            if (doctor == null)
            {
                return BadRequest(new { error = "Doctor data is required." });
            }

            if (ID != doctor.ID)
            {
                return BadRequest(new { error = "Doctor ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
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
                // Check if doctor exists
                var existingDoctor = await _context.Doctors.FindAsync(ID);
                if (existingDoctor == null)
                {
                    return NotFound(new { error = "Doctor not found.", doctor_ID = ID });
                }

                // Update doctor properties
                existingDoctor.Name = doctor.Name;
                existingDoctor.Phone = doctor.Phone;

                _context.Doctors.Update(existingDoctor);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Doctor updated successfully.", doctor = existingDoctor });
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if doctor still exists
                if (!await _context.Doctors.AnyAsync(d => d.ID == ID))
                {
                    return NotFound(new { error = "Doctor not found during update.", doctor_ID = ID });
                }
                throw;
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }
    }
}
