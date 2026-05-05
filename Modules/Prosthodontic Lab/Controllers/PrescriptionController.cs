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
    public class PrescriptionController(AppDbContext context, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetPrescriptions()
        {
            var prescriptions = await context.Prescriptions.ProjectTo<PrescriptionResponse>(mapper.ConfigurationProvider).ToListAsync();
            
            if (prescriptions.Count == 0)
            {
                return NotFound(new { error = "No prescriptions found." });
            }

            return Ok(prescriptions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPrescription(int id)
        {
            var prescription = await context.Prescriptions.FindAsync(id);

            if (prescription == null)
            {
                return NotFound(new { error = "Prescription not found.", prescriptionID = id });
            }

            return Ok(mapper.Map<PrescriptionResponse>(prescription));
        }

        [HttpPost]
        public async Task<IActionResult> CreatePrescription([FromBody] PrescriptionCreateRequest request)
        {
            var prescription = mapper.Map<Prescription>(request);

            context.Prescriptions.Add(prescription);
            await context.SaveChangesAsync();

            var response = mapper.Map<PrescriptionResponse>(prescription);

            return CreatedAtAction(nameof(GetPrescription), new { id = prescription.PrescriptionID }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrescription(int id, [FromBody] PrescriptionUpdateRequest request)
        {
            var prescription = await context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                return NotFound(new { error = "Prescription not found.", prescriptionID = id });
            }

            mapper.Map(request, prescription);

            await context.SaveChangesAsync();
            
            var response = mapper.Map<PrescriptionResponse>(prescription);

            return Ok(new { message = "Prescription updated successfully.", prescription = response });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var prescription = await context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                return NotFound(new { error = "Prescription not found.", prescriptionID = id });
            }

            context.Prescriptions.Remove(prescription);
            await context.SaveChangesAsync();

            return Ok(new { message = "Prescription deleted successfully.", prescriptionID = id });
        }
    }
}
