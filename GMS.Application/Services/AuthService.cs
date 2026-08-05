using GMS.Application.DTOs.Auth;
using GMS.Application.Exceptions;
using GMS.Application.Interfaces;
using GMS.Domain.Entities;
using BCrypt.Net;

namespace GMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Role> _roleRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IGenericRepository<User> userRepository, 
        IGenericRepository<Role> roleRepository,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("Full Name, Email and Password are required.");
        }

        if (request.Password.Length < 6)
        {
            throw new ArgumentException("Password must be at least 6 characters long.");
        }

        var existingUsers = await _userRepository.FindAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (existingUsers.Any())
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        // Get Citizen Role
        var roles = await _roleRepository.FindAsync(r => r.Name == "Citizen");
        var citizenRole = roles.FirstOrDefault();
        
        if (citizenRole == null)
        {
            citizenRole = new Role { Name = "Citizen", CreatedAt = DateTime.UtcNow };
            await _roleRepository.AddAsync(citizenRole);
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            RoleId = citizenRole.Id,
            Role = citizenRole,
            IsActive = true
        };

        await _userRepository.AddAsync(user);

        // Generate Token
        var (token, expiration) = _jwtTokenService.GenerateToken(user, citizenRole.Name);

        return new AuthResponse
        {
            Token = token,
            Expiration = expiration,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = citizenRole.Name,
            ProfileImageUrl = user.ProfileImageUrl,
            EmailNotificationsEnabled = user.EmailNotificationsEnabled
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and Password are required.");
        }

        var users = await _userRepository.FindAsync(u => u.Email.ToLower() == request.Email.ToLower());
        var user = users.FirstOrDefault();

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is inactive.");
        }

        // Fetch user's role
        var role = await _roleRepository.GetByIdAsync(user.RoleId);
        var roleName = role?.Name ?? "Citizen";

        var (token, expiration) = _jwtTokenService.GenerateToken(user, roleName);

        return new AuthResponse
        {
            Token = token,
            Expiration = expiration,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = roleName,
            ProfileImageUrl = user.ProfileImageUrl,
            EmailNotificationsEnabled = user.EmailNotificationsEnabled
        };
    }
}
