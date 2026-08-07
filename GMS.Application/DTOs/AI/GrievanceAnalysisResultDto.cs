namespace GMS.Application.DTOs.AI;

public class SimilarGrievanceDto
{
    public int Id { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GrievanceAnalysisResultDto
{
    public string Priority { get; set; } = "Medium";
    public string Summary { get; set; } = string.Empty;
    public List<SimilarGrievanceDto> SimilarGrievances { get; set; } = new();
}

public class AIChatRequest
{
    public string Message { get; set; } = string.Empty;
    public List<AIChatMessageDto> History { get; set; } = new();
}

public class AIChatMessageDto
{
    public string Sender { get; set; } = "user"; // "user" or "bot"
    public string Text { get; set; } = string.Empty;
}

public class AIChatResponse
{
    public string Reply { get; set; } = string.Empty;
}
