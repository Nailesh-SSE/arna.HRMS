using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace arna.HRMS.Infrastructure.Dependency.Identity;

public static class JwtAuthConfig
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings section is missing in appsettings.json");

        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
            throw new InvalidOperationException("JwtSettings:SecretKey is missing in appsettings.json");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
                    ),

                    // ✅ FIX: Changed from TimeSpan.Zero to 2 minutes
                    // ClockSkew.Zero means ZERO tolerance for clock difference between servers.
                    // On separate production servers, even a 1-second drift causes valid tokens
                    // to be rejected with 401 Unauthorized.
                    // 2 minutes is the standard safe tolerance for distributed systems.
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });

        services.AddAuthorization();

        return services;
    }
}