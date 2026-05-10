using AutoMapper;
using AutoMapper.QueryableExtensions;
using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Modules.ProsthodonticLab.DTOs;
using clinical.APIs.Modules.ProsthodonticLab.Handlers;
using clinical.APIs.Modules.ProsthodonticLab.Services;
using clinical.APIs.Shared.Data;
using ClinicalDentistSystem.Shared.Contracts.Lab;
using ClinicalDentistSystem.Shared.Services;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.ProsthodonticLab.Controllers
{
    [Authorize(Policy = "LabTechnician")]
    [ApiController]
    [Route("api/v1/prosthodonticlab/[controller]")]
    public class OrderController(
        AppDbContext context,
        IMapper mapper,
        IMediator mediator,
        ILabFhirMappingService mappingService,
        IFhirValidationService validationService,
        ILogger<OrderController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await context.Orders
                .ProjectTo<OrderResponse>(mapper.ConfigurationProvider)
                .ToListAsync();

            // ← Fix: empty collection is Ok([]), not NotFound
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await context.Orders.FindAsync(id);

            if (order == null)
                return NotFound(new { error = "Order not found.", orderID = id });

            return Ok(mapper.Map<OrderResponse>(order));
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
        {
            var order = mapper.Map<Order>(request);

            // ← Fix: validate BEFORE saving — no orphaned DB records on failure
            var deviceRequest = mappingService.MapOrderToDeviceRequest(order);
            var outcome = validationService.Validate(deviceRequest);
            if (HasErrors(outcome))
            {
                logger.LogWarning("FHIR validation failed for lab DeviceRequest {Id}", deviceRequest.Id);
                return BadRequest(new { error = "FHIR validation failed for lab order." });
            }

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            await mediator.Publish(new LabOrderCreatedEvent(deviceRequest), HttpContext.RequestAborted);

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderID }, mapper.Map<OrderResponse>(order));
        }

        // ← Added: dedicated retry endpoint for BackgroundSyncService
        [HttpPost("retry")]
        public async Task<IActionResult> RetryLabOrder([FromBody] LabOrderRetryPayload payload)
        {
            if (string.IsNullOrWhiteSpace(payload?.FhirResource))
                return BadRequest(new { error = "Missing FHIR resource payload." });

            DeviceRequest deviceRequest;
            try
            {
                var parser = new FhirJsonParser();
                deviceRequest = parser.Parse<DeviceRequest>(payload.FhirResource);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to deserialize FHIR DeviceRequest for retry.");
                return BadRequest(new { error = "Invalid FHIR DeviceRequest payload." });
            }

            var outcome = validationService.Validate(deviceRequest);
            if (HasErrors(outcome))
            {
                logger.LogWarning("FHIR validation failed on retry for DeviceRequest {Id}", deviceRequest.Id);
                return BadRequest(new { error = "FHIR validation failed on retry." });
            }

            await mediator.Publish(new LabOrderCreatedEvent(deviceRequest), HttpContext.RequestAborted);

            logger.LogInformation("Retry published LabOrderCreatedEvent for DeviceRequest {Id}", deviceRequest.Id);
            return Ok(new { message = "Lab order retry accepted." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrderUpdateRequest request)
        {
            var order = await context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { error = "Order not found.", orderID = id });

            // ← Fix: capture previous status before mapping to detect transition
            var previousStatus = order.Status;
            mapper.Map(request, order);
            await context.SaveChangesAsync();

            // Only fire if status just transitioned TO Completed
            if (!string.Equals(previousStatus, "Completed", StringComparison.OrdinalIgnoreCase)
                && string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                await mediator.Publish(new LabOrderCompletedEventTrigger(order.OrderID), HttpContext.RequestAborted);
            }

            return Ok(new { message = "Order updated successfully.", order = mapper.Map<OrderResponse>(order) });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderPatchStatusRequest request)
        {
            var order = await context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { error = "Order not found.", orderID = id });

            // ← Fix: same transition guard for PATCH
            var previousStatus = order.Status;
            order.Status = request.Status;
            await context.SaveChangesAsync();

            if (!string.Equals(previousStatus, "Completed", StringComparison.OrdinalIgnoreCase)
                && string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                await mediator.Publish(new LabOrderCompletedEventTrigger(order.OrderID), HttpContext.RequestAborted);
            }

            return Ok(new { message = "Order status updated successfully.", order = mapper.Map<OrderResponse>(order) });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { error = "Order not found.", orderID = id });

            context.Orders.Remove(order);
            await context.SaveChangesAsync();

            return Ok(new { message = "Order deleted successfully.", orderID = id });
        }

        private static bool HasErrors(OperationOutcome outcome)
            => outcome.Issue.Any(issue => issue.Severity is OperationOutcome.IssueSeverity.Error or OperationOutcome.IssueSeverity.Fatal);
    }
}