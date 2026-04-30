using System.Net;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Shared.Middleware
{
    public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Preserve DbUpdateConcurrencyException handling
                logger.LogWarning(ex, "A database concurrency conflict occurred for {Method} {Path}", context.Request.Method, context.Request.Path);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;

                await context.Response.WriteAsJsonAsync(new { error = "The record was modified by another user. Please try again." });
            }
            catch (Exception ex)
            {
                // Generic 500 response for all other unhandled exceptions (per copilot instructions)
                logger.LogError(ex, "An unhandled exception occurred during {Method} {Path}", context.Request.Method, context.Request.Path);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
            }
        }
    }
}