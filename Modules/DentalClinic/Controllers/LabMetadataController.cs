using clinical.APIs.Modules.ProsthodonticLab.Models;
using clinical.APIs.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.DentalClinic.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/dentalclinic/lab")]
public class LabMetadataController(AppDbContext context) : ControllerBase
{
    [HttpPost("metadata")]
    public async Task<IActionResult> CreateMetadata([FromBody] DiagnosticReportMetadata metadata)
    {
        context.LabDiagnosticReportMetadata.Add(metadata);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMetadataById), new { id = metadata.Id }, metadata);
    }

    [HttpGet("metadata/{id:int}")]
    public async Task<IActionResult> GetMetadataById(int id)
    {
        var metadata = await context.LabDiagnosticReportMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (metadata == null)
        {
            return NotFound(new { error = "Lab metadata not found", id });
        }

        return Ok(metadata);
    }
}
