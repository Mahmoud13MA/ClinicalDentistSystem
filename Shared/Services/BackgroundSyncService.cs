using System.Net.Http.Headers;
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

            // ← Fix #1: mark exhausted operations as Failed before processing
            var exhaustedOperations = await queueContext.PendingOperations
                .Where(x => x.Status == PendingOperationStatus.Pending && x.RetryCount >= _maxRetryAttempts)
                .ToListAsync(cancellationToken);

            foreach (var op in exhaustedOperations)
            {
                op.Status = PendingOperationStatus.Failed;
                op.LastError = "Max retry attempts reached.";
                logger.LogError(
                    "Operation {OperationId} for {Method} {Route} permanently failed after {Attempts} attempts.",
                    op.Id, op.HttpMethod, op.Route, op.RetryCount);
            }

            if (exhaustedOperations.Count > 0)
                await queueContext.SaveChangesAsync(cancellationToken);

            var pendingOperations = await queueContext.PendingOperations
                .Where(x => x.Status == PendingOperationStatus.Pending && x.RetryCount < _maxRetryAttempts)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            if (!pendingOperations.Any())
                return;

            logger.LogInformation("Found {Count} pending operations to sync.", pendingOperations.Count);

            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var jwtService = scope.ServiceProvider.GetRequiredService<clinical.APIs.Shared.Security.IJwtService>();

            var baseUrl = configuration["ApiBaseUrl"]
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(';').FirstOrDefault()
                ?? "https://localhost:7044";

            // ← Fix #2: warn when ApiBaseUrl is not explicitly configured
            if (configuration["ApiBaseUrl"] is null)
                logger.LogWarning("ApiBaseUrl not configured — falling back to localhost. This will fail in production.");

            // ← Fix #3: guard token creation — abort cycle if it fails
            string systemToken;
            try
            {
                systemToken = CreateSystemToken(jwtService, configuration);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create system token — aborting sync cycle.");
                return;
            }

            var client = httpClientFactory.CreateClient("LocalSyncClient");
            client.BaseAddress = new Uri(baseUrl);

            foreach (var op in pendingOperations)
            {
                try
                {
                    logger.LogInformation(
                        "Attempting to sync operation {OperationId} for {Method} {Route} (Attempt {Attempt})",
                        op.Id, op.HttpMethod, op.Route, op.RetryCount + 1);

                    op.LastAttemptAt = DateTime.UtcNow;
                    op.RetryCount++;

                    var request = new HttpRequestMessage(new HttpMethod(op.HttpMethod), op.Route)
                    {
                        Content = new StringContent(op.Payload, Encoding.UTF8, "application/json")
                    };

                    if (!string.IsNullOrWhiteSpace(op.IdempotencyKey))
                        request.Headers.Add("X-Idempotency-Key", op.IdempotencyKey);

                    request.Headers.Add("X-System-Sync", "true");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", systemToken);

                    var response = await client.SendAsync(request, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        op.Status = PendingOperationStatus.Completed;
                        op.CompletedAt = DateTime.UtcNow;
                        logger.LogInformation("Successfully synced operation {OperationId}.", op.Id);
                    }
                    else if ((int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        // Temporary failure — keep Pending for next cycle
                        op.LastError = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
                        logger.LogWarning("Temporary failure syncing operation {OperationId}: {Error}", op.Id, op.LastError);
                    }
                    else
                    {
                        // Permanent failure (4xx) — no point retrying
                        op.Status = PendingOperationStatus.Failed;
                        var content = await response.Content.ReadAsStringAsync(cancellationToken);
                        op.LastError = $"HTTP {(int)response.StatusCode}: {content}";
                        logger.LogError("Permanent failure syncing operation {OperationId}: {Error}", op.Id, op.LastError);
                    }
                }
                catch (Exception ex)
                {
                    op.LastError = ex.Message;
                    logger.LogError(ex, "Exception while attempting to sync operation {OperationId}.", op.Id);
                }

                // Save after each operation — don't lose progress if a later one fails
                await queueContext.SaveChangesAsync(cancellationToken);
            }
        }

        private static string CreateSystemToken(
            clinical.APIs.Shared.Security.IJwtService jwtService,
            IConfiguration configuration)
        {
            var syncSettings = configuration.GetSection("SyncAuth");
            var systemUserId = syncSettings["UserId"] ?? "0";
            var systemEmail = syncSettings["Email"] ?? "system-sync@clinicaldentist.local";
            var systemName = syncSettings["Name"] ?? "System Sync";
            var systemRole = syncSettings["Role"] ?? "Admin";

            if (!int.TryParse(systemUserId, out var parsedUserId))
                parsedUserId = 0;

            return jwtService.GenerateToken(parsedUserId, systemEmail, systemName, systemRole);
        }
    }
}