using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Modules.DentalClinic.Services;
using clinical.APIs.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/clinic/[controller]")]
    public class AppointmentController(AppDbContext context , IAppointmentMappingService mappingService) : ControllerBase
    {
       

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Nurse)
                .ToListAsync();

            if (appointments == null || appointments.Count == 0)
            {
                return NotFound(new { message = "No appointments found." });
            }

            var response = mappingService.MapToResponseList(appointments);
            return Ok(response);
        }

        [HttpGet("{Appointment_ID}")]
        public async Task<IActionResult> GetAppointmentById(int Appointment_ID)
        {
            var appointment = await context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Nurse)
                .FirstOrDefaultAsync(a => a.Appointment_ID == Appointment_ID);

            
            if (appointment == null)
            {
                return NotFound(new { error = "Appointment not found.", appointment_ID = Appointment_ID });
            }

            var response = mappingService.MapToResponse(appointment);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentCreateRequest request)
        {
            // Fetch the actual objects and store them in variables (patient, doctor, nurse)
            var patient = await context.Patients.FindAsync(request.Patient_ID);
            if (patient == null)
            {
                return BadRequest(new { error = "Invalid Patient_ID. Patient does not exist.", patient_ID = request.Patient_ID });
            }

            var doctor = await context.Doctors.FindAsync(request.Doctor_ID);
            if (doctor == null)
            {
                return BadRequest(new { error = "Invalid Doctor_ID. Doctor does not exist.", doctor_ID = request.Doctor_ID });
            }

            var nurse = await context.Nurses.FindAsync(request.Nurse_ID);
            if (nurse == null)
            {
                return BadRequest(new { error = "Invalid Nurse_ID. Nurse does not exist.", nurse_ID = request.Nurse_ID });
            }

            string refNum = $"APT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            var appointment = new Appointment
            {
                Date = request.Date,
                Time = request.Time,
                Ref_Num = refNum,
                Type = request.Type,
                Patient_ID = request.Patient_ID,
                Doctor_ID = request.Doctor_ID,
                Nurse_ID = request.Nurse_ID,
                //fot the mapper service
                Patient = patient,
                Doctor = doctor,
                Nurse = nurse
            };

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            var response = mappingService.MapToResponse(appointment);
            return CreatedAtAction(nameof(GetAppointmentById), new { Appointment_ID = appointment.Appointment_ID }, response);
        }

        [HttpPut("{Appointment_ID}")]
        public async Task<IActionResult> UpdateAppointment(int Appointment_ID, [FromBody] AppointmentUpdateRequest request)
        {
            if (Appointment_ID != request.Appointment_ID)
            {
                return BadRequest(new { error = "Appointment ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
            }

            var existingAppointment = await context.Appointments.FindAsync(Appointment_ID);
            if (existingAppointment == null)
            {
                return NotFound(new { error = "Appointment not found.", appointment_ID = Appointment_ID });
            }

            var patient = await context.Patients.FindAsync(request.Patient_ID);
            if (patient == null)
            {
                return BadRequest(new { error = "Invalid Patient_ID. Patient does not exist.", patient_ID = request.Patient_ID });
            }

            var doctor = await context.Doctors.FindAsync(request.Doctor_ID);
            if (doctor == null)
            {
                return BadRequest(new { error = "Invalid Doctor_ID. Doctor does not exist.", doctor_ID = request.Doctor_ID });
            }

            var nurse = await context.Nurses.FindAsync(request.Nurse_ID);
            if (nurse == null)
            {
                return BadRequest(new { error = "Invalid Nurse_ID. Nurse does not exist.", nurse_ID = request.Nurse_ID });
            }

            existingAppointment.Date = request.Date;
            existingAppointment.Time = request.Time;
            existingAppointment.Type = request.Type;
            existingAppointment.Patient_ID = request.Patient_ID;
            existingAppointment.Doctor_ID = request.Doctor_ID;
            existingAppointment.Nurse_ID = request.Nurse_ID;
            
            existingAppointment.Patient = patient;
            existingAppointment.Doctor = doctor;
            existingAppointment.Nurse = nurse;

            await context.SaveChangesAsync();

            var response = mappingService.MapToResponse(existingAppointment);
            return Ok(new { message = "Appointment updated successfully.", appointment = response });
        }

        [HttpDelete("{Appointment_ID}")]
        public async Task<IActionResult> DeleteAppointment(int Appointment_ID)
        {
            var appointment = await context.Appointments.FindAsync(Appointment_ID);
            
            if (appointment == null)
            {
                return NotFound(new { error = "Appointment not found.", appointment_ID = Appointment_ID });
            }

            context.Appointments.Remove(appointment);
            await context.SaveChangesAsync();

            return Ok(new { message = "Appointment deleted successfully.", appointment_ID = Appointment_ID });
        }
    }
}
