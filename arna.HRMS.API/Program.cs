using arna.HRMS.Infrastructure.Configuration;
using arna.HRMS.Infrastructure.Configuration.Dependency;
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

            app.UseMiddleware<TestAuthHeaderMiddleware>();
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
