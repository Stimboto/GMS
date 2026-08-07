using Microsoft.AspNetCore.Http;
using GMS.Domain.Enums;

namespace GMS.Application.DTOs.Grievances;

public class UpdateStatusRequest
{
    public GrievanceStatus Status { get; set; }
    public string? Remarks { get; set; }
    public IFormFile? File { get; set; }
}
