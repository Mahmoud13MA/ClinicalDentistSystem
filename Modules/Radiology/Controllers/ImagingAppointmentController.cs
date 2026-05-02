using AutoMapper;
using AutoMapper.QueryableExtensions;
using clinical.APIs.Modules.Radiology.DTOs;
using clinical.APIs.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radiology.Models;

namespace clinical.APIs.Modules.Radiology.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/radiology/[controller]")]
    public class ImagingAppointmentController(AppDbContext context , IMapper mapper) : ControllerBase
    {
     
        [HttpGet]
        public async Task<IActionResult> GetAllImagingAppointments()
        {
            var appointments = await context.ImagingAppointments
                .ProjectTo<ImagingAppointmentResponse>(mapper.ConfigurationProvider)
                .ToListAsync();

            if (appointments == null || appointments.Count == 0)
            {
                return NotFound(new { error = "No imaging appointments found." });
            }

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
            {
                return NotFound(new { error = "Imaging appointment not found.", imaging_ID = imagingId });
            }

            var response = mapper.Map<ImagingAppointmentResponse>(appointment);
            return Ok(response);
        }

        [HttpGet("bypatient/{patientId}")]
        public async Task<IActionResult> GetImagingAppointmentsByPatient(int patientId)
        {

            var appointments = await context.ImagingAppointments
                    .Where(ia => ia.PatientID == patientId)
                    .ProjectTo<ImagingAppointmentResponse>(mapper.ConfigurationProvider)
                    .ToListAsync();


            if (appointments == null || appointments.Count == 0)
            {
                return NotFound(new { error = "No imaging appointments found for this patient.", patient_ID = patientId });
            }

            return Ok(appointments);
        }

   
        [HttpGet("byradiologist/{radiologistId}")]
        public async Task<IActionResult> GetImagingAppointmentsByRadiologist(int radiologistId)
        {
            var appointments = await context.ImagingAppointments
                .Where(ia => ia.RadiologistID == radiologistId)
                .ProjectTo<ImagingAppointmentResponse>(mapper.ConfigurationProvider)
                .ToListAsync();


            if (appointments == null || appointments.Count == 0)
            {
                return NotFound(new { error = "No imaging appointments found for this radiologist.", radiologist_ID = radiologistId });
            }

            return Ok(appointments);
        }

        [HttpGet("byequipment/{equipmentId}")]
        public async Task<IActionResult> GetImagingAppointmentsByEquipment(int equipmentId)
        {
            var appointments = await context.ImagingAppointments
                .Where(ia => ia.EquipmentID == equipmentId)
                .ProjectTo<ImagingAppointmentResponse>(mapper.ConfigurationProvider)
                .ToListAsync();


            if (appointments == null || appointments.Count == 0)
            {
                return NotFound(new { error = "No imaging appointments found for this equipment.", equipment_ID = equipmentId });
            }

            return Ok(appointments);
        }

     
        [HttpPost]
        public async Task<IActionResult> CreateImagingAppointment([FromBody] ImagingAppointmentCreateRequest request)
        {

            var patient = await context.RadiologyPatients.FindAsync(request.PatientID);
            if (patient == null)
            {
                return BadRequest(new { error = "Invalid Patient ID.", patient_ID = request.PatientID });
            }

            var radiologist = await context.Radiologists.FindAsync(request.RadiologistID);
            if (radiologist == null )
            {
                return BadRequest(new { error = "Invalid Radiologist ID.", radiologist_ID = request.RadiologistID });
            }

            var equipment = await context.Equipment.FindAsync(request.EquipmentID);
            if (equipment == null)
            {
                return BadRequest(new { error = "Invalid Equipment ID.", equipment_ID = request.EquipmentID });
            }

            var appointment = mapper.Map<ImagingAppointment>(request);



            appointment.Patient = patient;
            appointment.Radiologist = radiologist;
            appointment.Equipment = equipment; 
           

            context.ImagingAppointments.Add(appointment);
            await context.SaveChangesAsync();
            var response = mapper.Map<ImagingAppointmentResponse>(appointment);
            return CreatedAtAction(nameof(GetImagingAppointmentById), new { imagingId = appointment.ImagingID }, response);
        }

      
        [HttpPut("{imagingId}")]
        public async Task<IActionResult> UpdateImagingAppointment(int imagingId, [FromBody] ImagingAppointmentUpdateRequest request)
        {
          

            if (imagingId != request.ImagingID)
            {
                return BadRequest(new { error = "Imaging appointment ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
            }

            var existingAppointment = await context.ImagingAppointments.FindAsync(imagingId);
            if (existingAppointment == null)
            {
                return NotFound(new { error = "Imaging appointment not found.", imaging_ID = imagingId });
            }

            // Validate foreign keys

            var patient = await context.RadiologyPatients.FindAsync(request.PatientID);
            if (patient == null)
            {
                return BadRequest(new { error = "Invalid Patient ID.", patient_ID = request.PatientID });
            }

            var radiologist = await context.Radiologists.FindAsync(request.RadiologistID);
            if (radiologist == null)
            {
                return BadRequest(new { error = "Invalid Radiologist ID.", radiologist_ID = request.RadiologistID });
            }

            var equipment = await context.Equipment.FindAsync(request.EquipmentID);
            if (equipment == null)
            {
                return BadRequest(new { error = "Invalid Equipment ID.", equipment_ID = request.EquipmentID });
            }

            // Update appointment properties

            mapper.Map(request, existingAppointment);
          
            existingAppointment.Patient = patient;
            existingAppointment.Equipment= equipment;
            existingAppointment.Radiologist= radiologist;



            await context.SaveChangesAsync();
            var response = mapper.Map<ImagingAppointmentResponse>(existingAppointment);

            return Ok(new { message = "Imaging appointment updated successfully.", imaging_appointment = response });
        }

       
     
    }
}
