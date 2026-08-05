using GMS.Domain.Common;

namespace GMS.Domain.Entities;

public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
