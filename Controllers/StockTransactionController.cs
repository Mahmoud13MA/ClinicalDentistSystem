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
    public class StockTransactionController : Controller
    {
        private readonly AppDbContext _context;

        public StockTransactionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /StockTransaction
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetStockTransactions()
        {
            var transactions = await _context.StockTransactions
                .Include(st => st.Doctor)
                .Include(st => st.Supply)
                .ToListAsync();

            if (transactions == null || transactions.Count == 0)
            {
                return NotFound(new { message = "No stock transactions found." });
            }

            return Ok(transactions);
        }

        // GET: /StockTransaction/{id}
        [HttpGet("{T_ID}")]
        public async Task<IActionResult> GetStockTransactionById(int T_ID)
        {
            var transaction = await _context.StockTransactions
                .Include(st => st.Doctor)
                .Include(st => st.Supply)
                .FirstOrDefaultAsync(st => st.T_ID == T_ID);

            if (transaction == null)
            {
                return NotFound(new { error = "Stock transaction not found.", transaction_ID = T_ID });
            }

            return Ok(transaction);
        }

        // GET: /StockTransaction/Doctor/{id}
        [HttpGet("Doctor/{Doctor_ID}")]
        public async Task<IActionResult> GetStockTransactionsByDoctor(int Doctor_ID)
        {
            var transactions = await _context.StockTransactions
                .Include(st => st.Doctor)
                .Include(st => st.Supply)
                .Where(st => st.Doctor_ID == Doctor_ID)
                .ToListAsync();

            if (transactions == null || transactions.Count == 0)
            {
                return NotFound(new { message = "No transactions found for this doctor.", doctor_ID = Doctor_ID });
            }

            return Ok(transactions);
        }

        // GET: /StockTransaction/Supply/{id}
        [HttpGet("Supply/{Supply_ID}")]
        public async Task<IActionResult> GetStockTransactionsBySupply(int Supply_ID)
        {
            var transactions = await _context.StockTransactions
                .Include(st => st.Doctor)
                .Include(st => st.Supply)
                .Where(st => st.Supply_ID == Supply_ID)
                .ToListAsync();

            if (transactions == null || transactions.Count == 0)
            {
                return NotFound(new { message = "No transactions found for this supply.", supply_ID = Supply_ID });
            }

            return Ok(transactions);
        }

        // POST: /StockTransaction
        [HttpPost]
        public async Task<IActionResult> CreateStockTransaction([FromBody] Stock_Transaction transaction)
        {
            if (transaction == null)
            {
                return BadRequest(new { error = "Stock transaction data is required.", hint = "Make sure you're sending a valid JSON body with transaction information." });
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
                    hint = "Required fields: Date (format: YYYY-MM-DD), Time (format: HH:mm:ss), Quantity, Doctor_ID, Supply_ID"
                });
            }

            try
            {
                // Validate that Doctor exists
                var doctorExists = await _context.Doctors.AnyAsync(d => d.ID == transaction.Doctor_ID);
                if (!doctorExists)
                {
                    return BadRequest(new { error = "Invalid Doctor_ID. Doctor does not exist.", doctor_ID = transaction.Doctor_ID });
                }

                // Validate that Supply exists
                var supplyExists = await _context.Supplies.AnyAsync(s => s.Supply_ID == transaction.Supply_ID);
                if (!supplyExists)
                {
                    return BadRequest(new { error = "Invalid Supply_ID. Supply does not exist.", supply_ID = transaction.Supply_ID });
                }

                // Check if supply has enough quantity
                var supply = await _context.Supplies.FindAsync(transaction.Supply_ID);
                if (supply.Quantity < transaction.Quantity)
                {
                    return BadRequest(new 
                    { 
                        error = "Insufficient supply quantity.", 
                        available = supply.Quantity, 
                        requested = transaction.Quantity 
                    });
                }

                // Deduct quantity from supply
                supply.Quantity -= transaction.Quantity;
                _context.Supplies.Update(supply);

                _context.StockTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetStockTransactionById), new { T_ID = transaction.T_ID }, transaction);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }

        // PUT: /StockTransaction/{id}
        [HttpPut("{T_ID}")]
        public async Task<IActionResult> UpdateStockTransaction(int T_ID, [FromBody] Stock_Transaction transaction)
        {
            if (transaction == null)
            {
                return BadRequest(new { error = "Stock transaction data is required." });
            }

            if (T_ID != transaction.T_ID)
            {
                return BadRequest(new { error = "Transaction ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
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
                // Check if transaction exists
                var existingTransaction = await _context.StockTransactions.FindAsync(T_ID);
                if (existingTransaction == null)
                {
                    return NotFound(new { error = "Stock transaction not found.", transaction_ID = T_ID });
                }

                // Validate that Doctor exists
                var doctorExists = await _context.Doctors.AnyAsync(d => d.ID == transaction.Doctor_ID);
                if (!doctorExists)
                {
                    return BadRequest(new { error = "Invalid Doctor_ID. Doctor does not exist.", doctor_ID = transaction.Doctor_ID });
                }

                // Validate that Supply exists
                var supplyExists = await _context.Supplies.AnyAsync(s => s.Supply_ID == transaction.Supply_ID);
                if (!supplyExists)
                {
                    return BadRequest(new { error = "Invalid Supply_ID. Supply does not exist.", supply_ID = transaction.Supply_ID });
                }

                // If quantity changed, adjust supply inventory
                if (existingTransaction.Quantity != transaction.Quantity || existingTransaction.Supply_ID != transaction.Supply_ID)
                {
                    // Restore old quantity to old supply
                    var oldSupply = await _context.Supplies.FindAsync(existingTransaction.Supply_ID);
                    oldSupply.Quantity += existingTransaction.Quantity;

                    // Deduct new quantity from new supply
                    var newSupply = await _context.Supplies.FindAsync(transaction.Supply_ID);
                    if (newSupply.Quantity < transaction.Quantity)
                    {
                        return BadRequest(new 
                        { 
                            error = "Insufficient supply quantity.", 
                            available = newSupply.Quantity, 
                            requested = transaction.Quantity 
                        });
                    }
                    newSupply.Quantity -= transaction.Quantity;

                    _context.Supplies.Update(oldSupply);
                    _context.Supplies.Update(newSupply);
                }

                // Update transaction properties
                existingTransaction.Date = transaction.Date;
                existingTransaction.Time = transaction.Time;
                existingTransaction.Quantity = transaction.Quantity;
                existingTransaction.Doctor_ID = transaction.Doctor_ID;
                existingTransaction.Supply_ID = transaction.Supply_ID;

                _context.StockTransactions.Update(existingTransaction);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Stock transaction updated successfully.", transaction = existingTransaction });
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if transaction still exists
                if (!await _context.StockTransactions.AnyAsync(st => st.T_ID == T_ID))
                {
                    return NotFound(new { error = "Stock transaction not found during update.", transaction_ID = T_ID });
                }
                throw;
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }

        // DELETE: /StockTransaction/{id}
        [HttpDelete("{T_ID}")]
        public async Task<IActionResult> DeleteStockTransaction(int T_ID)
        {
            try
            {
                var transaction = await _context.StockTransactions.FindAsync(T_ID);
                if (transaction == null)
                {
                    return NotFound(new { error = "Stock transaction not found.", transaction_ID = T_ID });
                }

                // Restore quantity to supply when deleting transaction
                var supply = await _context.Supplies.FindAsync(transaction.Supply_ID);
                supply.Quantity += transaction.Quantity;
                _context.Supplies.Update(supply);

                _context.StockTransactions.Remove(transaction);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Stock transaction deleted successfully.", transaction_ID = T_ID });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Internal server error", message = innerMessage });
            }
        }
    }
}
