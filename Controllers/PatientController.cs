using clinical.APIs.Data;
using clinical.APIs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PatientController : Controller
    {
        private readonly AppDbContext _context;

        public PatientController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetPatient()
        {
            var patients = await _context.Patients.ToListAsync();
            if (patients == null || patients.Count == 0)
            {
                return NotFound();

            }
            return Ok(patients);
        }

        [HttpGet("{Patient_ID}")]
        public async Task<IActionResult> GetPatientById(int Patient_ID)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Patient_ID == Patient_ID);
            if (patient == null)
            {
                return NotFound();
            }
            return Ok(patient);
        }
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] Patient patient)
        {
            if (patient == null)
            {
                return BadRequest(new { error = "Patient data is required.", hint = "Make sure you're sending a valid JSON body with patient information." });
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
                    hint = "Required fields: First, Last, Gender, DOB (format: YYYY-MM-DD or YYYY-MM-DDTHH:mm:ss)"
                });
            }

            try
            {
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetPatientById), new { Patient_ID = patient.Patient_ID }, patient);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("{Patient_ID}")]
        public async Task<IActionResult> UpdatePatient(int Patient_ID, [FromBody] Patient patient)
        {
            if (patient == null)
            {
                return BadRequest(new { error = "Patient data is required." });
            }

            if (Patient_ID != patient.Patient_ID)
            {
                return BadRequest(new { error = "Patient ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
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
                // Check if patient exists
                var existingPatient = await _context.Patients.FindAsync(Patient_ID);
                if (existingPatient == null)
                {
                    return NotFound(new { error = "Patient not found.", patient_ID = Patient_ID });
                }

                // Update patient properties
                existingPatient.First = patient.First;
                existingPatient.Middle = patient.Middle;
                existingPatient.Last = patient.Last;
                existingPatient.Gender = patient.Gender;
                existingPatient.DOB = patient.DOB;

                _context.Patients.Update(existingPatient);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Patient updated successfully.", patient = existingPatient });
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if patient still exists
                if (!await _context.Patients.AnyAsync(p => p.Patient_ID == Patient_ID))
                {
                    return NotFound(new { error = "Patient not found during update.", patient_ID = Patient_ID });
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
