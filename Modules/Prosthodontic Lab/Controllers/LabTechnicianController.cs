using AutoMapper;
using AutoMapper.QueryableExtensions;
using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Modules.ProsthodonticLab.DTOs;
using clinical.APIs.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.ProsthodonticLab.Controllers
{
    [Authorize(Policy = "LabTechnician")]
    [ApiController]
    [Route("api/v1/prosthodonticlab/[controller]")]
    public class LabTechnicianController(AppDbContext context, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetLabTechnicians()
        {
            var technicians = await context.LabTechnicians.ProjectTo<LabTechnicianResponse>(mapper.ConfigurationProvider).ToListAsync();
            
            if (technicians.Count == 0)
            {
                return NotFound(new { error = "No lab technicians found." });
            }

            return Ok(technicians);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLabTechnician(int id)
        {
            var technician = await context.LabTechnicians.FindAsync(id);

            if (technician == null)
            {
                return NotFound(new { error = "Lab technician not found.", labTechnicianID = id });
            }

            return Ok(mapper.Map<LabTechnicianResponse>(technician));
        }

        [HttpPost]
        public async Task<IActionResult> CreateLabTechnician([FromBody] LabTechnicianCreateRequest request)
        {
            var technician = mapper.Map<LabTechnician>(request);

            context.LabTechnicians.Add(technician);
            await context.SaveChangesAsync();

            var response = mapper.Map<LabTechnicianResponse>(technician);

            return CreatedAtAction(nameof(GetLabTechnician), new { id = technician.LabTechnicianID }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLabTechnician(int id, [FromBody] LabTechnicianUpdateRequest request)
        {
            var technician = await context.LabTechnicians.FindAsync(id);
            if (technician == null)
            {
                return NotFound(new { error = "Lab technician not found.", labTechnicianID = id });
            }

            mapper.Map(request, technician);

            await context.SaveChangesAsync();
            
            var response = mapper.Map<LabTechnicianResponse>(technician);

            return Ok(new { message = "Lab technician updated successfully.", technician = response });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLabTechnician(int id)
        {
            var technician = await context.LabTechnicians.FindAsync(id);
            if (technician == null)
            {
                return NotFound(new { error = "Lab technician not found.", labTechnicianID = id });
            }

            context.LabTechnicians.Remove(technician);
            await context.SaveChangesAsync();

            return Ok(new { message = "Lab technician deleted successfully.", labTechnicianID = id });
        }
    }
}
