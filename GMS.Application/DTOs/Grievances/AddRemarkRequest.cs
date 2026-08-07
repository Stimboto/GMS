using Microsoft.AspNetCore.Http;

namespace GMS.Application.DTOs.Grievances;

public class AddRemarkRequest
{
    public string? Remarks { get; set; }
    public IFormFile? File { get; set; }
    public bool IsInternal { get; set; }
}
