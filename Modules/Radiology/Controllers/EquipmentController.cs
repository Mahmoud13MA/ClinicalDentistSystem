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
    public class EquipmentController(AppDbContext context) : ControllerBase
    {
       
        [HttpGet]
        public async Task<IActionResult> GetAllEquipment()
        {
            var equipment = await context.Equipment.ToListAsync();
            if (equipment == null || equipment.Count == 0)
            {
                return NotFound(new { error = "No equipment found." });
            }

            var response = equipment.Select(e => new EquipmentResponse
            {
                EquipmentID = e.EquipmentID,
                Type = e.Type,
                Model = e.Model
            }).ToList();

            return Ok(response);
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

            var response = new EquipmentResponse
            {
                EquipmentID = equipment.EquipmentID,
                Type = equipment.Type,
                Model = equipment.Model,
                ImagingAppointments = equipment.ImagingAppointments?
                    .Select(ia => new ImagingAppointmentBasicInfo
                    {
                        ImagingID = ia.ImagingID,
                        Datetime = ia.Datetime,
                        Type = ia.Type
                    }).ToList()
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEquipment([FromBody] EquipmentCreateRequest request)
        {
           

            var equipment = new Equipment
            {
                Type = request.Type,
                Model = request.Model
            };

            context.Equipment.Add(equipment);
            await context.SaveChangesAsync();

            var response = new EquipmentResponse
            {
                EquipmentID = equipment.EquipmentID,
                Type = equipment.Type,
                Model = equipment.Model
            };

            return CreatedAtAction(nameof(GetEquipmentById), new { equipmentId = equipment.EquipmentID }, response);
        }

       
        [HttpPut("{equipmentId}")]
        [Authorize]
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

            existingEquipment.Type = request.Type;
            existingEquipment.Model = request.Model;

            await context.SaveChangesAsync();

            var response = new EquipmentResponse
            {
                EquipmentID = existingEquipment.EquipmentID,
                Type = existingEquipment.Type,
                Model = existingEquipment.Model
            };

            return Ok(new { message = "Equipment updated successfully.", equipment = response });
        }

     
        [HttpDelete("{equipmentId}")]
        [Authorize]
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
