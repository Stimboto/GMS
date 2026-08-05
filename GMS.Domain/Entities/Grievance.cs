using GMS.Domain.Common;
using GMS.Domain.Enums;

namespace GMS.Domain.Entities;

public class Grievance : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public GrievanceStatus Status { get; set; } = GrievanceStatus.Submitted;
    public GrievancePriority Priority { get; set; } = GrievancePriority.Low;
    public string Category { get; set; } = string.Empty;
    
    // Enterprise Features
    public string TrackingId { get; set; } = string.Empty;
    public int? SatisfactionRating { get; set; }
    public string? FeedbackRemarks { get; set; }

    public int SubmittedByUserId { get; set; }
    public User SubmittedByUser { get; set; } = null!;

    public int? AssignedOfficerId { get; set; }
    public User? AssignedOfficer { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();
}
