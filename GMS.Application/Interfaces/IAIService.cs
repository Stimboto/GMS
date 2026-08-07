using GMS.Application.DTOs.AI;

namespace GMS.Application.Interfaces;

public interface IAIService
{
    Task<string> CategorizeGrievanceAsync(string title, string description, CancellationToken cancellationToken = default);
    Task<string> DetectPriorityAsync(string title, string description, CancellationToken cancellationToken = default);
    Task<string> GenerateSummaryAsync(string title, string description, CancellationToken cancellationToken = default);
    Task<GrievanceAnalysisResultDto> AnalyzeAsync(string title, string description, CancellationToken cancellationToken = default);
    Task<AIChatResponse> ChatAsync(AIChatRequest request, CancellationToken cancellationToken = default);
    Task<object> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}
