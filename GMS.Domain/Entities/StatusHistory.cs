using GMS.Domain.Common;
using GMS.Domain.Enums;

namespace GMS.Domain.Entities;

public class StatusHistory : BaseEntity
{
    public GrievanceStatus OldStatus { get; set; }
    public GrievanceStatus NewStatus { get; set; }
    public string? Remarks { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsInternal { get; set; } = false;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public int? AttachmentId { get; set; }
    public Attachment? Attachment { get; set; }

    public int ChangedByUserId { get; set; }
    public User ChangedByUser { get; set; } = null!;

    public int GrievanceId { get; set; }
    public Grievance Grievance { get; set; } = null!;
}
