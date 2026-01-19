using arna.HRMS.Infrastructure.Mapping;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using arna.HRMS.Infrastructure.Repositories.Common.Interfaces;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Infrastructure.Services.Authentication;
using arna.HRMS.Infrastructure.Services.Authentication.Interfaces;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace arna.HRMS.Infrastructure.Configuration.Dependency;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(UserProfile).Assembly);

        // Base repository
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        // Services
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IUserServices, UserServices>();
        services.AddScoped<IAttendanceRequestService, AttendanceRequestService>();
        services.AddScoped<IFestivalHolidayService, FestivalHolidayService>();

        // Repositories
        services.AddScoped<DepartmentRepository>();
        services.AddScoped<EmployeeRepository>();
        services.AddScoped<AttendanceRepository>();
        services.AddScoped<UserRepository>();
        services.AddScoped<AttendanceRequestRepository>();
        services.AddScoped<FestivalHolidayRepository>();

        // Auth Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}
