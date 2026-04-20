using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [Authorize(Policy = "DoctorOnly")]
    [ApiController]
    [Route("[controller]")]
    public class SupplyController(AppDbContext context) : ControllerBase
    {
        private static StockTransactionBasicInfo MapStockTransactionToBasicInfo(Stock_Transaction transaction)
        {
            return new StockTransactionBasicInfo
            {
                T_ID = transaction.T_ID,
                Date = transaction.Date,
                Time = transaction.Time,
                Quantity = transaction.Quantity,
                Doctor_ID = transaction.Doctor_ID,
                DoctorName = transaction.Doctor?.Name
            };
        }

        private static SupplyResponse MapSupplyToResponse(Supply supply)
        {
            return new SupplyResponse
            {
                Supply_ID = supply.Supply_ID,
                Supply_Name = supply.Supply_Name,
                Category = supply.Category,
                Unit = supply.Unit,
                Quantity = supply.Quantity,
                Description = supply.Description,
                StockTransactions = supply.StockTransactions?.Select(MapStockTransactionToBasicInfo).ToList()
            };
        }

        private static SupplyBasicInfo MapSupplyToBasicInfo(Supply supply)
        {
            return new SupplyBasicInfo
            {
                Supply_ID = supply.Supply_ID,
                Supply_Name = supply.Supply_Name,
                Category = supply.Category,
                Unit = supply.Unit,
                Quantity = supply.Quantity,
                Description = supply.Description
            };
        }

        // GET: /Supply
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetSupplies()
        {
            var supplies = await context.Supplies
                .Include(s => s.StockTransactions)
                    .ThenInclude(st => st.Doctor)
                .ToListAsync();

            if (supplies.Count == 0)
            {
                return NotFound(new { message = "No supplies found." });
            }

            return Ok(supplies.Select(MapSupplyToResponse).ToList());
        }

        // GET: /Supply/{id}
        [HttpGet("{Supply_ID}")]
        public async Task<IActionResult> GetSupplyById(int Supply_ID)
        {
            var supply = await context.Supplies
                .Include(s => s.StockTransactions)
                    .ThenInclude(st => st.Doctor)
                .FirstOrDefaultAsync(s => s.Supply_ID == Supply_ID);

            if (supply == null)
            {
                return NotFound(new { error = "Supply not found.", supply_ID = Supply_ID });
            }

            return Ok(MapSupplyToResponse(supply));
        }

        // GET: /Supply/Category/{category}
        [HttpGet("Category/{category}")]
        public async Task<IActionResult> GetSuppliesByCategory(string category)
        {
            var supplies = await context.Supplies
                .Include(s => s.StockTransactions)
                    .ThenInclude(st => st.Doctor)
                .Where(s => string.Equals(s.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();

            if (supplies.Count == 0)
            {
                return NotFound(new { message = "No supplies found for this category.", category });
            }

            return Ok(supplies.Select(MapSupplyToResponse).ToList());
        }

        // GET: /Supply/LowStock/{threshold}
        [HttpGet("LowStock/{threshold}")]
        public async Task<IActionResult> GetLowStockSupplies(int threshold)
        {
            var supplies = await context.Supplies
                .Where(s => s.Quantity <= threshold)
                .ToListAsync();

            if (supplies.Count == 0)
            {
                return NotFound(new { message = "No low stock supplies found.", threshold });
            }

            return Ok(supplies.Select(MapSupplyToBasicInfo).ToList());
        }

        // POST: /Supply
        [HttpPost]
        public async Task<IActionResult> CreateSupply([FromBody] SupplyCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Supply data is required.", hint = "Make sure you're sending a valid JSON body with supply information." });
            }

            var supply = new Supply
            {
                Supply_Name = request.Supply_Name,
                Category = request.Category,
                Unit = request.Unit,
                Quantity = request.Quantity,
                Description = request.Description
            };

            context.Supplies.Add(supply);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSupplyById), new { Supply_ID = supply.Supply_ID }, MapSupplyToBasicInfo(supply));
        }

        // PUT: /Supply/{id}
        [HttpPut("{Supply_ID}")]
        public async Task<IActionResult> UpdateSupply(int Supply_ID, [FromBody] SupplyUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Supply data is required." });
            }

            if (Supply_ID != request.Supply_ID)
            {
                return BadRequest(new { error = "Supply ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
            }

            var existingSupply = await context.Supplies.FindAsync(Supply_ID);
            if (existingSupply == null)
            {
                return NotFound(new { error = "Supply not found.", supply_ID = Supply_ID });
            }

            existingSupply.Supply_Name = request.Supply_Name;
            existingSupply.Category = request.Category;
            existingSupply.Unit = request.Unit;
            existingSupply.Quantity = request.Quantity;
            existingSupply.Description = request.Description;

            await context.SaveChangesAsync();

            return Ok(new { message = "Supply updated successfully.", supply = MapSupplyToBasicInfo(existingSupply) });
        }

        // PATCH: /Supply/{id}/AddStock
        [HttpPatch("{Supply_ID}/AddStock")]
        public async Task<IActionResult> AddStock(int Supply_ID, [FromBody] SupplyAddStockRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Supply data is required." });
            }

            var supply = await context.Supplies.FindAsync(Supply_ID);
            if (supply == null)
            {
                return NotFound(new { error = "Supply not found.", supply_ID = Supply_ID });
            }

            supply.Quantity += request.Quantity;
            await context.SaveChangesAsync();

            return Ok(new
            {
                message = "Stock added successfully.",
                supply_ID = Supply_ID,
                added_quantity = request.Quantity,
                new_total = supply.Quantity
            });
        }

        // DELETE: /Supply/{id}
        [HttpDelete("{Supply_ID}")]
        public async Task<IActionResult> DeleteSupply(int Supply_ID)
        {
            var supply = await context.Supplies.FindAsync(Supply_ID);
            if (supply == null)
            {
                return NotFound(new { error = "Supply not found.", supply_ID = Supply_ID });
            }

            var transactionCount = await context.StockTransactions.CountAsync(st => st.Supply_ID == Supply_ID);
            if (transactionCount > 0)
            {
                return BadRequest(new
                {
                    error = "Cannot delete supply with existing transactions.",
                    supply_ID = Supply_ID,
                    transaction_count = transactionCount,
                    hint = "Delete all associated transactions first."
                });
            }

            context.Supplies.Remove(supply);
            await context.SaveChangesAsync();

            return Ok(new { message = "Supply deleted successfully.", supply_ID = Supply_ID });
        }
    }
}
