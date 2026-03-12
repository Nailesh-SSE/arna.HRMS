// ============================================================
// FILE: arna.HRMS.Infrastructure/Services/JwtService.cs
// ============================================================
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Dependency.Identity;
using arna.HRMS.Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace arna.HRMS.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserRepository _userRepository;

    public JwtService(IOptions<JwtSettings> jwtSettings, UserRepository userRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _userRepository = userRepository;
    }

    // ----------------------------------------------------------
    // Generate a short-lived Access Token (JWT)
    // Contains: UserId, Username, Email, Role, FullName, EmployeeId
    // ----------------------------------------------------------
    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.Username  ?? string.Empty),
            new(ClaimTypes.Email,          user.Email     ?? string.Empty),
            new(ClaimTypes.Role,           user.Role?.Name ?? string.Empty),
            new("FullName",                user.FullName  ?? string.Empty),
            new("EmployeeId",              user.EmployeeId?.ToString() ?? "0"),
            new("RoleId",                  user.RoleId.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ----------------------------------------------------------
    // Generate a long-lived Refresh Token (random bytes, not JWT)
    // ----------------------------------------------------------
    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    // ----------------------------------------------------------
    // Persist the Refresh Token to the database for the user
    // ----------------------------------------------------------
    public async Task SaveRefreshTokenAsync(int userId, string refreshToken)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) return;

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

        await _userRepository.UpdateUserAsync(user);
    }

    // ----------------------------------------------------------
    // Revoke the Refresh Token (on logout)
    // ----------------------------------------------------------
    public async Task RevokeRefreshTokenAsync(int userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) return;

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await _userRepository.UpdateUserAsync(user);
    }
}