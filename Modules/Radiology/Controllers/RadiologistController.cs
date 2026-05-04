using AutoMapper;
using AutoMapper.QueryableExtensions;
using clinical.APIs.Modules.Radiology.DTOs;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
using clinical.APIs.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radiology.Models;

namespace clinical.APIs.Modules.Radiology.Controllers
{
    [Authorize(Policy = "RadiologistOrAdmin")]
    [ApiController]
    [Route("api/v1/radiology/[controller]")]
    public class RadiologistController(AppDbContext context , IPasswordHashService passwordHashService, IEmailValidationService emailValidationService , IMapper mapper) : ControllerBase
    {
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetRadiologists()
        {
            var radiologists = await context.Radiologists.ToListAsync();
            if (radiologists == null || radiologists.Count == 0)
            {
                return NotFound(new { error = "No radiologists found." });
            }

            var response = mapper.Map<RadiologistResponse>(radiologists);

            return Ok(response);
        }

        [HttpGet("{RadiologistID}")]
        public async Task<IActionResult> GetRadiologistById(int RadiologistID)
        {
            var radiologist = await context.Radiologists.FirstOrDefaultAsync(r => r.RadiologistID == RadiologistID);
            if (radiologist == null)
            {
                return NotFound(new { error = "Radiologist not found.", radiologist_ID = RadiologistID });
            }

            var response = mapper.Map<RadiologistResponse>(radiologist);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRadiologist([FromBody] RadiologistCreateRequest request)
        {
          

            var isEmailUsed = await emailValidationService.IsEmailUsedAsync(request.Email);
            if (isEmailUsed)
            {
                return BadRequest(new { error = "Email already registered." });
            }

            var radiologist = mapper.Map<Radiologist>(request);
            

            context.Radiologists.Add(radiologist);
            await context.SaveChangesAsync();

            var response = mapper.Map<RadiologistResponse>(radiologist);

            radiologist.PasswordHash = passwordHashService.HashPassword(request.Password);

            radiologist.Email = request.Email.Trim().ToLowerInvariant();


            return CreatedAtAction(nameof(GetRadiologistById), new { RadiologistID = radiologist.RadiologistID }, response);
        }

        [HttpPut("{RadiologistID}")]
        public async Task<IActionResult> UpdateRadiologist(int RadiologistID, [FromBody] RadiologistUpdateRequest request)
        {
            if (RadiologistID != request.RadiologistID)
                return BadRequest(new { error = "Radiologist ID mismatch." });

            var existingRadiologist = await context.Radiologists.FindAsync(RadiologistID);
            if (existingRadiologist == null)
                return NotFound(new { error = "Radiologist not found.", radiologist_ID = RadiologistID });

            var isEmailUsed = await emailValidationService.IsEmailUsedAsync(request.Email, radiologistId:RadiologistID);
         
            if (isEmailUsed)
                return BadRequest(new { error = "Email already used by another radiologist." });

            mapper.Map(request, existingRadiologist);
            await context.SaveChangesAsync();

            var response = mapper.Map<RadiologistResponse>(existingRadiologist);
            return Ok(new { message = "Radiologist updated successfully.", radiologist = response });
        }

        [HttpDelete("{RadiologistID}")]
        public async Task<IActionResult> DeleteRadiologist(int RadiologistID)
        {
            var radiologist = await context.Radiologists.FindAsync(RadiologistID);
            if (radiologist == null)
            {
                return NotFound(new { error = "Radiologist not found.", radiologist_ID = RadiologistID });
            }

            context.Radiologists.Remove(radiologist);
            await context.SaveChangesAsync();

            return Ok(new { message = "Radiologist deleted successfully.", radiologist_ID = RadiologistID });
        }
    }
}
