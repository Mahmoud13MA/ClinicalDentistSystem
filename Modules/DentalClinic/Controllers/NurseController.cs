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
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class NurseController(AppDbContext context, INurseMappingService mappingService, IPasswordHashService passwordHashService, IEmailValidationService emailValidationService, IProfileManagementService profileManagementService): ControllerBase
    {
   
       

        // GET: /Nurse
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetNurses()
        {
            var nurses = await context.Nurses.ToListAsync();

            if (nurses == null || nurses.Count == 0)
            {
                return NotFound(new { message = "No nurses found." });
            }

            var response = mappingService.MapToResponseList(nurses);
            return Ok(response);
        }

        // GET: /Nurse/{id}
        [HttpGet("{NURSE_ID}")]
        public async Task<IActionResult> GetNurseById(int NURSE_ID)
        {
            var nurse = await context.Nurses.FirstOrDefaultAsync(n => n.NURSE_ID == NURSE_ID);

            if (nurse == null)
            {
                return NotFound(new { error = "Nurse not found.", nurse_ID = NURSE_ID });
            }

            var response = mappingService.MapToResponse(nurse);
            return Ok(response);
        }

        // POST: /Nurse
        [Authorize(Policy = "DoctorOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateNurse([FromBody] NurseCreateRequest request)
        {
         

             
                // Check if email already exists
                var emailExists = await emailValidationService.IsEmailUsedAsync(request.Email);
                if (emailExists)
                {
                    return BadRequest(new { error = "Email already registered." });
                }

                var nurse = new Nurse
                {
                    Name = request.Name,
                    Phone = request.Phone,
                    Email = request.Email.Trim().ToLowerInvariant(),
                    PasswordHash = passwordHashService.HashPassword(request.Password)
                };

                context.Nurses.Add(nurse);
                await context.SaveChangesAsync();

                var response = mappingService.MapToResponse(nurse);
                return CreatedAtAction(nameof(GetNurseById), new { NURSE_ID = nurse.NURSE_ID }, response);
            }
           
        

        // PUT: /Nurse/{id}
        [Authorize(Policy = "DoctorOnly")]
        [HttpPut("{NURSE_ID}")]
        public async Task<IActionResult> UpdateNurse(int NURSE_ID, [FromBody] UpdateStaffInfoRequest request)
        {
                var result = await profileManagementService.UpdateNurseInfoAsync(NURSE_ID, request);

                if (!result.IsSuccess)
                {
                    if (result.ErrorMessage == "Nurse not found")
                        return NotFound(new { error = result.ErrorMessage, nurse_ID = NURSE_ID });

                    return BadRequest(new { error = result.ErrorMessage });
                }

                // Fetch updated nurse to return
                var updatedNurse = await context.Nurses.FindAsync(NURSE_ID);
                var response = mappingService.MapToResponse(updatedNurse);

                return Ok(new { message = "Nurse updated successfully.", nurse = response });
            }
            
            
        

        // DELETE: /Nurse/{id}
        [Authorize(Policy ="DoctorOnly")]
        [HttpDelete("{NURSE_ID}")]
        public async Task<IActionResult> DeleteNurse(int NURSE_ID)
        {
         
                var nurse = await context.Nurses.FindAsync(NURSE_ID);
                if (nurse == null)
                {
                    return NotFound(new { error = "Nurse not found.", nurse_ID = NURSE_ID });
                }

                context.Nurses.Remove(nurse);
                await context.SaveChangesAsync();

                return Ok(new { message = "Nurse deleted successfully.", nurse_ID = NURSE_ID });
            
          
        }
    }
}
