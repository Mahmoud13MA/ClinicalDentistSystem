using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Shared.Services
{
    public class BackgroundSyncService(
        IServiceProvider serviceProvider,
        ILogger<BackgroundSyncService> logger) : BackgroundService
    {
        private readonly int _maxRetryAttempts = 5;
        private readonly TimeSpan _syncInterval = TimeSpan.FromSeconds(30);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Background Sync Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncPendingOperationsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while syncing pending operations.");
                }

                await Task.Delay(_syncInterval, stoppingToken);
            }

            logger.LogInformation("Background Sync Service is stopping.");
        }

        private async Task SyncPendingOperationsAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var queueContext = scope.ServiceProvider.GetRequiredService<LocalQueueDbContext>();

            var pendingOperations = await queueContext.PendingOperations
                .Where(x => x.Status == PendingOperationStatus.Pending && x.RetryCount < _maxRetryAttempts)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            if (!pendingOperations.Any())
            {
                return;
            }

            logger.LogInformation("Found {Count} pending operations to sync.", pendingOperations.Count);

            // We need an HttpClient to send the request back to ourselves
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            // Or just construct one pointing to localhost if we know the port, 
            // but ideally we should inject configuration to get the host URL

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            // Assume the API URL is injected or we just dispatch it internally. 
            // the most robust way in asp.net core for background tasks is sometimes 
            // internal dispatching, but HttpClient is easier if Kestrel is running
            // Or use mediatr, but HTTP is fine if we saved route and payload.

            // For this example we just dispatch HTTP back to localhost.
            var baseUrl = configuration["ApiBaseUrl"] 
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(';').FirstOrDefault()
                ?? "https://localhost:7044";
            var client = httpClientFactory.CreateClient("LocalSyncClient");
            client.BaseAddress = new Uri(baseUrl);

            foreach (var op in pendingOperations)
            {
                try
                {
                    logger.LogInformation("Attempting to sync operation {OperationId} for {Method} {Route} (Attempt {Attempt})", 
                        op.Id, op.HttpMethod, op.Route, op.RetryCount + 1);

                    op.LastAttemptAt = DateTime.UtcNow;
                    op.RetryCount++;

                    var request = new HttpRequestMessage(new HttpMethod(op.HttpMethod), op.Route)
                    {
                        Content = new StringContent(op.Payload, Encoding.UTF8, "application/json")
                    };

                    if (!string.IsNullOrWhiteSpace(op.IdempotencyKey))
                    {
                        request.Headers.Add("X-Idempotency-Key", op.IdempotencyKey);
                    }

                    // Add an internal sync header so we don't accidentally queue it again
                    // if it fails or if we need to bypass some auth (though usually we'd need a token)
                    // we'll assume the system can accept requests with a special header or we generate a system token
                    request.Headers.Add("X-System-Sync", "true");

                    var response = await client.SendAsync(request, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        op.Status = PendingOperationStatus.Completed;
                        op.CompletedAt = DateTime.UtcNow;
                        logger.LogInformation("Successfully synced operation {OperationId}", op.Id);
                    }
                    else if ((int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        // Temporary failure, keep it pending
                        op.LastError = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
                        logger.LogWarning("Temporary failure syncing operation {OperationId}: {Error}", op.Id, op.LastError);
                    }
                    else
                    {
                        // Permanent failure (4xx)
                        op.Status = PendingOperationStatus.Failed;
                        var content = await response.Content.ReadAsStringAsync(cancellationToken);
                        op.LastError = $"HTTP {(int)response.StatusCode}: {content}";
                        logger.LogError("Permanent failure syncing operation {OperationId}: {Error}", op.Id, op.LastError);
                    }
                }
                catch (Exception ex)
                {
                    op.LastError = ex.Message;
                    logger.LogError(ex, "Exception while attempting to sync operation {OperationId}", op.Id);
                }

                await queueContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}