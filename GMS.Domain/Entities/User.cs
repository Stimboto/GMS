using GMS.Domain.Common;

namespace GMS.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;

    public string? ProfileImageUrl { get; set; }
    public bool EmailNotificationsEnabled { get; set; } = true;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public ICollection<Grievance> SubmittedGrievances { get; set; } = new List<Grievance>();
    public ICollection<Grievance> AssignedGrievances { get; set; } = new List<Grievance>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
