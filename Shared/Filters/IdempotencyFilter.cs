using clinical.APIs.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace clinical.APIs.Shared.Filters;

public class IdempotencyFilter : IAsyncActionFilter
{
    private const string IdempotencyHeader = "X-Idempotency-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        // Let GET / HEAD / OPTIONS pass through
        if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method) || HttpMethods.IsOptions(request.Method))
        {
            await next();
            return;
        }

        // (Optional) Exclude login endpoint – remove this block if you want to enforce the key there too
        if (request.Path.StartsWithSegments("/api/admin/login", StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        // 1. Enforce header presence
        if (!request.Headers.TryGetValue(IdempotencyHeader, out var headerValues) ||
            string.IsNullOrWhiteSpace(headerValues.FirstOrDefault()))
        {
            context.Result = new BadRequestObjectResult(new
            {
                error = "Idempotency Key Required",
                message = $"The {IdempotencyHeader} header is required for safe duplicate prevention on mutating requests."
            });
            return;
        }

        var idempotencyKey = headerValues.First()!;

        // 2. Resolve the idempotency service from the current request scope
        var idempotencyService = context.HttpContext.RequestServices.GetRequiredService<IIdempotencyService>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<IdempotencyFilter>>();

        // 3. Check if the key was already processed
        bool isDuplicate = false;
        try
        {
            isDuplicate = await idempotencyService.IsDuplicateAsync(idempotencyKey);
        }
        catch (Exception ex)
        {
            // If the main DB is down, let the exception bubble up to the outage middleware
            logger.LogWarning(ex, "Idempotency check failed (DB likely down) – forwarding to outage middleware.");
            throw;
        }

        if (isDuplicate)
        {
            logger.LogInformation("Idempotency match: key {Key} already processed – blocking duplicate.", idempotencyKey);
            context.Result = new ConflictObjectResult(new
            {
                error = "Duplicate Request",
                message = "A request with this idempotency key was already successfully processed. Your data is safe."
            });
            return;
        }

        // 4. Execute the controller action
        var executedContext = await next();

        // 5. After successful execution (2xx), record the key
        if (executedContext.Exception == null && executedContext.Result != null)
        {
            var statusCode = context.HttpContext.Response.StatusCode;
            if (statusCode >= 200 && statusCode <= 299)   // Only on success
            {
                try
                {
                    await idempotencyService.MarkAsProcessedAsync(
                        idempotencyKey,
                        request.Method,
                        request.Path.Value ?? string.Empty);
                    logger.LogInformation("Recorded processed idempotency key {Key}", idempotencyKey);
                }
                catch (Exception ex)
                {
                    // A failure to record shouldn't break the successful response
                    logger.LogError(ex, "Failed to record idempotency key after successful request.");
                }
            }
        }
    }
}