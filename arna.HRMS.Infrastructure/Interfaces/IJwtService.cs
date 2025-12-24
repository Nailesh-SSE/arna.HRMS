using arna.HRMS.Core.Entities;

namespace arna.HRMS.Infrastructure.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken);
    Task SaveRefreshTokenAsync(int userId, string refreshToken);
    Task RevokeRefreshTokenAsync(int userId);
}
