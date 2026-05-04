using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinical.APIs.Shared.Data;
using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Services;
using System.Security.Claims;

namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/clinic/[controller]")]
    public class DoctorController(AppDbContext context, IDoctorMappingService mappingService, IProfileManagementService profileManagementService) : ControllerBase
    {
        [Authorize(Policy = "DoctorOnly")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctorById(int id)
        {
            // Specifically looking for NameIdentifier (which maps to "sub") or directly looking for "sub"
            var loggedInUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(loggedInUserIdString) || !int.TryParse(loggedInUserIdString, out int loggedInUserId) || loggedInUserId != id)
            {
                return Forbid();
            }

            var doctor = await context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound(new { error = "Doctor profile not found." });
            }

            var response = mappingService.MapToResponse(doctor);
            return Ok(response);
        }

        [Authorize(Policy = "DoctorOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] UpdateStaffInfoRequest request)
        {
            var loggedInUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(loggedInUserIdString) || !int.TryParse(loggedInUserIdString, out int loggedInUserId) || loggedInUserId != id)
            {
                return Forbid();
            }

            var result = await profileManagementService.UpdateDoctorInfoAsync(id, request);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage == "Doctor not found")
                    return NotFound(new { error = result.ErrorMessage, doctor_ID = id });

                return BadRequest(new { error = result.ErrorMessage });
            }

            var updatedDoctor = await context.Doctors.FindAsync(id);
            var response = mappingService.MapToResponse(updatedDoctor);

            return Ok(new { message = "Doctor updated successfully.", doctor = response });
        }

        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await context.Doctors.ToListAsync();
            
            if (doctors.Count == 0)
            {
                return NotFound(new { message = "No doctors found." });
            }

            var response = mappingService.MapToResponseList(doctors);
            return Ok(response);
        }
    }
}
