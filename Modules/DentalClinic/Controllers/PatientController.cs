using clinical.APIs.Modules.DentalClinic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinical.APIs.Shared.Data;
using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Services;

namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PatientController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPatientMappingService _mappingService;
        private readonly IProfileManagementService _profileManagement;

        public PatientController(AppDbContext context, IPatientMappingService mappingService, IProfileManagementService profileManagement)
        {
            _context = context;
            _mappingService = mappingService;
            _profileManagement = profileManagement;
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
        public async Task<IActionResult> UpdatePatient(int Patient_ID, [FromBody] UpdatePatientInfoRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Patient data is required." });
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
                var result = await _profileManagement.UpdatePatientInfoAsync(Patient_ID, request);

                if (!result.IsSuccess)
                {
                    if (result.ErrorMessage == "Patient not found")
                        return NotFound(new { error = result.ErrorMessage, patient_ID = Patient_ID });

                    return BadRequest(new { error = result.ErrorMessage });
                }

                var updatedPatient = await _context.Patients.FindAsync(Patient_ID);
                var response = _mappingService.MapToResponse(updatedPatient);

                return Ok(new { message = "Patient updated successfully.", patient = response });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }
    }
}   
