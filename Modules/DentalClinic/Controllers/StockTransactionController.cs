using clinical.APIs.Modules.DentalClinic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using clinical.APIs.Shared.Data;
using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Services;

namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [Authorize(Policy = "DoctorOnly")]
    [ApiController]
    [Route("[controller]")]
    public class StockTransactionController(AppDbContext context, IStockTransactionMappingService mappingService) : ControllerBase
    {
        private IQueryable<Stock_Transaction> StockTransactionsWithDetails => context.StockTransactions
            .Include(st => st.Doctor)
            .Include(st => st.Supply);

        private static List<string> GetValidationErrors(ModelStateDictionary modelState)
        {
            return modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
        }

        private async Task<StockTransactionResponse?> GetTransactionResponseAsync(int transactionId)
        {
            var transaction = await StockTransactionsWithDetails
                .FirstOrDefaultAsync(st => st.T_ID == transactionId);

            return transaction == null ? null : mappingService.MapToResponse(transaction);
        }

        // GET: /StockTransaction
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetStockTransactions()
        {
            var transactions = await StockTransactionsWithDetails.ToListAsync();

            if (transactions.Count == 0)
            {
                return NotFound(new { message = "No stock transactions found." });
            }

            return Ok(mappingService.MapToResponseList(transactions));
        }

        // GET: /StockTransaction/{id}
        [HttpGet("{T_ID}")]
        public async Task<IActionResult> GetStockTransactionById(int T_ID)
        {
            var response = await GetTransactionResponseAsync(T_ID);

            if (response == null)
            {
                return NotFound(new { error = "Stock transaction not found.", transaction_ID = T_ID });
            }

            return Ok(response);
        }

        // GET: /StockTransaction/Doctor/{id}
        [HttpGet("Doctor/{Doctor_ID}")]
        public async Task<IActionResult> GetStockTransactionsByDoctor(int Doctor_ID)
        {
            var transactions = await StockTransactionsWithDetails
                .Where(st => st.Doctor_ID == Doctor_ID)
                .ToListAsync();

            if (transactions.Count == 0)
            {
                return NotFound(new { message = "No transactions found for this doctor.", doctor_ID = Doctor_ID });
            }

            return Ok(mappingService.MapToResponseList(transactions));
        }

        // GET: /StockTransaction/Supply/{id}
        [HttpGet("Supply/{Supply_ID}")]
        public async Task<IActionResult> GetStockTransactionsBySupply(int Supply_ID)
        {
            var transactions = await StockTransactionsWithDetails
                .Where(st => st.Supply_ID == Supply_ID)
                .ToListAsync();

            if (transactions.Count == 0)
            {
                return NotFound(new { message = "No transactions found for this supply.", supply_ID = Supply_ID });
            }

            return Ok(mappingService.MapToResponseList(transactions));
        }

        // POST: /StockTransaction
        [HttpPost]
        public async Task<IActionResult> CreateStockTransaction([FromBody] StockTransactionCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Stock transaction data is required.", hint = "Make sure you're sending a valid JSON body with transaction information." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    error = "Validation failed",
                    details = GetValidationErrors(ModelState),
                    hint = "Required fields: Date (format: YYYY-MM-DD), Time (format: HH:mm:ss), Quantity, Doctor_ID, Supply_ID"
                });
            }

            var doctorExists = await context.Doctors.AnyAsync(d => d.ID == request.Doctor_ID);
            if (!doctorExists)
            {
                return BadRequest(new { error = "Invalid Doctor_ID. Doctor does not exist.", doctor_ID = request.Doctor_ID });
            }

            var supply = await context.Supplies.FindAsync(request.Supply_ID);
            if (supply == null)
            {
                return BadRequest(new { error = "Invalid Supply_ID. Supply does not exist.", supply_ID = request.Supply_ID });
            }

            if (supply.Quantity < request.Quantity)
            {
                return BadRequest(new
                {
                    error = "Insufficient supply quantity.",
                    available = supply.Quantity,
                    requested = request.Quantity
                });
            }

            supply.Quantity -= request.Quantity;

            var transaction = new Stock_Transaction
            {
                Date = request.Date,
                Time = request.Time,
                Quantity = request.Quantity,
                Doctor_ID = request.Doctor_ID,
                Supply_ID = request.Supply_ID
            };

            context.StockTransactions.Add(transaction);
            await context.SaveChangesAsync();

            var response = await GetTransactionResponseAsync(transaction.T_ID);
            if (response == null)
            {
                return StatusCode(500, new { error = "Internal server error", message = "Created stock transaction could not be loaded." });
            }

            return CreatedAtAction(nameof(GetStockTransactionById), new { T_ID = transaction.T_ID }, response);
        }

        // PUT: /StockTransaction/{id}
        [HttpPut("{T_ID}")]
        public async Task<IActionResult> UpdateStockTransaction(int T_ID, [FromBody] StockTransactionUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Stock transaction data is required." });
            }

            if (T_ID != request.T_ID)
            {
                return BadRequest(new { error = "Transaction ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    error = "Validation failed",
                    details = GetValidationErrors(ModelState)
                });
            }

            var existingTransaction = await context.StockTransactions.FindAsync(T_ID);
            if (existingTransaction == null)
            {
                return NotFound(new { error = "Stock transaction not found.", transaction_ID = T_ID });
            }

            var doctorExists = await context.Doctors.AnyAsync(d => d.ID == request.Doctor_ID);
            if (!doctorExists)
            {
                return BadRequest(new { error = "Invalid Doctor_ID. Doctor does not exist.", doctor_ID = request.Doctor_ID });
            }

            var supplyExists = await context.Supplies.AnyAsync(s => s.Supply_ID == request.Supply_ID);
            if (!supplyExists)
            {
                return BadRequest(new { error = "Invalid Supply_ID. Supply does not exist.", supply_ID = request.Supply_ID });
            }

            if (existingTransaction.Quantity != request.Quantity || existingTransaction.Supply_ID != request.Supply_ID)
            {
                var oldSupply = await context.Supplies.FindAsync(existingTransaction.Supply_ID);
                if (oldSupply == null)
                {
                    return BadRequest(new { error = "Original supply not found.", supply_ID = existingTransaction.Supply_ID });
                }

                oldSupply.Quantity += existingTransaction.Quantity;

                var newSupply = await context.Supplies.FindAsync(request.Supply_ID);
                if (newSupply == null)
                {
                    return BadRequest(new { error = "Target supply not found.", supply_ID = request.Supply_ID });
                }

                if (newSupply.Quantity < request.Quantity)
                {
                    oldSupply.Quantity -= existingTransaction.Quantity;
                    return BadRequest(new
                    {
                        error = "Insufficient supply quantity.",
                        available = newSupply.Quantity,
                        requested = request.Quantity
                    });
                }

                newSupply.Quantity -= request.Quantity;
            }

            existingTransaction.Date = request.Date;
            existingTransaction.Time = request.Time;
            existingTransaction.Quantity = request.Quantity;
            existingTransaction.Doctor_ID = request.Doctor_ID;
            existingTransaction.Supply_ID = request.Supply_ID;

            await context.SaveChangesAsync();

            var response = await GetTransactionResponseAsync(T_ID);
            if (response == null)
            {
                return StatusCode(500, new { error = "Internal server error", message = "Updated stock transaction could not be loaded." });
            }

            return Ok(new { message = "Stock transaction updated successfully.", transaction = response });
        }

        // DELETE: /StockTransaction/{id}
        [HttpDelete("{T_ID}")]
        public async Task<IActionResult> DeleteStockTransaction(int T_ID)
        {
            var transaction = await context.StockTransactions.FindAsync(T_ID);
            if (transaction == null)
            {
                return NotFound(new { error = "Stock transaction not found.", transaction_ID = T_ID });
            }

            var supply = await context.Supplies.FindAsync(transaction.Supply_ID);
            if (supply == null)
            {
                return BadRequest(new { error = "Associated supply not found.", supply_ID = transaction.Supply_ID });
            }

            supply.Quantity += transaction.Quantity;

            context.StockTransactions.Remove(transaction);
            await context.SaveChangesAsync();

            return Ok(new { message = "Stock transaction deleted successfully.", transaction_ID = T_ID });
        }
    }
}
