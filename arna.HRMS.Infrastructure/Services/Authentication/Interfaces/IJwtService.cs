using arna.HRMS.Core.Entities;

namespace arna.HRMS.Infrastructure.Services.Authentication.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Task SaveRefreshTokenAsync(int userId, string refreshToken);
    Task RevokeRefreshTokenAsync(int userId);
}
