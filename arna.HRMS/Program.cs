using arna.HRMS.ClientServices.Admin.Department;
using arna.HRMS.ClientServices.Admin.Employee;
using arna.HRMS.ClientServices.Admin.Role;
using arna.HRMS.ClientServices.Admin.User;
using arna.HRMS.ClientServices.Auth;
using arna.HRMS.ClientServices.Client.Attendance;
using arna.HRMS.ClientServices.Client.AttendanceRequest;
using arna.HRMS.ClientServices.Client.Clock;
using arna.HRMS.ClientServices.Common;
using arna.HRMS.ClientServices.Http;
using arna.HRMS.ClientServices.Leave;
using arna.HRMS.Components;
using arna.HRMS.Models.State.Attendance;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace arna.HRMS;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Razor Components
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // ==========================
        // HttpClient Configuration
        // ==========================
        builder.Services.AddHttpClient("Default", client =>
        {
            var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("ApiSettings:BaseUrl is missing in appsettings.json");

            client.BaseAddress = new Uri(baseUrl);
        });

        // Default HttpClient
        builder.Services.AddScoped(sp =>
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("Default")
        );

        // ==========================
        // Authentication (Blazor)
        // ==========================
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddScoped<ProtectedLocalStorage>();
        builder.Services.AddScoped<CustomAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<CustomAuthStateProvider>());

        // ==========================
        // App Services
        // ==========================
        builder.Services.AddScoped<NotificationService>();

        builder.Services.AddScoped<HttpService>();
        builder.Services.AddScoped<ApiClients>(sp =>
        {
            var httpService = sp.GetRequiredService<HttpService>();
            var apiClients = new ApiClients(httpService);
            httpService.SetApiClients(apiClients);

            return apiClients;
        });

        // ==========================
        // Feature Services
        // ==========================
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IEmployeeService, EmployeeService>();
        builder.Services.AddScoped<IDepartmentService, DepartmentService>();
        builder.Services.AddScoped<IClockService, ClockService>();
        builder.Services.AddScoped<IUsersService, UsersService>();
        builder.Services.AddScoped<IAttendanceService, AttendanceService>();
        builder.Services.AddScoped<IAttendanceRequestService, AttendanceRequestService>();
        builder.Services.AddScoped<ILeaveService, LeaveService>();
        builder.Services.AddScoped<IRoleService, RoleService>();

        builder.Services.AddScoped<AttendanceState>();

        // Other
        builder.Services.AddMemoryCache();
        builder.Services.AddBlazorBootstrap();

        var app = builder.Build();

        // ==========================
        // Middleware
        // ==========================
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
