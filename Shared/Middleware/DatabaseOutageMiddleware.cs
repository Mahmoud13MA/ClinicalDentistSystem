using System.Net;
using System.Text;
using System.Text.Json;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Shared.Middleware
{
    public class DatabaseOutageMiddleware(RequestDelegate next, ILogger<DatabaseOutageMiddleware> logger)
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            context.Request.EnableBuffering();

            try
            {
                await next(context);
            }
            catch (Exception ex) when (IsConnectivityError(ex))
            {
                logger.LogError(ex, "Connectivity error handled by outage middleware.");
                await HandleConnectivityFailureAsync(context, serviceProvider, ex, context.RequestAborted);
            }
        }

        private static bool IsConnectivityError(Exception ex)
        {
            var rootEx = ex.GetBaseException();

            return rootEx switch
            {
                SqlException sqlEx => sqlEx.Number is 53 or -2 or -1 or 20 or 64,
                TimeoutException => true,
                DbUpdateException dbEx => IsDbUpdateConnectivityError(dbEx),
                _ => false
            };
        }

        private static bool IsDbUpdateConnectivityError(DbUpdateException dbEx)
        {
            // Check if the inner exception is a SQL connectivity error
            var innerEx = dbEx.InnerException;

            if (innerEx is SqlException sqlEx)
            {
                // Only treat specific connectivity-related SQL errors as transient
                // 53: Named Pipes error
                // -2: Timeout
                // -1: Connection broken
                // 20: Instance not found
                // 64: Communication link failure
                // 233: Connection initialization error
                // 4060: Cannot open database
                return sqlEx.Number is 53 or -2 or -1 or 20 or 64 or 233 or 4060;
            }

            if (innerEx is TimeoutException)
            {
                return true;
            }

            // Do not treat other DbUpdateException types as connectivity errors
            // (e.g., constraint violations, schema errors, validation failures)
            return false;
        }

        private static bool IsWriteOperation(string method)
            => HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsDelete(method);

        private async Task HandleConnectivityFailureAsync(
            HttpContext context,
            IServiceProvider serviceProvider,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var method = context.Request.Method;
            if (IsWriteOperation(method))
            {
                await TryQueueFailedOperationAsync(context, serviceProvider, exception, cancellationToken);
            }

            if (context.Response.HasStarted)
            {
                logger.LogWarning("Cannot write outage response because response has already started for {Method} {Path}.", method, context.Request.Path);
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            context.Response.ContentType = "application/json";

            var responseObj = new
            {
                error = "Service Unavailable",
                message = "Temporary infrastructure outage detected. You can safely retry this request.",
                retryable = true
            };

            await context.Response.WriteAsJsonAsync(responseObj, SerializerOptions, cancellationToken);
        }

        private async Task TryQueueFailedOperationAsync(
            HttpContext context,
            IServiceProvider serviceProvider,
            Exception exception,
            CancellationToken cancellationToken)
        {
            try
            {
                var payload = await ReadRequestPayloadAsync(context.Request, cancellationToken);
                var idempotencyKey = context.Request.Headers["X-Idempotency-Key"].FirstOrDefault();
                var route = context.Request.Path.Value ?? string.Empty;
                var method = context.Request.Method;

                using var scope = serviceProvider.CreateScope();
                var queueContext = scope.ServiceProvider.GetRequiredService<LocalQueueDbContext>();

                if (!string.IsNullOrWhiteSpace(idempotencyKey))
                {
                    var duplicatePending = await queueContext.PendingOperations.AnyAsync(
                        x => x.IdempotencyKey == idempotencyKey && x.Status == PendingOperationStatus.Pending,
                        cancellationToken);

                    if (duplicatePending)
                    {
                        logger.LogInformation(
                            "Skipping duplicate pending queue item for idempotency key {IdempotencyKey} on {Method} {Path}.",
                            idempotencyKey,
                            method,
                            route);
                        return;
                    }
                }

                var pendingOp = new PendingOperation
                {
                    HttpMethod = method,
                    Route = route,
                    Payload = payload,
                    IdempotencyKey = idempotencyKey,
                    Status = PendingOperationStatus.Pending,
                    LastError = exception.GetBaseException().Message,
                    LastAttemptAt = DateTime.UtcNow
                };

                queueContext.PendingOperations.Add(pendingOp);
                await queueContext.SaveChangesAsync(cancellationToken);

                logger.LogWarning(
                    "Queued failed write operation {OperationId} for {Method} {Path} due to connectivity outage.",
                    pendingOp.Id,
                    method,
                    route);
            }
            catch (Exception queueEx)
            {
                logger.LogCritical(queueEx, "Fallback queue persistence failed while handling connectivity outage.");
            }
        }

        private static async Task<string> ReadRequestPayloadAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            if (request.Body.CanSeek)
            {
                request.Body.Position = 0;
            }

            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var payload = await reader.ReadToEndAsync(cancellationToken);

            if (request.Body.CanSeek)
            {
                request.Body.Position = 0;
            }

            return payload;
        }
    }
}
