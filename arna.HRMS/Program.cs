// ============================================================
// FILE: arna.HRMS/Program.cs  (Blazor Server Web App)
// ============================================================
using arna.HRMS.Components;
using arna.HRMS.Models.State.Attendance;
using arna.HRMS.Services;
using arna.HRMS.Services.Auth;
using arna.HRMS.Services.Common;
using arna.HRMS.Services.Dashboard;
using arna.HRMS.Services.Http;
using arna.HRMS.Services.Report;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace arna.HRMS;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── Razor / Blazor ────────────────────────────────────
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // ── Authorization Core ────────────────────────────────
        // Required for [Authorize], AuthorizeView, AuthorizeRouteView
        builder.Services.AddAuthorizationCore();

        // Provides AuthenticationState as a cascading value to all components
        builder.Services.AddCascadingAuthenticationState();

        // ── ProtectedSessionStorage ───────────────────────────
        // Stores tokens encrypted in the browser session
        // Requires Data Protection (included by default in ASP.NET Core)
        builder.Services.AddScoped<ProtectedSessionStorage>();

        // ── Auth State Provider ───────────────────────────────
        // Register BOTH as concrete type (for direct injection in HttpService)
        // AND as AuthenticationStateProvider (for Blazor auth infrastructure)
        builder.Services.AddScoped<CustomAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<CustomAuthStateProvider>());

        // ── HTTP Clients ──────────────────────────────────────
        var baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                      ?? throw new Exception("ApiSettings:BaseUrl not configured in appsettings.json");

        // AuthHeaderHandler reads token from CustomAuthStateProvider (NOT cookies)
        builder.Services.AddScoped<AuthHeaderHandler>();

        // Main HTTP client — used for all authenticated API calls
        builder.Services.AddHttpClient("AuthorizedClient", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromHours(2);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddHttpMessageHandler<AuthHeaderHandler>()
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // Default injected HttpClient resolves to AuthorizedClient
        builder.Services.AddScoped(sp =>
            sp.GetRequiredService<IHttpClientFactory>()
              .CreateClient("AuthorizedClient"));

        // ── Core App Services ─────────────────────────────────
        builder.Services.AddScoped<NotificationService>();
        builder.Services.AddScoped<HttpService>();

        // ApiClients wraps all API endpoint calls
        // SetApiClients call avoids circular DI (HttpService ↔ ApiClients)
        builder.Services.AddScoped<ApiClients>(sp =>
        {
            var httpService = sp.GetRequiredService<HttpService>();
            var apiClients = new ApiClients(httpService);
            httpService.SetApiClients(apiClients);
            return apiClients;
        });

        // ── Feature Services ──────────────────────────────────
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

        // ── Other ─────────────────────────────────────────────
        builder.Services.AddBlazorBootstrap();
        builder.Services.AddGeolocationServices();

        var app = builder.Build();

        // ── Middleware Pipeline ───────────────────────────────
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