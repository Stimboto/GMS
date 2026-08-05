using GMS.Application.Interfaces;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GMS.Infrastructure.Services;

public class OllamaAIService : IAIService
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<OllamaAIService> _logger;
    private readonly bool _isEnabled;
    private readonly string _modelName;
    private readonly int _timeoutSeconds;
    private readonly HttpClient _httpClient; // kept for health check

    public OllamaAIService(
        IConfiguration configuration,
        ILogger<OllamaAIService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        
        var aiConfig = configuration.GetSection("AI");
        _isEnabled = aiConfig.GetValue<bool>("Enabled");
        _modelName = aiConfig.GetValue<string>("Model") ?? "llama3.2";
        _timeoutSeconds = aiConfig.GetValue<int>("TimeoutSeconds", 30);
        var baseUrl = aiConfig.GetValue<string>("BaseUrl") ?? "http://localhost:11434";

        _httpClient = httpClientFactory.CreateClient("OllamaClient");
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);

        if (_isEnabled)
        {
            _chatClient = new OllamaChatClient(new Uri(baseUrl), _modelName, _httpClient);
        }
        else
        {
            _chatClient = null!; // Won't be used if disabled
        }
    }

    private async Task<string> GenerateResponseAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
    {
        if (!_isEnabled)
        {
            _logger.LogInformation("AI is disabled. Skipping request.");
            throw new InvalidOperationException("AI is disabled.");
        }

        var sw = Stopwatch.StartNew();
        _logger.LogInformation("AI request started.");

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, userPrompt)
            };

            // Limit generation to avoid long waits and unexpected formats
            var options = new ChatOptions { MaxOutputTokens = 100, Temperature = 0.0f };
            
            var response = await _chatClient.GetResponseAsync(messages, options, cts.Token);
            
            sw.Stop();
            _logger.LogInformation($"AI completed in {sw.ElapsedMilliseconds}ms.");
            
            var content = response.Text?.Trim() ?? string.Empty;
            return content;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, $"AI failed after {sw.ElapsedMilliseconds}ms.");
            throw; // Let the caller handle the fallback
        }
    }

    public async Task<string> CategorizeGrievanceAsync(string title, string description, CancellationToken cancellationToken = default)
    {
        var systemPrompt = @"You are a strict classification engine.
Your task is to classify the following public grievance into EXACTLY ONE of the following categories:
Roads, Water Supply, Electricity, Sanitation, Health, Transport, Education, Other.

Rules:
1. Output ONLY the category name.
2. NO explanations.
3. NO markdown, NO bullet points, NO extra text.
4. If you cannot decide, output 'Other'.";

        var userPrompt = $"Title: {title}\nDescription: {description}";

        var result = await GenerateResponseAsync(systemPrompt, userPrompt, cancellationToken);
        
        var validCategories = new[] { "Roads", "Water Supply", "Electricity", "Sanitation", "Health", "Transport", "Education", "Other" };
        if (!validCategories.Any(c => c.Equals(result, StringComparison.OrdinalIgnoreCase)))
        {
            return "Other";
        }

        return result;
    }

    public async Task<string> DetectPriorityAsync(string title, string description, CancellationToken cancellationToken = default)
    {
        var systemPrompt = @"You are a strict priority detection engine.
Your task is to assign a priority to the following public grievance from exactly one of these values:
Critical, High, Medium, Low.

Rules:
1. Output ONLY the priority value.
2. NO explanations.
3. NO markdown, NO bullet points.
4. If you cannot decide, output 'Medium'.";

        var userPrompt = $"Title: {title}\nDescription: {description}";

        var result = await GenerateResponseAsync(systemPrompt, userPrompt, cancellationToken);

        var validPriorities = new[] { "Critical", "High", "Medium", "Low" };
        if (!validPriorities.Any(c => c.Equals(result, StringComparison.OrdinalIgnoreCase)))
        {
            return "Medium";
        }

        return result;
    }

    public async Task<string> GenerateSummaryAsync(string title, string description, CancellationToken cancellationToken = default)
    {
        var systemPrompt = @"You are a strict summarization engine.
Your task is to summarize the following public grievance.

Rules:
1. Output a plain text summary in maximum 80 words.
2. NO explanations, NO intro text like 'Here is the summary'.
3. NO markdown, NO bullet points.";

        var userPrompt = $"Title: {title}\nDescription: {description}";

        var result = await GenerateResponseAsync(systemPrompt, userPrompt, cancellationToken);
        return result;
    }

    public async Task<object> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = new
        {
            AIEnabled = _isEnabled,
            Model = _modelName,
            ConnectionStatus = "Unknown",
            ResponseTimeMs = 0L
        };

        if (!_isEnabled)
        {
            return new { status.AIEnabled, status.Model, ConnectionStatus = "Disabled", ResponseTimeMs = 0 };
        }

        try
        {
            var sw = Stopwatch.StartNew();
            // Ping the Ollama API directly for health check
            var response = await _httpClient.GetAsync("/api/tags", cancellationToken);
            sw.Stop();

            if (response.IsSuccessStatusCode)
            {
                return new { status.AIEnabled, status.Model, ConnectionStatus = "Connected", ResponseTimeMs = sw.ElapsedMilliseconds };
            }
            else
            {
                return new { status.AIEnabled, status.Model, ConnectionStatus = "Error", ResponseTimeMs = sw.ElapsedMilliseconds };
            }
        }
        catch (Exception)
        {
            return new { status.AIEnabled, status.Model, ConnectionStatus = "Offline", ResponseTimeMs = -1 };
        }
    }
}
