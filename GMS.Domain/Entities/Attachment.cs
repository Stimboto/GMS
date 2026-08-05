using GMS.Domain.Common;

namespace GMS.Domain.Entities;

public class Attachment : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int GrievanceId { get; set; }
    public Grievance Grievance { get; set; } = null!;
}
