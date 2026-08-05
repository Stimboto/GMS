namespace GMS.Application.Interfaces;

public interface IOllamaService
{
    Task AnalyzeGrievanceAsync(int grievanceId, string description);
}
