using clinical.APIs.Data;
using clinical.APIs.Models;
using clinical.APIs.DTOs;
using clinical.APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Controllers
{
    [Authorize(Policy ="DoctorOnly")]
    [ApiController]
    [Route("[controller]")]
    public class DoctorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDoctorMappingService _mappingService;
        private readonly IPasswordHashService _passwordHashService;

        public DoctorController(AppDbContext context, IDoctorMappingService mappingService, IPasswordHashService passwordHashService)
        {
            _context = context;
            _mappingService = mappingService;
            _passwordHashService = passwordHashService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetDoctor()
        {
            var doctors = await _context.Doctors.ToListAsync();
            if (doctors == null || doctors.Count == 0)
            {
                return NotFound();
            }

            var response = _mappingService.MapToResponseList(doctors);
            return Ok(response);
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetDoctorById(int ID)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.ID == ID);
            if (doctor == null)
            {
                return NotFound();
            }

            var response = _mappingService.MapToResponse(doctor);
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
                var emailExists = await _context.Doctors.AnyAsync(d => d.Email == request.Email);
                if (emailExists)
                {
                    return BadRequest(new { error = "Email already registered." });
                }

                var doctor = new Doctor
                {
                    Name = request.Name,
                    Phone = request.Phone,
                    Email = request.Email,
                    PasswordHash = _passwordHashService.HashPassword(request.Password)
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                var response = _mappingService.MapToResponse(doctor);
                return CreatedAtAction(nameof(GetDoctorById), new { ID = doctor.ID }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("{ID}")]
        public async Task<IActionResult> UpdateDoctor(int ID, [FromBody] DoctorUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Doctor data is required." });
            }

            if (ID != request.ID)
            {
                return BadRequest(new { error = "Doctor ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
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
                // Check if doctor exists
                var existingDoctor = await _context.Doctors.FindAsync(ID);
                if (existingDoctor == null)
                {
                    return NotFound(new { error = "Doctor not found.", doctor_ID = ID });
                }

                // Check if email is being changed and if new email already exists
                if (existingDoctor.Email != request.Email)
                {
                    var emailExists = await _context.Doctors
                        .AnyAsync(d => d.Email == request.Email && d.ID != ID);
                    if (emailExists)
                    {
                        return BadRequest(new { error = "Email already in use by another doctor." });
                    }
                }

                // Update doctor properties
                existingDoctor.Name = request.Name;
                existingDoctor.Phone = request.Phone;
                existingDoctor.Email = request.Email;

                _context.Doctors.Update(existingDoctor);
                await _context.SaveChangesAsync();

                var response = _mappingService.MapToResponse(existingDoctor);
                return Ok(new { message = "Doctor updated successfully.", doctor = response });
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if doctor still exists
                if (!await _context.Doctors.AnyAsync(d => d.ID == ID))
                {
                    return NotFound(new { error = "Doctor not found during update.", doctor_ID = ID });
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
