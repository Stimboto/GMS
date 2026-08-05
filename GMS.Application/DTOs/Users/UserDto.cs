namespace GMS.Application.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    
    // Statistics
    public int TotalGrievances { get; set; }
    public int ResolvedGrievances { get; set; }
    public int AssignedCases { get; set; }
}
