using AutoMapper;
using AutoMapper.QueryableExtensions;
using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Modules.ProsthodonticLab.DTOs;
using clinical.APIs.Shared.Data;
using clinical.APIs.Modules.ProsthodonticLab.Handlers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.ProsthodonticLab.Controllers
{
    [Authorize(Policy = "LabTechnician")]
    [ApiController]
    [Route("api/v1/prosthodonticlab/[controller]")]
    public class OrderController(AppDbContext context, IMapper mapper, IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await context.Orders.ProjectTo<OrderResponse>(mapper.ConfigurationProvider).ToListAsync();
            
            if (orders.Count == 0)
            {
                return NotFound(new { error = "No orders found." });
            }

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound(new { error = "Order not found.", orderID = id });
            }

            return Ok(mapper.Map<OrderResponse>(order));
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
        {
            var order = mapper.Map<Order>(request);

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var response = mapper.Map<OrderResponse>(order);

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderID }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrderUpdateRequest request)
        {
            var order = await context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { error = "Order not found.", orderID = id });
            }

            mapper.Map(request, order);

            await context.SaveChangesAsync();

            var response = mapper.Map<OrderResponse>(order);

            return Ok(new { message = "Order updated successfully.", order = response });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderPatchStatusRequest request)
        {
            var order = await context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { error = "Order not found.", orderID = id });
            }

            order.Status = request.Status;

            await context.SaveChangesAsync();

            if (string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                await mediator.Publish(new LabOrderCompletedEventTrigger(order.OrderID), HttpContext.RequestAborted);
            }

            var response = mapper.Map<OrderResponse>(order);

            return Ok(new { message = "Order status updated successfully.", order = response });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { error = "Order not found.", orderID = id });
            }

            context.Orders.Remove(order);
            await context.SaveChangesAsync();

            return Ok(new { message = "Order deleted successfully.", orderID = id });
        }
    }
}
