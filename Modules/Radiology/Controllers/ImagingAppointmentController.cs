using clinical.APIs.Modules.Radiology.DTOs;
using clinical.APIs.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radiology.Models;

namespace clinical.APIs.Modules.Radiology.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagingAppointmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ImagingAppointmentController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all imaging appointments
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllImagingAppointments()
        {
            try
            {
                var appointments = await _context.ImagingAppointments
                    .Include(ia => ia.Patient)
                    .Include(ia => ia.Radiologist)
                    .Include(ia => ia.Equipment)
                    .ToListAsync();

                if (appointments == null || appointments.Count == 0)
                {
                    return NotFound(new { error = "No imaging appointments found." });
                }

                var response = appointments.Select(ia => MapToResponse(ia)).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific imaging appointment by ID
        /// </summary>
        [HttpGet("{imagingId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImagingAppointmentById(int imagingId)
        {
            try
            {
                var appointment = await _context.ImagingAppointments
                    .Include(ia => ia.Patient)
                    .Include(ia => ia.Radiologist)
                    .Include(ia => ia.Equipment)
                    .FirstOrDefaultAsync(ia => ia.ImagingID == imagingId);

                if (appointment == null)
                {
                    return NotFound(new { error = "Imaging appointment not found.", imaging_ID = imagingId });
                }

                var response = MapToResponse(appointment);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get imaging appointments by patient
        /// </summary>
        [HttpGet("bypatient/{patientId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImagingAppointmentsByPatient(int patientId)
        {
            try
            {
                var appointments = await _context.ImagingAppointments
                    .Where(ia => ia.PatientID == patientId)
                    .Include(ia => ia.Patient)
                    .Include(ia => ia.Radiologist)
                    .Include(ia => ia.Equipment)
                    .ToListAsync();

                if (appointments == null || appointments.Count == 0)
                {
                    return NotFound(new { error = "No imaging appointments found for this patient.", patient_ID = patientId });
                }

                var response = appointments.Select(ia => MapToResponse(ia)).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get imaging appointments by radiologist
        /// </summary>
        [HttpGet("byradiologist/{radiologistId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImagingAppointmentsByRadiologist(int radiologistId)
        {
            try
            {
                var appointments = await _context.ImagingAppointments
                    .Where(ia => ia.RadiologistID == radiologistId)
                    .Include(ia => ia.Patient)
                    .Include(ia => ia.Radiologist)
                    .Include(ia => ia.Equipment)
                    .ToListAsync();

                if (appointments == null || appointments.Count == 0)
                {
                    return NotFound(new { error = "No imaging appointments found for this radiologist.", radiologist_ID = radiologistId });
                }

                var response = appointments.Select(ia => MapToResponse(ia)).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get imaging appointments by equipment
        /// </summary>
        [HttpGet("byequipment/{equipmentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImagingAppointmentsByEquipment(int equipmentId)
        {
            try
            {
                var appointments = await _context.ImagingAppointments
                    .Where(ia => ia.EquipmentID == equipmentId)
                    .Include(ia => ia.Patient)
                    .Include(ia => ia.Radiologist)
                    .Include(ia => ia.Equipment)
                    .ToListAsync();

                if (appointments == null || appointments.Count == 0)
                {
                    return NotFound(new { error = "No imaging appointments found for this equipment.", equipment_ID = equipmentId });
                }

                var response = appointments.Select(ia => MapToResponse(ia)).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Create a new imaging appointment
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateImagingAppointment([FromBody] ImagingAppointmentCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Imaging appointment data is required.", hint = "Make sure you're sending a valid JSON body with appointment information." });
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
                    hint = "Required fields: Datetime, Type, PatientID, RadiologistID, EquipmentID"
                });
            }

            try
            {
                // Validate foreign keys
                var patientExists = await _context.RadiologyPatients.AnyAsync(p => p.PatientID == request.PatientID);
                if (!patientExists)
                {
                    return BadRequest(new { error = "Invalid Patient ID.", patient_ID = request.PatientID });
                }

                var radiologistExists = await _context.Radiologists.AnyAsync(r => r.RadiologistID == request.RadiologistID);
                if (!radiologistExists)
                {
                    return BadRequest(new { error = "Invalid Radiologist ID.", radiologist_ID = request.RadiologistID });
                }

                var equipmentExists = await _context.Equipment.AnyAsync(e => e.EquipmentID == request.EquipmentID);
                if (!equipmentExists)
                {
                    return BadRequest(new { error = "Invalid Equipment ID.", equipment_ID = request.EquipmentID });
                }

                var appointment = new ImagingAppointment
                {
                    Datetime = request.Datetime,
                    Type = request.Type,
                    PatientID = request.PatientID,
                    RadiologistID = request.RadiologistID,
                    EquipmentID = request.EquipmentID
                };

                _context.ImagingAppointments.Add(appointment);
                await _context.SaveChangesAsync();

                // Fetch the created appointment with related data
                var createdAppointment = await _context.ImagingAppointments
                    .Include(ia => ia.Patient)
                    .Include(ia => ia.Radiologist)
                    .Include(ia => ia.Equipment)
                    .FirstOrDefaultAsync(ia => ia.ImagingID == appointment.ImagingID);

                var response = MapToResponse(createdAppointment);
                return CreatedAtAction(nameof(GetImagingAppointmentById), new { imagingId = appointment.ImagingID }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing imaging appointment
        /// </summary>
        [HttpPut("{imagingId}")]
        [Authorize]
        public async Task<IActionResult> UpdateImagingAppointment(int imagingId, [FromBody] ImagingAppointmentUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Imaging appointment data is required." });
            }

            if (imagingId != request.ImagingID)
            {
                return BadRequest(new { error = "Imaging appointment ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
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
                var existingAppointment = await _context.ImagingAppointments.FindAsync(imagingId);
                if (existingAppointment == null)
                {
                    return NotFound(new { error = "Imaging appointment not found.", imaging_ID = imagingId });
                }

                // Validate foreign keys
                var patientExists = await _context.RadiologyPatients.AnyAsync(p => p.PatientID == request.PatientID);
                if (!patientExists)
                {
                    return BadRequest(new { error = "Invalid Patient ID.", patient_ID = request.PatientID });
                }

                var radiologistExists = await _context.Radiologists.AnyAsync(r => r.RadiologistID == request.RadiologistID);
                if (!radiologistExists)
                {
                    return BadRequest(new { error = "Invalid Radiologist ID.", radiologist_ID = request.RadiologistID });
                }

                var equipmentExists = await _context.Equipment.AnyAsync(e => e.EquipmentID == request.EquipmentID);
                if (!equipmentExists)
                {
                    return BadRequest(new { error = "Invalid Equipment ID.", equipment_ID = request.EquipmentID });
                }

                // Update appointment properties
                existingAppointment.Datetime = request.Datetime;
                existingAppointment.Type = request.Type;
                existingAppointment.PatientID = request.PatientID;
                existingAppointment.RadiologistID = request.RadiologistID;
                existingAppointment.EquipmentID = request.EquipmentID;

                _context.ImagingAppointments.Update(existingAppointment);
                await _context.SaveChangesAsync();

                // Fetch updated appointment with related data
                var updatedAppointment = await _context.ImagingAppointments
                    .Include(ia => ia.Patient)
                    .Include(ia => ia.Radiologist)
                    .Include(ia => ia.Equipment)
                    .FirstOrDefaultAsync(ia => ia.ImagingID == imagingId);

                var response = MapToResponse(updatedAppointment);
                return Ok(new { message = "Imaging appointment updated successfully.", imaging_appointment = response });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.ImagingAppointments.AnyAsync(ia => ia.ImagingID == imagingId))
                {
                    return NotFound(new { error = "Imaging appointment not found during update.", imaging_ID = imagingId });
                }
                throw;
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", details = innerMessage });
            }
        }

        /// <summary>
        /// Map ImagingAppointment model to ImagingAppointmentResponse DTO
        /// </summary>
        private ImagingAppointmentResponse MapToResponse(ImagingAppointment appointment)
        {
            return new ImagingAppointmentResponse
            {
                ImagingID = appointment.ImagingID,
                Datetime = appointment.Datetime,
                Type = appointment.Type,
                PatientID = appointment.PatientID,
                Patient = appointment.Patient != null ? new PatientBasicInfoRadiology
                {
                    PatientID = appointment.Patient.PatientID
                } : null,
                RadiologistID = appointment.RadiologistID,
                Radiologist = appointment.Radiologist != null ? new RadiologistBasicInfo
                {
                    RadiologistID = appointment.Radiologist.RadiologistID,
                    Name = appointment.Radiologist.Name,
                    Phone = appointment.Radiologist.Phone,
                    Email = appointment.Radiologist.Email,
                    Specialty = appointment.Radiologist.Specialty
                } : null,
                EquipmentID = appointment.EquipmentID,
                Equipment = appointment.Equipment != null ? new EquipmentResponseBasicInfo
                {
                    EquipmentID = appointment.Equipment.EquipmentID,
                    Type = appointment.Equipment.Type,
                    Model = appointment.Equipment.Model
                } : null
            };
        }
    }
}
