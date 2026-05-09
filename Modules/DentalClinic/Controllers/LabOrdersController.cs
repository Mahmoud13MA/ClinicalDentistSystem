using clinical.APIs.Modules.DentalClinic.DTOs;
using ClinicalDentistSystem.Shared.Contracts.Lab;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace clinical.APIs.Modules.DentalClinic.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/dentalclinic/laborders")]
public class LabOrdersController(ILabModule labModule, IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateLabOrder(
       [FromBody] CreateLabOrderDto dto,
       CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue("sub"), out var doctorId))
            return Unauthorized(new { error = "Invalid or missing doctor identity in token." });

        var serviceRequest = new ServiceRequest
        {
            Status = RequestStatus.Active,
            Intent = RequestIntent.Order,
            Code = new CodeableConcept { Text = dto.ProcedureDescription },
            Subject = new ResourceReference($"Patient/{dto.PatientId}")
        };

        try
        {
            var createdRequest = await mediator.Send(
                new CreateLabOrderCommand(serviceRequest, doctorId),
                cancellationToken);

            return Ok(createdRequest);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetLabOrder(string orderId, CancellationToken cancellationToken)
    {
        var labOrder = await labModule.GetLabOrderAsync(orderId, cancellationToken);
        if (labOrder == null)
            return NotFound(new { error = "Lab order not found", orderId });

        return Ok(labOrder);
    }

    [HttpGet("{id}/results")]  
    public async Task<IActionResult> GetLabResults(string id, CancellationToken ct)
    {
        var report = await mediator.Send(new GetLabResultsQuery(id), ct);

        if (report is null)
            return NotFound(new { error = "Lab report not found", id });

        return Ok(report);
    }
}