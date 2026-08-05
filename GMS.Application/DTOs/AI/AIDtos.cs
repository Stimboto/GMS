namespace GMS.Application.DTOs.AI;

public class AITestRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AIResponse
{
    public string Result { get; set; } = string.Empty;
}
