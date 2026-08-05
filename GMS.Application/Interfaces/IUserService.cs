using GMS.Application.DTOs.Users;

namespace GMS.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> GetUserByIdAsync(int id);
    Task UpdateUserRoleAsync(int id, UpdateUserRoleRequest request, string performedByEmail);
    Task UpdateUserStatusAsync(int id, UpdateUserStatusRequest request, string performedByEmail);
    Task DeleteUserAsync(int id, string performedByEmail);
    Task UpdateProfileImageAsync(int userId, string imageUrl);
    Task UpdatePreferencesAsync(int userId, bool emailNotificationsEnabled);
}
