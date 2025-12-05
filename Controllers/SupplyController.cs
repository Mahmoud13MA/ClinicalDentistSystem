using clinical.APIs.Data;
using clinical.APIs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Controllers
{
    [Authorize(Policy = "DoctorOnly")]
    [ApiController]
    [Route("[controller]")]
    public class SupplyController : Controller
    {
        private readonly AppDbContext _context;

        public SupplyController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Supply
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetSupplies()
        {
            var supplies = await _context.Supplies
                .Include(s => s.StockTransactions)
                .ToListAsync();

            if (supplies == null || supplies.Count == 0)
            {
                return NotFound(new { message = "No supplies found." });
            }

            return Ok(supplies);
        }

        // GET: /Supply/{id}
        [HttpGet("{Supply_ID}")]
        public async Task<IActionResult> GetSupplyById(int Supply_ID)
        {
            var supply = await _context.Supplies
                .Include(s => s.StockTransactions)
                .FirstOrDefaultAsync(s => s.Supply_ID == Supply_ID);

            if (supply == null)
            {
                return NotFound(new { error = "Supply not found.", supply_ID = Supply_ID });
            }

            return Ok(supply);
        }

        // GET: /Supply/Category/{category}
        [HttpGet("Category/{category}")]
        public async Task<IActionResult> GetSuppliesByCategory(string category)
        {
            var supplies = await _context.Supplies
                .Include(s => s.StockTransactions)
                .Where(s => s.Category.ToLower() == category.ToLower())
                .ToListAsync();

            if (supplies == null || supplies.Count == 0)
            {
                return NotFound(new { message = "No supplies found for this category.", category = category });
            }

            return Ok(supplies);
        }

        // GET: /Supply/LowStock/{threshold}
        [HttpGet("LowStock/{threshold}")]
        public async Task<IActionResult> GetLowStockSupplies(int threshold)
        {
            var supplies = await _context.Supplies
                .Where(s => s.Quantity <= threshold)
                .ToListAsync();

            if (supplies == null || supplies.Count == 0)
            {
                return NotFound(new { message = "No low stock supplies found.", threshold = threshold });
            }

            return Ok(supplies);
        }

        // POST: /Supply
        [HttpPost]
        public async Task<IActionResult> CreateSupply([FromBody] Supply supply)
        {
            if (supply == null)
            {
                return BadRequest(new { error = "Supply data is required.", hint = "Make sure you're sending a valid JSON body with supply information." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    error = "Validation failed",
                    details = errors,
                    hint = "Required fields: Supply_Name, Category, Unit, Quantity"
                });
            }

            try
            {
                _context.Supplies.Add(supply);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSupplyById), new { Supply_ID = supply.Supply_ID }, supply);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }

        // PUT: /Supply/{id}
        [HttpPut("{Supply_ID}")]
        public async Task<IActionResult> UpdateSupply(int Supply_ID, [FromBody] Supply supply)
        {
            if (supply == null)
            {
                return BadRequest(new { error = "Supply data is required." });
            }

            if (Supply_ID != supply.Supply_ID)
            {
                return BadRequest(new { error = "Supply ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    error = "Validation failed",
                    details = errors
                });
            }

            try
            {
                // Check if supply exists
                var existingSupply = await _context.Supplies.FindAsync(Supply_ID);
                if (existingSupply == null)
                {
                    return NotFound(new { error = "Supply not found.", supply_ID = Supply_ID });
                }

                // Update supply properties
                existingSupply.Supply_Name = supply.Supply_Name;
                existingSupply.Category = supply.Category;
                existingSupply.Unit = supply.Unit;
                existingSupply.Quantity = supply.Quantity;

                _context.Supplies.Update(existingSupply);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Supply updated successfully.", supply = existingSupply });
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if supply still exists
                if (!await _context.Supplies.AnyAsync(s => s.Supply_ID == Supply_ID))
                {
                    return NotFound(new { error = "Supply not found during update.", supply_ID = Supply_ID });
                }
                throw;
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }

        // PATCH: /Supply/{id}/AddStock
        [HttpPatch("{Supply_ID}/AddStock")]
        public async Task<IActionResult> AddStock(int Supply_ID, [FromBody] int quantity)
        {
            if (quantity <= 0)
            {
                return BadRequest(new { error = "Quantity must be greater than zero.", quantity = quantity });
            }

            try
            {
                var supply = await _context.Supplies.FindAsync(Supply_ID);
                if (supply == null)
                {
                    return NotFound(new { error = "Supply not found.", supply_ID = Supply_ID });
                }

                supply.Quantity += quantity;
                _context.Supplies.Update(supply);
                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "Stock added successfully.", 
                    supply_ID = Supply_ID,
                    added_quantity = quantity,
                    new_total = supply.Quantity 
                });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }

        // DELETE: /Supply/{id}
        [HttpDelete("{Supply_ID}")]
        public async Task<IActionResult> DeleteSupply(int Supply_ID)
        {
            try
            {
                var supply = await _context.Supplies
                    .Include(s => s.StockTransactions)
                    .FirstOrDefaultAsync(s => s.Supply_ID == Supply_ID);

                if (supply == null)
                {
                    return NotFound(new { error = "Supply not found.", supply_ID = Supply_ID });
                }

                // Check if supply has associated transactions
                if (supply.StockTransactions != null && supply.StockTransactions.Count > 0)
                {
                    return BadRequest(new 
                    { 
                        error = "Cannot delete supply with existing transactions.", 
                        supply_ID = Supply_ID,
                        transaction_count = supply.StockTransactions.Count,
                        hint = "Delete all associated transactions first."
                    });
                }

                _context.Supplies.Remove(supply);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Supply deleted successfully.", supply_ID = Supply_ID });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }
    }
}
