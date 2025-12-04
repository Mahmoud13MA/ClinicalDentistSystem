using clinical.APIs.Data;
using clinical.APIs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EHRController : Controller
    {
        private readonly AppDbContext _context;

        public EHRController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetEHR()
        {
            var ehrs = await _context.EHRs.Include(e => e.Patient).Include(e => e.Appointment).ToListAsync();
            if (ehrs == null || ehrs.Count == 0)
            {
                return NotFound();
            }
            return Ok(ehrs);
        }

        [HttpGet("{EHR_ID}")]
        public async Task<IActionResult> GetEHRById(int EHR_ID)
        {
            var ehr = await _context.EHRs.Include(e => e.Patient).Include(e => e.Appointment).FirstOrDefaultAsync(e => e.EHR_ID == EHR_ID);
            if (ehr == null)
            {
                return NotFound();
            }
            return Ok(ehr);
        }

        [HttpGet("patient/{Patient_ID}")]
        public async Task<IActionResult> GetEHRByPatientId(int Patient_ID)
        {
            var ehrs = await _context.EHRs.Include(e => e.Patient).Include(e => e.Appointment).Where(e => e.Patient_ID == Patient_ID).ToListAsync();
            if (ehrs == null || ehrs.Count == 0)
            {
                return NotFound();
            }
            return Ok(ehrs);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEHR([FromBody] EHR ehr)
        {
            if (ehr == null)
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
                var patient = await _context.Patients.FindAsync(ehr.Patient_ID);
                if (patient == null)
                {
                    return BadRequest(new { error = "Patient not found.", patient_ID = ehr.Patient_ID });
                }

                // Verify that the appointment exists
                var appointment = await _context.Appointments.FindAsync(ehr.AppointmentId);
                if (appointment == null)
                {
                    return BadRequest(new { error = "Appointment not found.", appointment_id = ehr.AppointmentId });
                }

                ehr.Last_Updated = DateTime.Now;
                _context.EHRs.Add(ehr);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetEHRById), new { EHR_ID = ehr.EHR_ID }, ehr);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("{EHR_ID}")]
        public async Task<IActionResult> UpdateEHR(int EHR_ID, [FromBody] EHR ehr)
        {
            if (ehr == null)
            {
                return BadRequest(new { error = "EHR data is required." });
            }

            if (EHR_ID != ehr.EHR_ID)
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
                if (existingEHR.Patient_ID != ehr.Patient_ID)
                {
                    var patient = await _context.Patients.FindAsync(ehr.Patient_ID);
                    if (patient == null)
                    {
                        return BadRequest(new { error = "Patient not found.", patient_ID = ehr.Patient_ID });
                    }
                }

                // Verify that the appointment exists if AppointmentId is being changed
                if (existingEHR.AppointmentId != ehr.AppointmentId)
                {
                    var appointment = await _context.Appointments.FindAsync(ehr.AppointmentId);
                    if (appointment == null)
                    {
                        return BadRequest(new { error = "Appointment not found.", appointment_id = ehr.AppointmentId });
                    }
                }

                // Update EHR properties
                existingEHR.Medications = ehr.Medications;
                existingEHR.Allergies = ehr.Allergies;
                existingEHR.History = ehr.History;
                existingEHR.Treatments = ehr.Treatments;
                existingEHR.Patient_ID = ehr.Patient_ID;
                existingEHR.AppointmentId = ehr.AppointmentId;
                existingEHR.Last_Updated = DateTime.Now;

                _context.EHRs.Update(existingEHR);
                await _context.SaveChangesAsync();

                return Ok(new { message = "EHR updated successfully.", ehr = existingEHR });
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if EHR still existas
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
