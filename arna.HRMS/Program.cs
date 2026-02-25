using arna.HRMS.ClientServices.Admin.Department;
using arna.HRMS.ClientServices.Admin.Employee;
using arna.HRMS.ClientServices.Admin.FestivalHolidays;
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

namespace arna.HRMS;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ==========================
        // Razor Components
        // ==========================
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // ==========================
        // Authentication (Blazor)
        // ==========================
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();

        // Add MemoryCache
        builder.Services.AddMemoryCache();

        // Add logging
        builder.Services.AddLogging();

        // Register CustomAuthStateProvider
        builder.Services.AddScoped<CustomAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<CustomAuthStateProvider>());

        // ==========================
        // HTTP CLIENTS
        // ==========================

        var baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                      ?? throw new Exception("ApiSettings:BaseUrl not configured");

        // Register AuthHeaderHandler
        builder.Services.AddScoped<AuthHeaderHandler>();

        // Authorized Client with token handler
        builder.Services.AddHttpClient("AuthorizedClient", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromHours(2);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddHttpMessageHandler<AuthHeaderHandler>()
        .SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Prevent handler caching issues

        // Refresh Client (no token handler)
        builder.Services.AddHttpClient("RefreshClient", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Default injected HttpClient = AuthorizedClient
        builder.Services.AddScoped(sp =>
            sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient("AuthorizedClient"));

        // ==========================
        // Core App Services
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
        builder.Services.AddScoped<IFestivalHoliday, FestivalHolidayService>();

        builder.Services.AddScoped<AttendanceState>();

        // ==========================
        // Other
        // ==========================
        builder.Services.AddBlazorBootstrap();
        builder.Services.AddGeolocationServices();

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