using arna.HRMS.ClientServices.Attendance;
using arna.HRMS.ClientServices.Auth;
using arna.HRMS.ClientServices.Common;
using arna.HRMS.ClientServices.Employee;
using arna.HRMS.ClientServices.Users;
using arna.HRMS.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace arna.HRMS;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Default"));

        builder.Services.AddHttpClient("Default", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7134/");
        });

        builder.Services.AddScoped<HttpService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IEmployeeService, EmployeeService>();
        builder.Services.AddScoped<IDepartmentService, DepartmentService>();
        builder.Services.AddScoped<IClockService, ClockService>();
        builder.Services.AddScoped<IUsersService, UsersService>();

        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<ProtectedLocalStorage>();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
        builder.Services.AddScoped<CustomAuthStateProvider>();
        builder.Services.AddMemoryCache();

        builder.Services.AddBlazorBootstrap();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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