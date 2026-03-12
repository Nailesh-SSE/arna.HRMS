// ============================================================
// FILE: arna.HRMS.API/Program.cs
// ============================================================
using arna.HRMS.Infrastructure.Dependency;
using arna.HRMS.Infrastructure.Dependency.Identity;

namespace arna.HRMS.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // ── CORS ──────────────────────────────────────────────
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazorApp", policy =>
            {
                policy
                    .WithOrigins(
                        "https://hrms.arnatechnosoft.com",
                        "https://www.hrms.arnatechnosoft.com",
                        "http://hrms.arnatechnosoft.com",
                        "https://hrms-api.arnatechnosoft.com",
                        "https://localhost:7263",   // Blazor dev
                        "http://localhost:5263"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // ── Swagger with JWT support ──────────────────────────
        builder.Services.AddSwaggerWithJwt();

        // ── Database ──────────────────────────────────────────
        builder.Services.AddDatabase(builder.Configuration);

        // ── JWT Authentication ────────────────────────────────
        // Reads JwtSettings from appsettings.json
        // Validates every request with [Authorize] attribute
        builder.Services.AddJwtAuthentication(builder.Configuration);

        // ── All Infrastructure Services (repos, services etc.) 
        builder.Services.AddInfrastructureServices();

        var app = builder.Build();

        // ── Middleware Pipeline ───────────────────────────────
        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowBlazorApp");  // Must be before UseAuthentication

        app.UseAuthentication();        // Reads JWT from Authorization header
        app.UseAuthorization();         // Checks [Authorize] attributes

        app.MapControllers();
        app.Run();
    }
}