using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace arna.HRMS.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public JwtService(IConfiguration configuration, ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("FullName", user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("EmployeeId", user.EmployeeId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var jwtSettings = _configuration.GetSection("JwtSettings");

        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;

        var accessTokenMinutes =
            int.Parse(jwtSettings["AccessTokenExpirationInMinutes"]!);

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey));

        var expires = DateTime.UtcNow.AddMinutes(accessTokenMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public async Task SaveRefreshTokenAsync(int userId, string refreshToken)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return;

        var refreshMinutes = int.Parse(
            _configuration["JwtSettings:RefreshTokenExpirationInMinutes"]!);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(refreshMinutes);

        await _context.SaveChangesAsync();
    }

    public async Task RevokeRefreshTokenAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return;

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await _context.SaveChangesAsync();
    }
}
