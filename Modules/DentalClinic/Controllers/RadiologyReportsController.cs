using ClinicalDentistSystem.Shared.Contracts.Radiology;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clinical.APIs.Modules.DentalClinic.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/dentalclinic/radiologyreports")]
public class RadiologyReportsController(IRadiologyModule radiologyModule) : ControllerBase
{
    [HttpGet("{reportId}")]
    public async Task<IActionResult> GetRadiologyReport(string reportId, CancellationToken cancellationToken)
    {
        var report = await radiologyModule.GetReportAsync(reportId, cancellationToken);
        if (report == null)
        {
            return NotFound(new { error = "Radiology report not found", reportId });
        }

        return Ok(report);
    }
}
