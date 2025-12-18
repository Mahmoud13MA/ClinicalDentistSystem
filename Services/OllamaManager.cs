using System.Diagnostics;
using System.Text.Json;

namespace clinical.APIs.Services
{
    public class OllamaManager : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OllamaManager> _logger;
        private readonly string _ollamaEndpoint;

        public OllamaManager(IConfiguration configuration, ILogger<OllamaManager> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _ollamaEndpoint = _configuration["LlamaSettings:ApiEndpoint"] ?? "http://localhost:11434";
        }

        public async Task StartWithFallbackAsync()
        {
            _logger.LogInformation("Checking Ollama AI Service...");

            // Check if Ollama is already running
            if (await IsOllamaRunningAsync())
            {
                _logger.LogInformation("Ollama is already running and ready");

                // Test if the model is available
                if (await TestModelAsync())
                {
                    _logger.LogInformation("Model 'llama3.1:8b' is loaded and ready");
                    return;
                }
                else
                {
                    _logger.LogWarning("Model 'llama3.1:8b' not found. Pull it with: ollama pull llama3.1:8b");
                    return;
                }
            }
            _logger.LogError("❌ Ollama is not running.");
          
        }

        private async Task<bool> IsOllamaRunningAsync()
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                var response = await client.GetAsync($"{_ollamaEndpoint}/api/tags");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Ollama connection failed: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> TestModelAsync()
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

                var response = await client.GetAsync($"{_ollamaEndpoint}/api/tags");
                if (!response.IsSuccessStatusCode) return false;

                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);

                if (json.RootElement.TryGetProperty("models", out var models))
                {
                    foreach (var model in models.EnumerateArray())
                    {
                        if (model.TryGetProperty("name", out var name))
                        {
                            var modelName = name.GetString() ?? "";
                            if (modelName.StartsWith("llama3.1:8b", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Model check failed: {ex.Message}");
                return false;
            }
        }

        public void Stop()
        {
            _logger.LogInformation("Ollama service continues running (managed externally)");
        }

        public void Dispose()
        {
            
            GC.SuppressFinalize(this);
        }
    }
}
