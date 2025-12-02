using Microsoft.AspNetCore.Mvc;
using clinical.APIs.Data;
using clinical.APIs.Models;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppointmentController : Controller
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Appointment
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Nurse)
                .ToListAsync();

            if (appointments == null || appointments.Count == 0)
            {
                return NotFound(new { message = "No appointments found." });
            }

            return Ok(appointments);
        }

        // GET: /Appointment/{id}
        [HttpGet("{Appointment_ID}")]
        public async Task<IActionResult> GetAppointmentById(int Appointment_ID)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Nurse)
                .FirstOrDefaultAsync(a => a.Appointment_ID == Appointment_ID);

            if (appointment == null)
            {
                return NotFound(new { error = "Appointment not found.", appointment_ID = Appointment_ID });
            }

            return Ok(appointment);
        }

        // POST: /Appointment
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
        {
            if (appointment == null)
            {
                return BadRequest(new { error = "Appointment data is required.", hint = "Make sure you're sending a valid JSON body with appointment information." });
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
                    hint = "Required fields: Date (format: YYYY-MM-DD), Time (format: HH:mm:ss), Ref_Num, Type, Patient_ID, Doctor_ID, Nurse_ID"
                });
            }

            try
            {
                // Validate that Patient exists
                var patientExists = await _context.Patients.AnyAsync(p => p.Patient_ID == appointment.Patient_ID);
                if (!patientExists)
                {
                    return BadRequest(new { error = "Invalid Patient_ID. Patient does not exist.", patient_ID = appointment.Patient_ID });
                }

                // Validate that Doctor exists
                var doctorExists = await _context.Doctors.AnyAsync(d => d.ID == appointment.Doctor_ID);
                if (!doctorExists)
                {
                    return BadRequest(new { error = "Invalid Doctor_ID. Doctor does not exist.", doctor_ID = appointment.Doctor_ID });
                }

                // Validate that Nurse exists
                var nurseExists = await _context.Nurses.AnyAsync(n => n.NURSE_ID == appointment.Nurse_ID);
                if (!nurseExists)
                {
                    return BadRequest(new { error = "Invalid Nurse_ID. Nurse does not exist.", nurse_ID = appointment.Nurse_ID });
                }

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAppointmentById), new { Appointment_ID = appointment.Appointment_ID }, appointment);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }

        // PUT: /Appointment/{id}
        [HttpPut("{Appointment_ID}")]
        public async Task<IActionResult> UpdateAppointment(int Appointment_ID, [FromBody] Appointment appointment)
        {
            if (appointment == null)
            {
                return BadRequest(new { error = "Appointment data is required." });
            }

            if (Appointment_ID != appointment.Appointment_ID)
            {
                return BadRequest(new { error = "Appointment ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
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
                // Check if appointment exists
                var existingAppointment = await _context.Appointments.FindAsync(Appointment_ID);
                if (existingAppointment == null)
                {
                    return NotFound(new { error = "Appointment not found.", appointment_ID = Appointment_ID });
                }

                // Validate that Patient exists
                var patientExists = await _context.Patients.AnyAsync(p => p.Patient_ID == appointment.Patient_ID);
                if (!patientExists)
                {
                    return BadRequest(new { error = "Invalid Patient_ID. Patient does not exist.", patient_ID = appointment.Patient_ID });
                }

                // Validate that Doctor exists
                var doctorExists = await _context.Doctors.AnyAsync(d => d.ID == appointment.Doctor_ID);
                if (!doctorExists)
                {
                    return BadRequest(new { error = "Invalid Doctor_ID. Doctor does not exist.", doctor_ID = appointment.Doctor_ID });
                }

                // Validate that Nurse exists
                var nurseExists = await _context.Nurses.AnyAsync(n => n.NURSE_ID == appointment.Nurse_ID);
                if (!nurseExists)
                {
                    return BadRequest(new { error = "Invalid Nurse_ID. Nurse does not exist.", nurse_ID = appointment.Nurse_ID });
                }

                // Update appointment properties
                existingAppointment.Date = appointment.Date;
                existingAppointment.Time = appointment.Time;
                existingAppointment.Ref_Num = appointment.Ref_Num;
                existingAppointment.Type = appointment.Type;
                existingAppointment.Patient_ID = appointment.Patient_ID;
                existingAppointment.Doctor_ID = appointment.Doctor_ID;
                existingAppointment.Nurse_ID = appointment.Nurse_ID;

                _context.Appointments.Update(existingAppointment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Appointment updated successfully.", appointment = existingAppointment });
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if appointment still exists
                if (!await _context.Appointments.AnyAsync(a => a.Appointment_ID == Appointment_ID))
                {
                    return NotFound(new { error = "Appointment not found during update.", appointment_ID = Appointment_ID });
                }
                throw;
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }

        // DELETE: /Appointment/{id}
        [HttpDelete("{Appointment_ID}")]
        public async Task<IActionResult> DeleteAppointment(int Appointment_ID)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(Appointment_ID);
                if (appointment == null)
                {
                    return NotFound(new { error = "Appointment not found.", appointment_ID = Appointment_ID });
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Appointment deleted successfully.", appointment_ID = Appointment_ID });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }
    }
}
