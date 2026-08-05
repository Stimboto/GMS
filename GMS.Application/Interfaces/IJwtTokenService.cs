using GMS.Domain.Entities;

namespace GMS.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime Expiration) GenerateToken(User user, string roleName);
}
