using GMS.Application.DTOs.Users;
using GMS.Application.Exceptions;
using GMS.Application.Exceptions;
using GMS.Application.Interfaces;
using GMS.Domain.Entities;
using GMS.Domain.Enums;

namespace GMS.Application.Services;

public class UserService : IUserService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Role> _roleRepository;
    private readonly IGrievanceRepository _grievanceRepository;
    private readonly IAuditLogService _auditLogService;

    public UserService(
        IGenericRepository<User> userRepository,
        IGenericRepository<Role> roleRepository,
        IGrievanceRepository grievanceRepository,
        IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _grievanceRepository = grievanceRepository;
        _auditLogService = auditLogService;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var roles = await _roleRepository.GetAllAsync();
        var grievances = await _grievanceRepository.GetAllAsync();

        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Role = roles.FirstOrDefault(r => r.Id == u.RoleId)?.Name ?? "Unknown",
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            ProfileImageUrl = u.ProfileImageUrl,
            EmailNotificationsEnabled = u.EmailNotificationsEnabled,
            TotalGrievances = grievances.Count(g => g.SubmittedByUserId == u.Id),
            ResolvedGrievances = grievances.Count(g => g.SubmittedByUserId == u.Id && g.Status == GrievanceStatus.Resolved),
            AssignedCases = grievances.Count(g => g.AssignedOfficerId == u.Id)
        }).ToList();

        return userDtos;
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException("User", id);

        var role = await _roleRepository.GetByIdAsync(user.RoleId);
        var grievances = await _grievanceRepository.GetAllAsync();

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = role?.Name ?? "Unknown",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            ProfileImageUrl = user.ProfileImageUrl,
            EmailNotificationsEnabled = user.EmailNotificationsEnabled,
            TotalGrievances = grievances.Count(g => g.SubmittedByUserId == user.Id),
            ResolvedGrievances = grievances.Count(g => g.SubmittedByUserId == user.Id && g.Status == GrievanceStatus.Resolved),
            AssignedCases = grievances.Count(g => g.AssignedOfficerId == user.Id)
        };
    }

    public async Task UpdateUserRoleAsync(int id, UpdateUserRoleRequest request, string performedByEmail)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new NotFoundException("User", id);

        if (user.Email.ToLower() == "admin@gms.com")
            throw new InvalidOperationException("The Bootstrap Admin role cannot be changed.");

        var oldRoleObj = await _roleRepository.GetByIdAsync(user.RoleId);
        var oldRoleName = oldRoleObj?.Name;

        if (oldRoleName == "Admin" && request.Role != "Admin")
        {
            var adminRole = (await _roleRepository.FindAsync(r => r.Name == "Admin")).FirstOrDefault();
            if (adminRole != null)
            {
                var activeAdmins = (await _userRepository.FindAsync(u => u.RoleId == adminRole.Id && u.IsActive)).Count();
                if (activeAdmins <= 1)
                    throw new InvalidOperationException("Cannot demote the last active Admin.");
            }
        }

        var newRole = (await _roleRepository.FindAsync(r => r.Name == request.Role)).FirstOrDefault();
        if (newRole == null) throw new ArgumentException("Invalid role specified.");

        user.RoleId = newRole.Id;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _auditLogService.LogAsync("Role Updated", performedByEmail, oldRoleName, newRole.Name);
    }

    public async Task UpdateUserStatusAsync(int id, UpdateUserStatusRequest request, string performedByEmail)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new NotFoundException("User", id);

        if (user.Email.ToLower() == "admin@gms.com")
            throw new InvalidOperationException("The Bootstrap Admin status cannot be changed.");

        var oldStatus = user.IsActive.ToString();

        if (user.IsActive && !request.IsActive)
        {
            var adminRole = (await _roleRepository.FindAsync(r => r.Name == "Admin")).FirstOrDefault();
            if (adminRole != null && user.RoleId == adminRole.Id)
            {
                var activeAdmins = (await _userRepository.FindAsync(u => u.RoleId == adminRole.Id && u.IsActive)).Count();
                if (activeAdmins <= 1)
                    throw new InvalidOperationException("Cannot deactivate the last active Admin.");
            }
        }

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _auditLogService.LogAsync("Status Updated", performedByEmail, oldStatus, request.IsActive.ToString());
    }

    public async Task DeleteUserAsync(int id, string performedByEmail)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new NotFoundException("User", id);

        if (user.Email.ToLower() == "admin@gms.com")
            throw new InvalidOperationException("The Bootstrap Admin cannot be deleted.");

        var adminRole = (await _roleRepository.FindAsync(r => r.Name == "Admin")).FirstOrDefault();
        if (adminRole != null && user.RoleId == adminRole.Id)
        {
            var activeAdmins = (await _userRepository.FindAsync(u => u.RoleId == adminRole.Id && u.IsActive && !u.IsDeleted)).Count();
            if (activeAdmins <= 1)
                throw new InvalidOperationException("Cannot delete the last active Admin.");
        }

        user.IsDeleted = true;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        await _auditLogService.LogAsync("User Deleted", performedByEmail, user.Email, "Deleted");
    }

    public async Task UpdateProfileImageAsync(int userId, string imageUrl)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException(nameof(User), userId);

        user.ProfileImageUrl = imageUrl;
        await _userRepository.UpdateAsync(user);
    }

    public async Task UpdatePreferencesAsync(int userId, bool emailNotificationsEnabled)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException(nameof(User), userId);

        user.EmailNotificationsEnabled = emailNotificationsEnabled;
        await _userRepository.UpdateAsync(user);
    }
}
