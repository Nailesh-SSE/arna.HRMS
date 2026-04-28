using arna.HRMS.Infrastructure.Dependency;
using arna.HRMS.Infrastructure.Dependency.Identity;
using arna.HRMS.Infrastructure.Middleware;

namespace arna.HRMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

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
                          "https://www.hrms-api.arnatechnosoft.com",
                          "http://hrms-api.arnatechnosoft.com"
                      )
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
                });
            });

            builder.Services.AddSwaggerWithJwt();
            builder.Services.AddDatabase(builder.Configuration);
            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.Services.AddInfrastructureServices();

            var app = builder.Build();

            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // ✅ FIX #1: CORS MUST come BEFORE UseHttpsRedirection
            // Otherwise preflight OPTIONS requests get redirected (301) before CORS headers
            // are applied → browser treats as CORS failure → appears as Unauthorized
            app.UseCors("AllowBlazorApp");

            app.UseHttpsRedirection();

            // ✅ FIX #2: TestAuthHeaderMiddleware REMOVED from production
            // The static TestTokenStore.Token is shared across ALL users on the server
            // causing cross-user token injection. Only use in Development.
            if (app.Environment.IsDevelopment())
            {
                app.UseMiddleware<TestAuthHeaderMiddleware>();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}