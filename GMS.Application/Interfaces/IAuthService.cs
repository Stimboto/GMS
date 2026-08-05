using GMS.Application.DTOs.Auth;
using GMS.Domain.Entities;

namespace GMS.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
