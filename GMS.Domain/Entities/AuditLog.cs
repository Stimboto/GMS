using GMS.Domain.Common;

namespace GMS.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedOn { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
