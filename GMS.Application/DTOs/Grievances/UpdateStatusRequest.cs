using GMS.Domain.Enums;

namespace GMS.Application.DTOs.Grievances;

public class UpdateStatusRequest
{
    public GrievanceStatus Status { get; set; }
    public string? Remarks { get; set; }
    public string? ImageUrl { get; set; }
}
