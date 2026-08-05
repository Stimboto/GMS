namespace GMS.Application.DTOs.Grievances;

public class UpdateGrievanceRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string Category { get; set; } = string.Empty;
}
