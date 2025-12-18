using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace clinical.APIs.Services
{
    public class LlamaService : ILlamaService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _llamaApiEndpoint;
        private readonly string _model;
        private readonly double _temperature;
        private readonly int _maxTokens;
        private readonly double _topP;
        private readonly int _topK;
        private readonly int _contextLength;
        private readonly double _repeatPenalty;
        private readonly List<string> _stopSequences;
        private readonly int _gpuLayers;

        public LlamaService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            
            _llamaApiEndpoint = _configuration["LlamaSettings:ApiEndpoint"] ?? "http://localhost:11434/api/generate";
            _model = _configuration["LlamaSettings:Model"] ?? "llama3.1:8b";
            _temperature = double.Parse(_configuration["LlamaSettings:Temperature"] ?? "0.3");
            _maxTokens = int.Parse(_configuration["LlamaSettings:MaxTokens"] ?? "2000");
            _topP = double.Parse(_configuration["LlamaSettings:TopP"] ?? "0.9");
            _topK = int.Parse(_configuration["LlamaSettings:TopK"] ?? "40");
            _contextLength = int.Parse(_configuration["LlamaSettings:ContextLength"] ?? "4096");
            _repeatPenalty = double.Parse(_configuration["LlamaSettings:RepeatPenalty"] ?? "1.1");
            _gpuLayers = int.Parse(_configuration["LlamaSettings:GpuLayers"] ?? "20");
            
            _stopSequences = _configuration.GetSection("LlamaSettings:Stop").Get<List<string>>() 
                ?? new List<string> { "###", "User:", "Assistant:" };
            
            var timeoutSeconds = int.Parse(_configuration["LlamaSettings:TimeoutSeconds"] ?? "180");
            _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        }

        public async Task<List<string>> GetAutoCompleteSuggestionsAsync(string partialText, string context)
        {
            var prompt = $@"You are a dental clinical assistant. Complete the following partial clinical note with 3 relevant suggestions.
Context: {context}
Partial text: {partialText}

Provide only 3 short, relevant completions (max 10 words each), one per line. Focus on dental terminology and procedures.
###";

            var response = await CallLlamaAsync(prompt, maxTokens: 100);
            
            return response
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Take(3)
                .ToList();
        }

        public async Task<string> GenerateClinicalNotesAsync(string bulletPoints, string patientContext)
        {
            var prompt = $@"You are a professional dental clinical assistant. Convert the following bullet points into a well-formatted clinical note.

Patient Context: {patientContext}

Bullet Points:
{bulletPoints}

Generate a professional clinical note with proper dental terminology. Use present tense. Be concise and clinical.
###";

            return await CallLlamaAsync(prompt, maxTokens: 500);
        }

        public async Task<List<string>> SuggestTreatmentsAsync(string diagnosis, string patientHistory)
        {
            var prompt = $@"You are a dental AI assistant. Based on the diagnosis and patient history, suggest 3-5 appropriate treatment options.

Diagnosis: {diagnosis}
Patient History: {patientHistory}

List treatment options with brief explanations (max 20 words each). Format: Treatment name - brief explanation.
###";

            var response = await CallLlamaAsync(prompt, maxTokens: 300);
            
            return response
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s) && s.Contains('-'))
                .ToList();
        }

        public async Task<ClinicalDataExtractionResult> ExtractClinicalDataAsync(string freeText)
        {
            var prompt = $@"You are a dental AI assistant. Extract structured clinical data from the following free text and return ONLY valid JSON.

Free text: {freeText}

Extract the following fields:
- diagnosis: Primary dental diagnosis (string)
- symptoms: List of reported symptoms (array of strings)
- treatments: List of treatments mentioned (array of strings)
- periodontalStatus: Periodontal health status (string or null)
- medications: List of medications (array of strings)
- affectedTeeth: Tooth numbers using Universal Numbering System (array of integers, 1-32). Tooth 1 is upper right 3rd molar, tooth 16 is lower left 3rd molar, tooth 32 is lower right 3rd molar.

Return JSON format:
{{
  ""diagnosis"": ""extracted diagnosis"",
  ""symptoms"": [""symptom1"", ""symptom2""],
  ""treatments"": [""treatment1"", ""treatment2""],
  ""periodontalStatus"": ""status or null"",
  ""medications"": [""med1"", ""med2""],
  ""affectedTeeth"": [3, 14, 19, 30]
}}

Return ONLY the JSON object, no explanations:
###";

            var response = await CallLlamaAsync(prompt, maxTokens: 400);
            
            try
            {
                var jsonStart = response.IndexOf('{');
                var jsonEnd = response.LastIndexOf('}');
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonString = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    return JsonSerializer.Deserialize<ClinicalDataExtractionResult>(jsonString, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                        ?? new ClinicalDataExtractionResult();
                }
            }
            catch (JsonException)
            {
                // Return empty result if parsing fails
            }
            
            return new ClinicalDataExtractionResult();
        }

        public async Task<List<string>> GetDentalTerminologySuggestionsAsync(string partialTerm)
        {
            var prompt = $@"Suggest 5 dental terms that start with or contain: {partialTerm}

Return only the term names, one per line. Include common dental procedures, conditions, and anatomical terms.
###";

            var response = await CallLlamaAsync(prompt, maxTokens: 100);
            
            return response
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Take(5)
                .ToList();
        }

        public async Task<CompleteEHRExtractionResult> ParseToCompleteEHRAsync(string largeText, string patientContext, CancellationToken cancellationToken = default)
        {
            var prompt = $@"You are a dental AI assistant. Extract comprehensive EHR data from doctor's notes. Return ONLY valid JSON, no other text.

Patient Context: {patientContext}
Doctor's Notes: {largeText}

Extract all available fields. Use Universal Numbering System (1-32) for tooth numbers.

JSON format (use exact field names):
{{
  ""allergies"": ""text or null"",
  ""medicalAlerts"": ""text or null"",
  ""diagnosis"": ""text"",
  ""xRayFindings"": ""text or null"",
  ""periodontalStatus"": ""text or null"",
  ""clinicalNotes"": ""text"",
  ""recommendations"": ""text"",
  ""history"": ""text"",
  ""treatments"": ""text"",
  ""medications"": [{{""name"": """", ""dosage"": """", ""frequency"": """", ""duration"": """"}}],
  ""procedures"": [{{""name"": """", ""description"": """", ""date"": ""2024-01-01""}}],
  ""affectedTeeth"": [{{""toothNumber"": 14, ""condition"": """", ""treatment"": """"}}],
  ""xRays"": [{{""type"": """", ""findings"": """", ""date"": ""2024-01-01""}}]
}}

Important:
- toothNumber must be 1-32 (Universal Numbering System)
- Use null for missing fields
- Dates in ISO format (yyyy-MM-dd)

Return JSON only:
###";

            var response = await CallLlamaAsync(prompt, maxTokens: 1500, cancellationToken);
            
            try
            {
                var jsonStart = response.IndexOf('{');
                var jsonEnd = response.LastIndexOf('}');
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonString = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    
                    var options = new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    
                    var result = JsonSerializer.Deserialize<CompleteEHRExtractionResult>(jsonString, options);
                    return result ?? new CompleteEHRExtractionResult();
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error: {ex.Message}");
            }
            
            return new CompleteEHRExtractionResult();
        }

        private async Task<string> CallLlamaAsync(string prompt, int maxTokens = 256, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                model = _model,
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = _temperature,
                    num_predict = maxTokens,
                    top_p = _topP,
                    top_k = _topK,
                    num_ctx = _contextLength,
                    repeat_penalty = _repeatPenalty,
                    num_gpu = _gpuLayers,
                    stop = _stopSequences
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(_llamaApiEndpoint, content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Llama API error: {response.StatusCode} - {await response.Content.ReadAsStringAsync(cancellationToken)}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            return jsonResponse.GetProperty("response").GetString() ?? string.Empty;
        }
    }
}
