using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Handler;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Models.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace arna.HRMS.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add HttpClient if needed
        builder.Services.AddScoped(sp =>
            new HttpClient
            {
                BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"])
            });

        // JWT Authentication Configuration
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured in appsettings.json");

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"] ?? "EmployeeManagementAPI",
                ValidAudience = jwtSettings["Audience"] ?? "EmployeeManagementUsers",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Add Authorization
        builder.Services.AddAuthorization();

        // Add controllers & Swagger
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Configure Swagger to include JWT Authentication
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title = "Employee Management API",
                Version = "v1",
                Description = "API for managing employees with JWT authentication and role-based authorization"
            });

            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: &quot;Authorization: Bearer {token}&quot;",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // ✅ AutoMapper INLINE configuration (NO extra files)
        builder.Services.AddAutoMapper(cfg =>
        {
            // INSERT
            cfg.CreateMap<CreateEmployeeRequest, Employee>();

            // UPDATE and ".ForMember(dest => dest.ManagerId, opt => opt.Ignore());" is for testing because it not accept ManagerID as null or 0
            cfg.CreateMap<UpdateEmployeeRequest, Employee>(); 

            // RESPONSE
            cfg.CreateMap<Employee, EmployeeDto>();
        });

        builder.Services.AddAutoMapper(cfg =>
        {
            // INSERT
            cfg.CreateMap<CreateDepartmentRequest, Department>();

            // UPDATE 
            cfg.CreateMap<UpdateDepartmentRequest, Department>();

            // RESPONSE
            cfg.CreateMap<Department, DepartmentDto>();
        });

        builder.Services.AddAutoMapper(cfg =>
        {
            // INSERT
            cfg.CreateMap<CreateUserRequest, User>();

            // UPDATE and ".ForMember(dest => dest.ManagerId, opt => opt.Ignore());" is for testing because it not accept ManagerID as null or 0
            cfg.CreateMap<UpdateUserRequest, User>();

            // RESPONSE
            cfg.CreateMap<User, UserDto>();
        });

        // Register DbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly("arna.HRMS.Infrastructure")
            )
        );


        // Register repositories & services
        builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        builder.Services.AddScoped<IEmployeeService, EmployeeService>();
        builder.Services.AddScoped<IDepartmentService, DepartmentService>();
        builder.Services.AddScoped<IUserServices, UserServices>();
        builder.Services.AddScoped<DepartmentRepository>();
        builder.Services.AddScoped<EmployeeRepository>();
        builder.Services.AddScoped<UserRepository>();

        // Register Authentication & JWT Services
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IJwtService, JwtService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
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
