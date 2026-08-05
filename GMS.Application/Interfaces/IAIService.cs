namespace GMS.Application.Interfaces;

public interface IAIService
{
    Task<string> CategorizeGrievanceAsync(string title, string description, CancellationToken cancellationToken = default);
    Task<string> DetectPriorityAsync(string title, string description, CancellationToken cancellationToken = default);
    Task<string> GenerateSummaryAsync(string title, string description, CancellationToken cancellationToken = default);
    Task<object> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}
