using arna.HRMS.Components;
using arna.HRMS.Models.State.Attendance;
using arna.HRMS.Services;
using arna.HRMS.Services.Auth;
using arna.HRMS.Services.Common;
using arna.HRMS.Services.Dashboard;
using arna.HRMS.Services.Http;
using arna.HRMS.Services.Report;
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

        // Authorized Client with token handler (used for all normal API calls)
        builder.Services.AddHttpClient("AuthorizedClient", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromHours(2);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "arna.HRMS/1.0"); // ← add this
        })
        .AddHttpMessageHandler<AuthHeaderHandler>()
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // ✅ FIX: RefreshClient has NO AuthHeaderHandler attached.
        // This is the dedicated client used ONLY for token refresh calls in HttpService.
        // Using the AuthorizedClient for refresh creates an infinite retry loop:
        //   401 → TryRefreshTokenAsync → uses same client → 401 → loop → ObjectDisposedException
        // The RefreshClient bypasses the auth handler entirely, hitting /api/auth/refresh directly.
        builder.Services.AddHttpClient("RefreshClient", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "arna.HRMS/1.0"); // ← add this
        });

        // Default injected HttpClient = AuthorizedClient
        builder.Services.AddScoped(sp =>
            sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient("AuthorizedClient"));

        // ==========================
        // Core App Services
        // ==========================
        builder.Services.AddScoped<NotificationService>();

        // ✅ FIX: HttpService now requires IHttpClientFactory to create the RefreshClient
        // internally for token refresh (preventing infinite refresh loop).
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
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IEmployeeService, EmployeeService>();
        builder.Services.AddScoped<IDepartmentService, DepartmentService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAttendanceService, AttendanceService>();
        builder.Services.AddScoped<IAttendanceRequestService, AttendanceRequestService>();
        builder.Services.AddScoped<ILeaveService, LeaveService>();
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IFestivalHolidayService, FestivalHolidayService>();
        builder.Services.AddScoped<IReportService, ReportService>();

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