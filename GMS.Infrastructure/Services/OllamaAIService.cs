using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using GMS.Application.DTOs.AI;
using GMS.Application.Interfaces;
using GMS.Domain.Entities;
using GMS.Domain.Enums;
using GMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GMS.Infrastructure.Services;

public class OllamaAIService : IAIService
{
    private readonly ILogger<OllamaAIService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly bool _isEnabled;
    private readonly string _modelName;
    private readonly int _timeoutSeconds;
    private readonly HttpClient _httpClient;

    public OllamaAIService(
        IConfiguration configuration,
        ILogger<OllamaAIService> logger,
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        
        var aiConfig = configuration.GetSection("AI");
        _isEnabled = aiConfig.GetValue<bool>("Enabled");
        _modelName = aiConfig.GetValue<string>("Model") ?? "llama3.2:1b";
        _timeoutSeconds = aiConfig.GetValue<int>("TimeoutSeconds", 30);
        var baseUrl = aiConfig.GetValue<string>("BaseUrl") ?? "http://localhost:11434";

        _httpClient = httpClientFactory.CreateClient("OllamaClient");
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
    }

    private async Task<string> GenerateDirectResponseAsync(string prompt, CancellationToken cancellationToken)
    {
        if (!_isEnabled) throw new InvalidOperationException("AI is disabled.");

        var payload = new
        {
            model = _modelName,
            prompt = prompt,
            stream = false,
            options = new { num_predict = 350, temperature = 0.1 }
        };

        var response = await _httpClient.PostAsJsonAsync("/api/generate", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
        if (json.TryGetProperty("response", out var respProp))
        {
            return respProp.GetString()?.Trim() ?? string.Empty;
        }

        return string.Empty;
    }

    private string CalculateTriagePriority(string title, string description)
    {
        var text = (title + " " + description).ToLowerInvariant();

        // Critical Keywords (Life-threatening / Immediate Danger)
        if (text.Contains("fire") || text.Contains("explosion") || text.Contains("electric shock") || 
            text.Contains("live wire") || text.Contains("gas leak") || text.Contains("chemical") || 
            text.Contains("poison") || text.Contains("collapse") || text.Contains("fatal") || 
            text.Contains("death") || text.Contains("life threatening") || text.Contains("electrocution"))
        {
            return "Critical";
        }

        // High Keywords (Urgent Hazards / Major Outages / Accidents)
        if (text.Contains("pothole") || text.Contains("accident") || text.Contains("traffic jam") || 
            text.Contains("sewage") || text.Contains("flooding") || text.Contains("overflow") || 
            text.Contains("hazard") || text.Contains("danger") || text.Contains("open manhole") || 
            text.Contains("hospital") || text.Contains("water outage") || text.Contains("no water") || 
            text.Contains("blackout") || text.Contains("severe") || text.Contains("urgent") || text.Contains("emergency"))
        {
            return "High";
        }

        // Low Keywords (General Inquiries / Feedback / Requests)
        if (text.Contains("inquiry") || text.Contains("question") || text.Contains("feedback") || 
            text.Contains("suggestion") || text.Contains("app issue") || text.Contains("peaceful") || 
            text.Contains("study") || text.Contains("general"))
        {
            return "Low";
        }

        return "Medium";
    }

    public async Task<string> CategorizeGrievanceAsync(string title, string description, CancellationToken cancellationToken = default)
    {
        var prompt = $"Classify this public grievance into exactly one category (Roads, Water Supply, Electricity, Sanitation, Health, Transport, Education, Other). Output ONLY the category name.\nTitle: {title}\nDescription: {description}";
        try
        {
            return await GenerateDirectResponseAsync(prompt, cancellationToken);
        }
        catch
        {
            return "Other";
        }
    }

    public async Task<string> DetectPriorityAsync(string title, string description, CancellationToken cancellationToken = default)
    {
        return CalculateTriagePriority(title, description);
    }

    public async Task<string> GenerateSummaryAsync(string title, string description, CancellationToken cancellationToken = default)
    {
        var prompt = $"Summarize this public grievance in 1 to 2 clear sentences (max 40 words). Focus only on key facts.\nTitle: {title}\nDescription: {description}";
        try
        {
            return await GenerateDirectResponseAsync(prompt, cancellationToken);
        }
        catch
        {
            return description.Length > 120 ? description.Substring(0, 120) : description;
        }
    }

    public async Task<GrievanceAnalysisResultDto> AnalyzeAsync(string title, string description, CancellationToken cancellationToken = default)
    {
        var result = new GrievanceAnalysisResultDto();
        result.Priority = CalculateTriagePriority(title, description);

        // Fetch candidate active grievances (up to 30) for semantic comparison
        var candidateGrievances = new List<Grievance>();
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            candidateGrievances = await db.Grievances
                .Include(g => g.Department)
                .Where(g => g.Status != GrievanceStatus.Closed && g.Status != GrievanceStatus.Resolved)
                .OrderByDescending(g => g.CreatedAt)
                .Take(30)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load candidate grievances for duplicate check.");
        }

        var candidateListText = string.Join("\n", candidateGrievances.Select(g => $"[ID:{g.Id}] Title: {g.Title} | Desc: {g.Description}"));

        var prompt = $@"You are the core AI Engine for a Municipal Grievance Management System.
Analyze the new citizen grievance below.

NEW GRIEVANCE:
Title: {title}
Description: {description}

EXISTING ACTIVE GRIEVANCES:
{(candidateGrievances.Any() ? candidateListText : "None")}

TASK:
1. Provide a concise, professional 1 to 2 sentence summary (30-50 words) of the new grievance. Do NOT copy the description verbatim. Focus on the core problem and requested action.
2. Identify IDs of existing grievances that are SEMANTICALLY SIMILAR (same problem location or issue) with >85% confidence. If none, return empty list [].

Return ONLY a JSON object in this exact format (no markdown codeblocks, no intro):
{{
  ""summary"": ""..."",
  ""similarGrievanceIds"": [12]
}}";

        try
        {
            var jsonText = await GenerateDirectResponseAsync(prompt, cancellationToken);
            var cleanedJson = Regex.Replace(jsonText, @"^```(json)?|```$", "", RegexOptions.Multiline).Trim();

            using var doc = JsonDocument.Parse(cleanedJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("summary", out var sumProp) && !string.IsNullOrWhiteSpace(sumProp.GetString()))
            {
                var s = sumProp.GetString()!.Trim();
                s = Regex.Replace(s, @"(?i)^(summary|the summary is):?\s*", "").Trim();
                if (s.Length > 10) result.Summary = s;
            }

            if (root.TryGetProperty("similarGrievanceIds", out var idsProp) && idsProp.ValueKind == JsonValueKind.Array)
            {
                var matchedIds = new HashSet<int>();
                foreach (var item in idsProp.EnumerateArray())
                {
                    if (item.TryGetInt32(out var idVal)) matchedIds.Add(idVal);
                }

                foreach (var g in candidateGrievances.Where(cg => matchedIds.Contains(cg.Id)))
                {
                    result.SimilarGrievances.Add(new SimilarGrievanceDto
                    {
                        Id = g.Id,
                        TrackingId = g.TrackingId,
                        Title = g.Title,
                        Status = g.Status.ToString(),
                        Department = g.Department?.DepartmentName ?? "General",
                        CreatedAt = g.CreatedAt
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Single AI analysis call failed. Returning fallback.");
        }

        if (string.IsNullOrWhiteSpace(result.Summary))
        {
            result.Summary = description.Length > 120 ? description.Substring(0, 120) : description;
        }

        return result;
    }

    public async Task<AIChatResponse> ChatAsync(AIChatRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new AIChatResponse { Reply = "Hello! How can I assist you with the Grievance Management System today?" };
        }

        string departmentsList = "Roads, Water Supply, Electricity, Sanitation, Health, Transport, Education, General";
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deps = await db.Departments.Select(d => d.DepartmentName).ToListAsync(cancellationToken);
            if (deps.Any()) departmentsList = string.Join(", ", deps);
        }
        catch { }

        var systemInstructions = $@"You are GMS Smart, the official AI assistant of the Grievance Management System.
You help citizens:
- Choose the correct department (Available Departments: {departmentsList})
- Understand grievance statuses (Submitted, Assigned, In Progress, Resolved, Closed, Reopened)
- Explain the grievance filing process and recommended attachments (documents/photos under 5MB)
- Explain Tracking IDs (e.g. GMS-2026-XXXXXX)

STRICT RULES:
1. You NEVER invent government policies or laws.
2. You NEVER perform administrative actions, submit grievances, or modify data.
3. If uncertain or for complex legal disputes, tell the citizen to contact the municipal office.
4. Keep replies helpful, polite, and concise (max 80 words).";

        var chatHistoryPrompt = string.Join("\n", request.History.TakeLast(6).Select(h => $"{h.Sender.ToUpper()}: {h.Text}"));
        var fullPrompt = $"{systemInstructions}\n\nCONVERSATION HISTORY:\n{chatHistoryPrompt}\nUSER: {request.Message}\nASSISTANT:";

        try
        {
            var reply = await GenerateDirectResponseAsync(fullPrompt, cancellationToken);
            if (string.IsNullOrWhiteSpace(reply))
            {
                reply = "I am here to help you navigate the Grievance Management System. Please let me know if you need help choosing a department or tracking your status.";
            }
            return new AIChatResponse { Reply = reply };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GMS Smart chat call failed.");
            return new AIChatResponse { Reply = "I am currently undergoing brief maintenance. Please try again shortly or contact the municipal helpline." };
        }
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
