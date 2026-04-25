using clinical.APIs.Modules.Radiology.DTOs;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radiology.Models;

namespace clinical.APIs.Modules.Radiology.Controllers
{
    [Authorize(Policy = "DoctorOnly")]
    [ApiController]
    [Route("[controller]")]
    public class RadiologistController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHashService _passwordHashService;

        public RadiologistController(AppDbContext context, IPasswordHashService passwordHashService)
        {
            _context = context;
            _passwordHashService = passwordHashService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetRadiologists()
        {
            try
            {
                var radiologists = await _context.Radiologists.ToListAsync();
                if (radiologists == null || radiologists.Count == 0)
                {
                    return NotFound(new { error = "No radiologists found." });
                }

                var response = radiologists.Select(r => new RadiologistResponse
                {
                    RadiologistID = r.RadiologistID,
                    Name = r.Name,
                    Phone = r.Phone,
                    Email = r.Email,
                    Specialty = r.Specialty
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("{RadiologistID}")]
        public async Task<IActionResult> GetRadiologistById(int RadiologistID)
        {
            try
            {
                var radiologist = await _context.Radiologists.FirstOrDefaultAsync(r => r.RadiologistID == RadiologistID);
                if (radiologist == null)
                {
                    return NotFound(new { error = "Radiologist not found.", radiologist_ID = RadiologistID });
                }

                var response = new RadiologistResponse
                {
                    RadiologistID = radiologist.RadiologistID,
                    Name = radiologist.Name,
                    Phone = radiologist.Phone,
                    Email = radiologist.Email,
                    Specialty = radiologist.Specialty
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateRadiologist([FromBody] RadiologistCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Radiologist data is required.", hint = "Make sure you're sending a valid JSON body with radiologist information." });
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
                    hint = "Required fields: Name, Phone, Email, Specialty, Password (minimum 8 characters)"
                });
            }

            try
            {
                // Check if email already exists
                var emailExists = await _context.Radiologists.AnyAsync(r => r.Email == request.Email);
                if (emailExists)
                {
                    return BadRequest(new { error = "Email already registered." });
                }

                var radiologist = new Radiologist
                {
                    Name = request.Name,
                    Phone = request.Phone,
                    Email = request.Email,
                    Specialty = request.Specialty,
                    PasswordHash = _passwordHashService.HashPassword(request.Password)
                };

                _context.Radiologists.Add(radiologist);
                await _context.SaveChangesAsync();

                var response = new RadiologistResponse
                {
                    RadiologistID = radiologist.RadiologistID,
                    Name = radiologist.Name,
                    Phone = radiologist.Phone,
                    Email = radiologist.Email,
                    Specialty = radiologist.Specialty
                };

                return CreatedAtAction(nameof(GetRadiologistById), new { RadiologistID = radiologist.RadiologistID }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("{RadiologistID}")]
        public async Task<IActionResult> UpdateRadiologist(int RadiologistID, [FromBody] RadiologistUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Radiologist data is required." });
            }

            if (RadiologistID != request.RadiologistID)
            {
                return BadRequest(new { error = "Radiologist ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
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
                var existingRadiologist = await _context.Radiologists.FindAsync(RadiologistID);
                if (existingRadiologist == null)
                {
                    return NotFound(new { error = "Radiologist not found.", radiologist_ID = RadiologistID });
                }

                // Check if email is being changed and if new email already exists
                if (existingRadiologist.Email != request.Email)
                {
                    var emailExists = await _context.Radiologists
                        .AnyAsync(r => r.Email == request.Email && r.RadiologistID != RadiologistID);
                    if (emailExists)
                    {
                        return BadRequest(new { error = "Email already in use by another radiologist." });
                    }
                }

                // Update radiologist properties
                existingRadiologist.Name = request.Name;
                existingRadiologist.Phone = request.Phone;
                existingRadiologist.Email = request.Email;
                existingRadiologist.Specialty = request.Specialty;

                _context.Radiologists.Update(existingRadiologist);
                await _context.SaveChangesAsync();

                var response = new RadiologistResponse
                {
                    RadiologistID = existingRadiologist.RadiologistID,
                    Name = existingRadiologist.Name,
                    Phone = existingRadiologist.Phone,
                    Email = existingRadiologist.Email,
                    Specialty = existingRadiologist.Specialty
                };

                return Ok(new { message = "Radiologist updated successfully.", radiologist = response });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Radiologists.AnyAsync(r => r.RadiologistID == RadiologistID))
                {
                    return NotFound(new { error = "Radiologist not found during update.", radiologist_ID = RadiologistID });
                }
                throw;
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }

        [HttpDelete("{RadiologistID}")]
        public async Task<IActionResult> DeleteRadiologist(int RadiologistID)
        {
            try
            {
                var radiologist = await _context.Radiologists.FindAsync(RadiologistID);
                if (radiologist == null)
                {
                    return NotFound(new { error = "Radiologist not found.", radiologist_ID = RadiologistID });
                }

                _context.Radiologists.Remove(radiologist);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Radiologist deleted successfully.", radiologist_ID = RadiologistID });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
}
