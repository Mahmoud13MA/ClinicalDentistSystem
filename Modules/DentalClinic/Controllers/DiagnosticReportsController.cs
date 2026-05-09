using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Models;
using ClinicalDentistSystem.Shared.Contracts.Diagnostics;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.DentalClinic.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/dentalclinic/diagnosticreports")]
public class DiagnosticReportsController(AppDbContext context, IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateServiceRequest([FromBody] ServiceRequest request, CancellationToken cancellationToken)
    {
       
        var createdRequest = await mediator.Send(new CreateServiceRequestCommand(request), cancellationToken);
        return Ok(createdRequest);
    }

    [HttpPost("metadata")]
    public async Task<IActionResult> CreateMetadata([FromBody] DiagnosticReportMetadata metadata)
    {
        context.DiagnosticReportMetadata.Add(metadata);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMetadataById), new { id = metadata.Id }, metadata);
    }

    [HttpGet("metadata/{id:int}")]
    public async Task<IActionResult> GetMetadataById(int id)
    {
        var metadata = await context.DiagnosticReportMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (metadata == null)
        {
            return NotFound(new { error = "Diagnostic report metadata not found", id });
        }

        return Ok(metadata);
    }
}
