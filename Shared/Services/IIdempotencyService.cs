namespace clinical.APIs.Shared.Services;

public interface IIdempotencyService
{
    Task<bool> IsDuplicateAsync(string idempotencyKey);
    Task MarkAsProcessedAsync(string idempotencyKey, string httpMethod, string route);
}