using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Shared.Data;
using ClinicalDentistSystem.Shared.Contracts.Diagnostics;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace clinical.APIs.Modules.DentalClinic.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/dentalclinic/radiology")]
public class RadiologyController(IMediator mediator, AppDbContext context) : ControllerBase
{
    /// <summary>
    /// Doctor creates a radiology imaging request for a patient.
    /// Triggers automatic scheduling of an ImagingAppointment in the Radiology module.
    /// </summary>
    [HttpPost("service-requests")]
    public async Task<IActionResult> CreateServiceRequest(
        [FromBody] CreateRadiologyServiceRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue("sub"), out var doctorId))
            return Unauthorized(new { error = "Invalid or missing doctor identity in token." });

        var serviceRequest = new ServiceRequest
        {
            Status = RequestStatus.Active,
            Intent = RequestIntent.Order,
            Code = new CodeableConcept { Text = dto.ImagingType },
            Subject = new ResourceReference($"Patient/{dto.PatientId}"),
            Requester = new ResourceReference($"Practitioner/{doctorId}")
        };

        try
        {
            var createdRequest = await mediator.Send(
                new CreateServiceRequestCommand(serviceRequest),
                cancellationToken);

            return Ok(new
            {
                message = "Radiology service request created. An imaging appointment will be scheduled automatically.",
                serviceRequestId = createdRequest.Id,
                patientId = dto.PatientId,
                imagingType = dto.ImagingType
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Doctor checks if their imaging request has been scheduled.
    /// </summary>
    [HttpGet("appointments/patient/{patientId}")]
    public async Task<IActionResult> GetScheduledAppointments(int patientId, CancellationToken cancellationToken)
    {
        var appointments = await context.ImagingAppointmentMetadata
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.ScheduledAt)
            .Select(x => new
            {
                x.ImagingId,
                x.PatientId,
                x.Modality,
                x.ScheduledAt,
                x.Status
            })
            .ToListAsync(cancellationToken);

        return Ok(appointments);
    }

    /// <summary>
    /// Doctor checks status of a specific imaging appointment.
    /// </summary>
    [HttpGet("appointments/{imagingId}")]
    public async Task<IActionResult> GetAppointmentStatus(int imagingId, CancellationToken cancellationToken)
    {
        var appointment = await context.ImagingAppointmentMetadata
            .FirstOrDefaultAsync(x => x.ImagingId == imagingId, cancellationToken);

        if (appointment == null)
            return NotFound(new { error = "Imaging appointment not found or not yet scheduled.", imagingId });

        return Ok(new
        {
            appointment.ImagingId,
            appointment.PatientId,
            appointment.Modality,
            appointment.ScheduledAt,
            appointment.Status
        });
    }
}