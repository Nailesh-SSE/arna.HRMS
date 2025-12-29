using arna.HRMS.ClientServices.Attendance;
using arna.HRMS.ClientServices.Employee;
using arna.HRMS.Components;

namespace arna.HRMS;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddHttpClient();

        builder.Services.AddHttpClient<IEmployeeService, EmployeeService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7134/");  // API base URL
        });
        builder.Services.AddHttpClient<IDepartmentService, DepartmentService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7134/");  // API base URL
        });
        builder.Services.AddBlazorBootstrap();
        builder.Services.AddHttpClient<IClockService, ClockService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7134/");  // API base URL
        });



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
