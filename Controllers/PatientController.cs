using clinical.APIs.Data;
using clinical.APIs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Controllers
{
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
    }
}   
