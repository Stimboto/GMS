namespace GMS.Application.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    string FullName { get; }
    string Email { get; }
    string Role { get; }
    int RoleId { get; }
    bool IsAuthenticated { get; }
}
