using arna.HRMS.Core.Interfaces.Repository;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Mapping;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Infrastructure.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace arna.HRMS.Infrastructure.Dependency;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(AttendanceProfile).Assembly);
        services.AddAutoMapper(typeof(AttendanceRequestProfile).Assembly);
        services.AddAutoMapper(typeof(DepartmentProfile).Assembly);
        services.AddAutoMapper(typeof(EmployeeProfile).Assembly);
        services.AddAutoMapper(typeof(FestivalHolidayProfile).Assembly);
        services.AddAutoMapper(typeof(LeaveProfile).Assembly);
        services.AddAutoMapper(typeof(UserProfile).Assembly);
        services.AddAutoMapper(typeof(RoleProfile).Assembly);

        // Base repository
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        // Services
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IUserServices, UserServices>();
        services.AddScoped<IAttendanceRequestService, AttendanceRequestService>();
        services.AddScoped<IFestivalHolidayService, FestivalHolidayService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<IRoleService, RoleService>();

        // Repositories
        services.AddScoped<DepartmentRepository>();
        services.AddScoped<EmployeeRepository>();
        services.AddScoped<AttendanceRepository>();
        services.AddScoped<UserRepository>();
        services.AddScoped<AttendanceRequestRepository>();
        services.AddScoped<FestivalHolidayRepository>();
        services.AddScoped<LeaveRepository>();
        services.AddScoped<RoleRepository>();

        // Auth Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();

        // Validators
        services.AddScoped<UserValidator>();
        services.AddScoped<RoleValidator>();
        services.AddScoped<EmployeeValidator>();
        services.AddScoped<DepartmentValidator>();
        services.AddScoped<FestivalHolidayValidator>();
        services.AddScoped<LeaveValidator>();
        services.AddScoped<AttendanceRequestValidator>();
        services.AddScoped<AttendanceValidator>();

        return services;
    }
}
