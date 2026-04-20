using clinical.APIs.Modules.Radiology.DTOs;
using clinical.APIs.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radiology.Models;

namespace clinical.APIs.Modules.Radiology.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EquipmentController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all radiology equipment
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllEquipment()
        {
            try
            {
                var equipment = await _context.Equipment.ToListAsync();
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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific equipment by ID
        /// </summary>
        [HttpGet("{equipmentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEquipmentById(int equipmentId)
        {
            try
            {
                var equipment = await _context.Equipment
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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Create new equipment
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEquipment([FromBody] EquipmentCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Equipment data is required.", hint = "Make sure you're sending a valid JSON body with equipment information." });
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
                    hint = "Required fields: Type, Model (max 100 characters each)"
                });
            }

            try
            {
                var equipment = new Equipment
                {
                    Type = request.Type,
                    Model = request.Model
                };

                _context.Equipment.Add(equipment);
                await _context.SaveChangesAsync();

                var response = new EquipmentResponse
                {
                    EquipmentID = equipment.EquipmentID,
                    Type = equipment.Type,
                    Model = equipment.Model
                };

                return CreatedAtAction(nameof(GetEquipmentById), new { equipmentId = equipment.EquipmentID }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Update existing equipment
        /// </summary>
        [HttpPut("{equipmentId}")]
        [Authorize]
        public async Task<IActionResult> UpdateEquipment(int equipmentId, [FromBody] EquipmentUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Equipment data is required." });
            }

            if (equipmentId != request.EquipmentID)
            {
                return BadRequest(new { error = "Equipment ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
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
                var existingEquipment = await _context.Equipment.FindAsync(equipmentId);
                if (existingEquipment == null)
                {
                    return NotFound(new { error = "Equipment not found.", equipment_ID = equipmentId });
                }

                // Update equipment properties
                existingEquipment.Type = request.Type;
                existingEquipment.Model = request.Model;

                _context.Equipment.Update(existingEquipment);
                await _context.SaveChangesAsync();

                var response = new EquipmentResponse
                {
                    EquipmentID = existingEquipment.EquipmentID,
                    Type = existingEquipment.Type,
                    Model = existingEquipment.Model
                };

                return Ok(new { message = "Equipment updated successfully.", equipment = response });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Equipment.AnyAsync(e => e.EquipmentID == equipmentId))
                {
                    return NotFound(new { error = "Equipment not found during update.", equipment_ID = equipmentId });
                }
                throw;
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", details = innerMessage });
            }
        }

        /// <summary>
        /// Delete equipment
        /// </summary>
        [HttpDelete("{equipmentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteEquipment(int equipmentId)
        {
            try
            {
                var equipment = await _context.Equipment.FindAsync(equipmentId);
                if (equipment == null)
                {
                    return NotFound(new { error = "Equipment not found.", equipment_ID = equipmentId });
                }

                _context.Equipment.Remove(equipment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Equipment deleted successfully.", equipment_ID = equipmentId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}
