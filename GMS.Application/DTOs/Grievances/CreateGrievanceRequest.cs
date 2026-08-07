using Microsoft.AspNetCore.Http;

namespace GMS.Application.DTOs.Grievances;

public class CreateGrievanceRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public IFormFile? File { get; set; }
}
