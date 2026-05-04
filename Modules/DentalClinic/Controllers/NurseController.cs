using clinical.APIs.Modules.DentalClinic.Models;
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
    public class NurseController(AppDbContext context, INurseMappingService mappingService, IProfileManagementService profileManagementService) : ControllerBase
    {
        [Authorize(Policy = "NurseOnly")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNurseById(int id)
        {
            var loggedInUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                                    ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(loggedInUserIdString) || !int.TryParse(loggedInUserIdString, out int loggedInUserId) || loggedInUserId != id)
            {
                return Forbid();
            }

            var nurse = await context.Nurses.FindAsync(id);
            if (nurse == null)
            {
                return NotFound(new { error = "Nurse profile not found.", nurse_ID = id });
            }

            var response = mappingService.MapToResponse(nurse);
            return Ok(response);
        }

        [Authorize(Policy = "NurseOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNurse(int id, [FromBody] UpdateStaffInfoRequest request)
        {
            var loggedInUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                                    ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(loggedInUserIdString) || !int.TryParse(loggedInUserIdString, out int loggedInUserId) || loggedInUserId != id)
            {
                return Forbid();
            }

            var result = await profileManagementService.UpdateNurseInfoAsync(id, request);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage == "Nurse not found")
                    return NotFound(new { error = result.ErrorMessage, nurse_ID = id });

                return BadRequest(new { error = result.ErrorMessage });
            }

            var updatedNurse = await context.Nurses.FindAsync(id);
            var response = mappingService.MapToResponse(updatedNurse);

            return Ok(new { message = "Nurse updated successfully.", nurse = response });
        }

        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetNurses()
        {
            var nurses = await context.Nurses.ToListAsync();

            if (nurses.Count == 0)
            {
                return NotFound(new { message = "No nurses found." });
            }

            var response = mappingService.MapToResponseList(nurses);
            return Ok(response);
        }
    }
}
