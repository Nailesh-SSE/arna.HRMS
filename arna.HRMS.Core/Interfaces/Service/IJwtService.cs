using arna.HRMS.Core.Entities;

namespace arna.HRMS.Core.Interfaces.Service;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Task SaveRefreshTokenAsync(int userId, string refreshToken);
    Task RevokeRefreshTokenAsync(int userId);
}
