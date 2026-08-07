using GMS.Domain.Enums;

namespace GMS.Application.DTOs.Grievances;

public class StatusHistoryResponse
{
    public int Id { get; set; }
    public GrievanceStatus OldStatus { get; set; }
    public GrievanceStatus NewStatus { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public bool IsInternal { get; set; }
    public DateTime ChangedAt { get; set; }
    public int ChangedByUserId { get; set; }
    public string ChangedByUserName { get; set; } = string.Empty;
}
