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
    public class EHRController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEHRMappingService _mappingService;

        public EHRController(AppDbContext context, IEHRMappingService mappingService)
        {
            _context = context;
            _mappingService = mappingService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetEHR()
        {
            var ehrs = await _context.EHRs
                .Include(e => e.Patient)
                .Include(e => e.Appointment)
                .ToListAsync();

            if (ehrs == null || ehrs.Count == 0)
            {
                return NotFound();
            }

            var response = _mappingService.MapToResponseList(ehrs);
            return Ok(response);
        }

        [HttpGet("{EHR_ID}")]
        public async Task<IActionResult> GetEHRById(int EHR_ID)
        {
            var ehr = await _context.EHRs
                .Include(e => e.Patient)
                .Include(e => e.Appointment)
                .FirstOrDefaultAsync(e => e.EHR_ID == EHR_ID);

            if (ehr == null)
            {
                return NotFound();
            }

            var response = _mappingService.MapToResponse(ehr);
            return Ok(response);
        }

        [HttpGet("patient/{Patient_ID}")]
        public async Task<IActionResult> GetEHRByPatientId(int Patient_ID)
        {
            var ehrs = await _context.EHRs
                .Include(e => e.Patient)
                .Include(e => e.Appointment)
                .Where(e => e.Patient_ID == Patient_ID)
                .ToListAsync();

            if (ehrs == null || ehrs.Count == 0)
            {
                return NotFound();
            }

            var response = _mappingService.MapToResponseList(ehrs);
            return Ok(response);
        }

        [Authorize(Policy = "DoctorOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateEHR([FromBody] EHRCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "EHR data is required.", hint = "Make sure you're sending a valid JSON body with EHR information." });
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
                    hint = "Required fields: Patient_ID, AppointmentId"
                });
            }

            try
            {
                // Verify that the patient exists
                var patient = await _context.Patients.FindAsync(request.Patient_ID);
                if (patient == null)
                {
                    return BadRequest(new { error = "Patient not found.", patient_ID = request.Patient_ID });
                }

                // Verify that the appointment exists
                var appointment = await _context.Appointments.FindAsync(request.AppointmentId);
                if (appointment == null)
                {
                    return BadRequest(new { error = "Appointment not found.", appointment_id = request.AppointmentId });
                }

                var ehr = new EHR
                {
                    Medications = request.Medications,
                    Allergies = request.Allergies,
                    History = request.History,
                    Treatments = request.Treatments,
                    Patient_ID = request.Patient_ID,
                    AppointmentId = request.AppointmentId,
                    Last_Updated = DateTime.Now
                };

                _context.EHRs.Add(ehr);
                await _context.SaveChangesAsync();

                // Load related entities for response
                var createdEHR = await _context.EHRs
                    .Include(e => e.Patient)
                    .Include(e => e.Appointment)
                    .FirstOrDefaultAsync(e => e.EHR_ID == ehr.EHR_ID);

                var response = _mappingService.MapToResponse(createdEHR);
                return CreatedAtAction(nameof(GetEHRById), new { EHR_ID = ehr.EHR_ID }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [Authorize(Policy = "DoctorOnly")]
        [HttpPut("{EHR_ID}")]
        public async Task<IActionResult> UpdateEHR(int EHR_ID, [FromBody] EHRUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "EHR data is required." });
            }

            if (EHR_ID != request.EHR_ID)
            {
                return BadRequest(new { error = "EHR ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
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
                // Check if EHR exists
                var existingEHR = await _context.EHRs.FindAsync(EHR_ID);
                if (existingEHR == null)
                {
                    return NotFound(new { error = "EHR not found.", ehr_id = EHR_ID });
                }

                // Verify that the patient exists if Patient_ID is being changed
                if (existingEHR.Patient_ID != request.Patient_ID)
                {
                    var patient = await _context.Patients.FindAsync(request.Patient_ID);
                    if (patient == null)
                    {
                        return BadRequest(new { error = "Patient not found.", patient_ID = request.Patient_ID });
                    }
                }

                // Verify that the appointment exists if AppointmentId is being changed
                if (existingEHR.AppointmentId != request.AppointmentId)
                {
                    var appointment = await _context.Appointments.FindAsync(request.AppointmentId);
                    if (appointment == null)
                    {
                        return BadRequest(new { error = "Appointment not found.", appointment_id = request.AppointmentId });
                    }
                }

                // Update EHR properties
                existingEHR.Medications = request.Medications;
                existingEHR.Allergies = request.Allergies;
                existingEHR.History = request.History;
                existingEHR.Treatments = request.Treatments;
                existingEHR.Patient_ID = request.Patient_ID;
                existingEHR.AppointmentId = request.AppointmentId;
                existingEHR.Last_Updated = DateTime.Now;

                _context.EHRs.Update(existingEHR);
                await _context.SaveChangesAsync();

                // Load related entities for response
                var updatedEHR = await _context.EHRs
                    .Include(e => e.Patient)
                    .Include(e => e.Appointment)
                    .FirstOrDefaultAsync(e => e.EHR_ID == EHR_ID);

                var response = _mappingService.MapToResponse(updatedEHR);
                return Ok(new { message = "EHR updated successfully.", ehr = response });
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if EHR still exists
                if (!await _context.EHRs.AnyAsync(e => e.EHR_ID == EHR_ID))
                {
                    return NotFound(new { error = "EHR not found during update.", ehr_id = EHR_ID });
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
