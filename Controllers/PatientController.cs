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
    public class PatientController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPatientMappingService _mappingService;

        public PatientController(AppDbContext context, IPatientMappingService mappingService)
        {
            _context = context;
            _mappingService = mappingService;
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

            var response = _mappingService.MapToResponseList(patients);
            return Ok(response);
        }

        [HttpGet("{Patient_ID}")]
        public async Task<IActionResult> GetPatientById(int Patient_ID)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Patient_ID == Patient_ID);
            if (patient == null)
            {
                return NotFound();
            }

            var response = _mappingService.MapToResponse(patient);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] PatientCreateRequest request)
        {
            if (request == null)
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
                var patient = new Patient
                {
                    First = request.First,
                    Middle = request.Middle,
                    Last = request.Last,
                    Gender = request.Gender,
                    DOB = request.DOB,
                    Phone = request.Phone
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                var response = _mappingService.MapToResponse(patient);
                return CreatedAtAction(nameof(GetPatientById), new { Patient_ID = patient.Patient_ID }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("{Patient_ID}")]
        public async Task<IActionResult> UpdatePatient(int Patient_ID, [FromBody] PatientUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Patient data is required." });
            }

            if (Patient_ID != request.Patient_ID)
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
                existingPatient.First = request.First;
                existingPatient.Middle = request.Middle;
                existingPatient.Last = request.Last;
                existingPatient.Gender = request.Gender;
                existingPatient.DOB = request.DOB;
                existingPatient.Phone = request.Phone;

                _context.Patients.Update(existingPatient);
                await _context.SaveChangesAsync();

                var response = _mappingService.MapToResponse(existingPatient);
                return Ok(new { message = "Patient updated successfully.", patient = response });
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
