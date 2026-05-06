
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Shared.Services
{
    public class IdempotencyService(AppDbContext _context) : IIdempotencyService
    {


        public async Task<bool> IsDuplicateAsync(string idempotencyKey)
        {
            return await _context.ProcessedRequests.AnyAsync(p => p.IdempotencyKey == idempotencyKey);
        }

        public async Task MarkAsProcessedAsync(string idempotencyKey, string httpMethod, string route)
        {
            _context.ProcessedRequests.Add(new ProcessedRequest
            {
                IdempotencyKey = idempotencyKey,
                HttpMethod = httpMethod,
                Route = route,
            });
            await _context.SaveChangesAsync();
        }


    }
}

