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
        // Issue 15 FIX: Only one AddAutoMapper call needed — scans entire assembly for all Profile classes
        services.AddAutoMapper(typeof(AttendanceProfile).Assembly);

        // Base repository
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        // Services
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IUserServices, UserServices>();
        services.AddScoped<IAttendanceRequestService, AttendanceRequestService>();
        services.AddScoped<IFestivalHolidayService, FestivalHolidayService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IReportService, ReportService>();

        // Repositories
        services.AddScoped<DashboardRepository>();
        services.AddScoped<DepartmentRepository>();
        services.AddScoped<EmployeeRepository>();
        services.AddScoped<AttendanceRepository>();
        services.AddScoped<UserRepository>();
        services.AddScoped<AttendanceRequestRepository>();
        services.AddScoped<FestivalHolidayRepository>();
        services.AddScoped<LeaveRepository>();
        services.AddScoped<RoleRepository>();
        services.AddScoped<ReportRepository>();

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
