using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Models;
using ClinicalDentistSystem.Shared.Contracts.Radiology;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.DentalClinic.Controllers;

[Authorize(Policy = "DoctorOrAdmin")]
[ApiController]
[Route("api/v1/dentalclinic/radiologyreports")]
public class RadiologyReportsController(
    IRadiologyModule radiologyModule,
    AppDbContext context,
    ILogger<RadiologyReportsController> logger) : ControllerBase
{
    [HttpGet("{reportId}")]
    public async Task<IActionResult> GetRadiologyReport(string reportId, CancellationToken cancellationToken)
    {
        var report = await radiologyModule.GetReportAsync(reportId, cancellationToken);
        if (report == null)
        {
            logger.LogWarning("GetRadiologyReport: Report not found for ReportId {ReportId}.", reportId);
            return NotFound(new { error = "Radiology report not found", reportId });
        }

        logger.LogInformation("GetRadiologyReport: Returning report for ReportId {ReportId}.", reportId);
        return Ok(report);
    }

    // 🔴 Fix: retry endpoint for BackgroundSyncService fallback queue
    [HttpPost("metadata/retry")]
    public async Task<IActionResult> RetryDiagnosticReportMetadata(
        [FromBody] DiagnosticReportRetryPayload payload,
        CancellationToken cancellationToken)
    {
        if (payload == null || string.IsNullOrWhiteSpace(payload.ReportId))
        {
            logger.LogWarning("RetryDiagnosticReportMetadata: Received invalid payload.");
            return BadRequest(new { error = "ReportId is required." });
        }

        var exists = await context.DiagnosticReportMetadata
            .AnyAsync(x => x.ReportId == payload.ReportId, cancellationToken);

        if (exists)
        {
            logger.LogWarning(
                "RetryDiagnosticReportMetadata: ReportId {ReportId} already exists. Skipping duplicate.",
                payload.ReportId);
            return Ok();
        }

        var metadata = new DiagnosticReportMetadata
        {
            ReportId = payload.ReportId,
            ReportDate = payload.ReportDate,
            Title = payload.Title,
            Status = payload.Status
        };

        context.DiagnosticReportMetadata.Add(metadata);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "RetryDiagnosticReportMetadata: Saved metadata for ReportId {ReportId}.",
            payload.ReportId);

        return Ok();
    }
}