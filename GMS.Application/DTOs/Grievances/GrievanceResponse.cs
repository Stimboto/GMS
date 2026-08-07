using GMS.Domain.Enums;

namespace GMS.Application.DTOs.Grievances;

public class GrievanceResponse
{
    public int Id { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public GrievanceStatus Status { get; set; }
    public GrievancePriority Priority { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public string? AssignedOfficer { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public int? SatisfactionRating { get; set; }
    public string? FeedbackRemarks { get; set; }
    
    public List<AttachmentResponse> Attachments { get; set; } = new();
    public List<StatusHistoryResponse> StatusHistories { get; set; } = new();
}
