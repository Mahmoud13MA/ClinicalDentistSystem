using AutoMapper;
using AutoMapper.QueryableExtensions;
using clinical.APIs.Modules.Radiology.DTOs;
using clinical.APIs.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Radiology.Models;

namespace clinical.APIs.Modules.Radiology.Controllers
{
    [Authorize(Policy = "RadiologistOrAdmin")]
    [ApiController]
    [Route("api/v1/radiology/[controller]")]
    public class ImagingAppointmentController(
        AppDbContext context,
        IMapper mapper,
        ILogger<ImagingAppointmentController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllImagingAppointments()
        {
            var appointments = await context.ImagingAppointments
                .ProjectTo<ImagingAppointmentResponse>(mapper.ConfigurationProvider)
                .ToListAsync();

            // ← Fix: empty collection is Ok([]), not NotFound
            return Ok(appointments);
        }

        [HttpGet("{imagingId}")]
        public async Task<IActionResult> GetImagingAppointmentById(int imagingId)
        {
            var appointment = await context.ImagingAppointments
                .Include(ia => ia.Patient)
                .Include(ia => ia.Radiologist)
                .Include(ia => ia.Equipment)
                .FirstOrDefaultAsync(ia => ia.ImagingID == imagingId);

            if (appointment == null)
                return NotFound(new { error = "Imaging appointment not found.", imaging_ID = imagingId });

            return Ok(mapper.Map<ImagingAppointmentResponse>(appointment));
        }

        [HttpGet("bypatient/{patientId}")]
        public async Task<IActionResult> GetImagingAppointmentsByPatient(int patientId)
        {
            var appointments = await context.ImagingAppointments
                .Where(ia => ia.PatientID == patientId)
                .ProjectTo<ImagingAppointmentResponse>(mapper.ConfigurationProvider)
                .ToListAsync();

            // ← Fix: empty collection is Ok([]), not NotFound
            return Ok(appointments);
        }

        [HttpGet("byradiologist/{radiologistId}")]
        public async Task<IActionResult> GetImagingAppointmentsByRadiologist(int radiologistId)
        {
            var appointments = await context.ImagingAppointments
                .Where(ia => ia.RadiologistID == radiologistId)
                .ProjectTo<ImagingAppointmentResponse>(mapper.ConfigurationProvider)
                .ToListAsync();

            // ← Fix: empty collection is Ok([]), not NotFound
            return Ok(appointments);
        }

        [HttpGet("byequipment/{equipmentId}")]
        public async Task<IActionResult> GetImagingAppointmentsByEquipment(int equipmentId)
        {
            var appointments = await context.ImagingAppointments
                .Where(ia => ia.EquipmentID == equipmentId)
                .ProjectTo<ImagingAppointmentResponse>(mapper.ConfigurationProvider)
                .ToListAsync();

            // ← Fix: empty collection is Ok([]), not NotFound
            return Ok(appointments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateImagingAppointment([FromBody] ImagingAppointmentCreateRequest request)
        {
            var patient = await context.RadiologyPatients.FindAsync(request.PatientID);
            if (patient == null)
                return BadRequest(new { error = "Invalid Patient ID.", patient_ID = request.PatientID });

            var radiologist = await context.Radiologists.FindAsync(request.RadiologistID);
            if (radiologist == null)
                return BadRequest(new { error = "Invalid Radiologist ID.", radiologist_ID = request.RadiologistID });

            var equipment = await context.Equipment.FindAsync(request.EquipmentID);
            if (equipment == null)
                return BadRequest(new { error = "Invalid Equipment ID.", equipment_ID = request.EquipmentID });

            var appointment = mapper.Map<ImagingAppointment>(request);
            appointment.Patient = patient;
            appointment.Radiologist = radiologist;
            appointment.Equipment = equipment;

            context.ImagingAppointments.Add(appointment);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetImagingAppointmentById),
                new { imagingId = appointment.ImagingID },
                mapper.Map<ImagingAppointmentResponse>(appointment));
        }

        // ← Added: dedicated retry endpoint for BackgroundSyncService
        // RadiologyRequestCreatedHandler queues { PatientID, Type, Datetime }
        // Update RadiologyRequestCreatedHandler queue route to "api/v1/radiology/imagingappointment/retry"
        [HttpPost("retry")]
        [AllowAnonymous]
        public async Task<IActionResult> RetryImagingAppointment([FromBody] ImagingAppointmentRetryPayload payload)
        {
            if (payload is null || payload.PatientID <= 0)
                return BadRequest(new { error = "Invalid retry payload." });

            var patient = await context.RadiologyPatients.FindAsync(payload.PatientID);
            if (patient == null)
                return BadRequest(new { error = "Invalid Patient ID.", patient_ID = payload.PatientID });

            var today = DateTime.UtcNow.Date;
            var modality = payload.Type ?? "X-Ray";

            var radiologistId = await context.Radiologists
                .OrderBy(r => context.ImagingAppointments
                    .Count(a => a.RadiologistID == r.RadiologistID
                             && a.Datetime >= today
                             && a.Type == modality))
                .Select(r => (int?)r.RadiologistID)
                .FirstOrDefaultAsync();

            var equipmentId = await context.Equipment
                .OrderBy(e => context.ImagingAppointments
                    .Count(a => a.EquipmentID == e.EquipmentID
                             && a.Datetime >= today
                             && a.Type == modality))
                .Select(e => (int?)e.EquipmentID)
                .FirstOrDefaultAsync();

            if (radiologistId == null || equipmentId == null)
            {
                logger.LogWarning("Retry failed — no available radiologist or equipment for modality {Modality}.", modality);
                return StatusCode(503, new { error = "No available radiologist or equipment — will retry." });
            }

            var appointment = new ImagingAppointment
            {
                PatientID = payload.PatientID,
                RadiologistID = radiologistId.Value,
                EquipmentID = equipmentId.Value,
                Datetime = payload.Datetime == default ? DateTime.UtcNow : payload.Datetime,
                Type = modality
            };

            context.ImagingAppointments.Add(appointment);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Retry created imaging appointment for Patient={PatientId}, Radiologist={RadiologistId}, Equipment={EquipmentId}, Modality={Modality}",
                payload.PatientID, radiologistId, equipmentId, modality);

            return Ok(new { message = "Imaging appointment created via retry.", imagingId = appointment.ImagingID });
        }

        [HttpPut("{imagingId}")]
        public async Task<IActionResult> UpdateImagingAppointment(int imagingId, [FromBody] ImagingAppointmentUpdateRequest request)
        {
            if (imagingId != request.ImagingID)
                return BadRequest(new { error = "Imaging appointment ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });

            var existingAppointment = await context.ImagingAppointments.FindAsync(imagingId);
            if (existingAppointment == null)
                return NotFound(new { error = "Imaging appointment not found.", imaging_ID = imagingId });

            var patient = await context.RadiologyPatients.FindAsync(request.PatientID);
            if (patient == null)
                return BadRequest(new { error = "Invalid Patient ID.", patient_ID = request.PatientID });

            var radiologist = await context.Radiologists.FindAsync(request.RadiologistID);
            if (radiologist == null)
                return BadRequest(new { error = "Invalid Radiologist ID.", radiologist_ID = request.RadiologistID });

            var equipment = await context.Equipment.FindAsync(request.EquipmentID);
            if (equipment == null)
                return BadRequest(new { error = "Invalid Equipment ID.", equipment_ID = request.EquipmentID });

            mapper.Map(request, existingAppointment);
            existingAppointment.Patient = patient;
            existingAppointment.Equipment = equipment;
            existingAppointment.Radiologist = radiologist;

            await context.SaveChangesAsync();

            return Ok(new { message = "Imaging appointment updated successfully.", imaging_appointment = mapper.Map<ImagingAppointmentResponse>(existingAppointment) });
        }
    }
}