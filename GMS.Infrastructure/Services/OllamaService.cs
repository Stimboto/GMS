using System.Text;
using System.Text.Json;
using GMS.Application.Interfaces;
using GMS.Domain.Entities;
using GMS.Domain.Enums;
using GMS.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GMS.Infrastructure.Services;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OllamaService> _logger;
    private readonly IConfiguration _configuration;

    public OllamaService(HttpClient httpClient, IServiceScopeFactory scopeFactory, ILogger<OllamaService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task AnalyzeGrievanceAsync(int grievanceId, string description)
    {
        try
        {
            var prompt = $@"
Analyze the following grievance description and provide a JSON response with three fields:
- Category (string: e.g., 'Maintenance', 'Billing', 'IT Support', 'HR', 'General')
- Priority (string: must be exactly 'Low', 'Medium', 'High', or 'Critical')
- Summary (string: a concise 1-2 sentence summary of the issue)

Description: {description}

Return ONLY valid JSON.
";

            var baseUrl = _configuration["AI:BaseUrl"] ?? "http://localhost:11434";
            var modelName = _configuration["AI:Model"] ?? "llama3.1";

            var requestBody = new
            {
                model = modelName,
                prompt = prompt,
                stream = false,
                format = "json"
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{baseUrl}/api/generate", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!string.IsNullOrEmpty(ollamaResponse?.Response))
                {
                    var analysis = JsonSerializer.Deserialize<GrievanceAnalysis>(ollamaResponse.Response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (analysis != null)
                    {
                        // Use a new scope to update the database since this runs in the background
                        using var scope = _scopeFactory.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var grievance = await dbContext.Grievances.FindAsync(grievanceId);
                        if (grievance != null)
                        {
                            grievance.Category = analysis.Category ?? grievance.Category;
                            grievance.Summary = analysis.Summary ?? grievance.Summary;

                            if (Enum.TryParse<GrievancePriority>(analysis.Priority, true, out var priority))
                            {
                                grievance.Priority = priority;
                            }

                            await dbContext.SaveChangesAsync();
                            _logger.LogInformation("Grievance {Id} analyzed by Ollama successfully.", grievanceId);
                            
                            var notifier = scope.ServiceProvider.GetRequiredService<IRealTimeNotifier>();
                            await notifier.NotifyUserAsync(grievance.SubmittedByUserId, "AI Analysis Complete", $"Grievance '{grievance.TrackingId}' has been analyzed by AI and categorized as {grievance.Category}.");
                            await notifier.NotifyRoleAsync("Admin", "Grievance AI Analysis Complete", $"Grievance '{grievance.TrackingId}' analysis finished. Priority: {grievance.Priority}.");
                        }
                    }
                }
            }
            else
            {
                _logger.LogWarning("Failed to reach Ollama API. Status Code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during Ollama analysis for Grievance {Id}", grievanceId);
        }
    }

    private class OllamaResponse
    {
        public string Response { get; set; } = string.Empty;
    }

    private class GrievanceAnalysis
    {
        public string? Category { get; set; }
        public string? Priority { get; set; }
        public string? Summary { get; set; }
    }
}
