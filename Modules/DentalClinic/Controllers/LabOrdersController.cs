using ClinicalDentistSystem.Shared.Contracts.Lab;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clinical.APIs.Modules.DentalClinic.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/dentalclinic/laborders")]
public class LabOrdersController(ILabModule labModule) : ControllerBase
{
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetLabOrder(string orderId, CancellationToken cancellationToken)
    {
        var labOrder = await labModule.GetLabOrderAsync(orderId, cancellationToken);
        if (labOrder == null)
        {
            return NotFound(new { error = "Lab order not found", orderId });
        }

        return Ok(labOrder);
    }
}
