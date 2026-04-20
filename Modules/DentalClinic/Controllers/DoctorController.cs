using clinical.APIs.Modules.DentalClinic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
using clinical.APIs.Shared.Services;
using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Services;

namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [Authorize(Policy ="DoctorOnly")]
    [ApiController]
    [Route("[controller]")]
    public class DoctorController(AppDbContext context,IDoctorMappingService mappingService , IEmailValidationService emailValidationService ,IProfileManagementService profileManagementService , IPasswordHashService passwordHashService) : ControllerBase
    {
       

    

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetDoctor()
        {
            var doctors = await context.Doctors.ToListAsync();
            if (doctors == null || doctors.Count == 0)
            {
                return NotFound();
            }

            var response = mappingService.MapToResponseList(doctors);
            return Ok(response);
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetDoctorById(int ID)
        {
            var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.ID == ID);
            if (doctor == null)
            {
                return NotFound();
            }

            var response = mappingService.MapToResponse(doctor);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctor([FromBody] DoctorCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Doctor data is required.", hint = "Make sure you're sending a valid JSON body with doctor information." });
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
                    hint = "Required fields: Name, Phone, Email, Password (minimum 8 characters)"
                });
            }

            try
            {
                // Check if email already exists
                var emailExists = await emailValidationService.IsEmailUsedAsync(request.Email);
                if (emailExists)
                {
                    return BadRequest(new { error = "Email already registered." });
                }

                var doctor = new Doctor
                {
                    Name = request.Name,
                    Phone = request.Phone,
                    Email = request.Email.Trim().ToLowerInvariant(),
                    PasswordHash = passwordHashService.HashPassword(request.Password)
                };

                context.Doctors.Add(doctor);
                await context.SaveChangesAsync();

                var response = mappingService.MapToResponse(doctor);
                return CreatedAtAction(nameof(GetDoctorById), new { ID = doctor.ID }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("{ID}")]
        public async Task<IActionResult> UpdateDoctor(int ID, [FromBody] UpdateStaffInfoRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Doctor data is required." });
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
                var result = await profileManagementService.UpdateDoctorInfoAsync(ID, request);

                if (!result.IsSuccess)
                {
                    if (result.ErrorMessage == "Doctor not found")
                        return NotFound(new { error = result.ErrorMessage, doctor_ID = ID });

                    return BadRequest(new { error = result.ErrorMessage });
                }

                // Fetch updated doctor to return
                var updatedDoctor = await context.Doctors.FindAsync(ID);
                var response = mappingService.MapToResponse(updatedDoctor);

                return Ok(new { message = "Doctor updated successfully.", doctor = response });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }
    }
}
