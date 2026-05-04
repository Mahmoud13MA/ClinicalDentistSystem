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
    public class EquipmentController(AppDbContext context , IMapper mapper) : ControllerBase
    {
       
        [HttpGet]
        public async Task<IActionResult> GetAllEquipment()
        {
            var equipment = await context.Equipment.ProjectTo<EquipmentResponse>(mapper.ConfigurationProvider)
                .ToListAsync();

            if (!equipment.Any())
            {
                return NotFound(new { error = "No equipment found." });
            }


            return Ok(equipment);
        }

        
        [HttpGet("{equipmentId}")]
        public async Task<IActionResult> GetEquipmentById(int equipmentId)
        {
            var equipment = await context.Equipment
                .Include(e => e.ImagingAppointments)
                .FirstOrDefaultAsync(e => e.EquipmentID == equipmentId);

            if (equipment == null)
            {
                return NotFound(new { error = "Equipment not found.", equipment_ID = equipmentId });
            }

            var response = mapper.Map<EquipmentResponse>(equipment); 
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEquipment([FromBody] EquipmentCreateRequest request)
        {


            var equipment = mapper.Map<Equipment>(request);
            context.Equipment.Add(equipment);
            await context.SaveChangesAsync();

            var response = mapper.Map<EquipmentResponse>(equipment);
            return CreatedAtAction(nameof(GetEquipmentById), new { equipmentId = equipment.EquipmentID }, response);
        }

       
        [HttpPut("{equipmentId}")]
        public async Task<IActionResult> UpdateEquipment(int equipmentId, [FromBody] EquipmentUpdateRequest request)
        {
         
            if (equipmentId != request.EquipmentID)
            {
                return BadRequest(new { error = "Equipment ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
            }

            var existingEquipment = await context.Equipment.FindAsync(equipmentId);
            if (existingEquipment == null)
            {
                return NotFound(new { error = "Equipment not found.", equipment_ID = equipmentId });
            }

         
            mapper.Map(request, existingEquipment);
            await context.SaveChangesAsync();


            var response = mapper.Map<EquipmentBasicInfo>(existingEquipment);
            
            return Ok(new { message = "Equipment updated successfully.", equipment = response });
        }

     
        [HttpDelete("{equipmentId}")]
        public async Task<IActionResult> DeleteEquipment(int equipmentId)
        {
            var equipment = await context.Equipment.FindAsync(equipmentId);
            if (equipment == null)
            {
                return NotFound(new { error = "Equipment not found.", equipment_ID = equipmentId });
            }

            context.Equipment.Remove(equipment);
            await context.SaveChangesAsync();

            return Ok(new { message = "Equipment deleted successfully.", equipment_ID = equipmentId });
        }
    }
}
