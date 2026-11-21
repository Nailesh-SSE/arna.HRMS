using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace arna.HRMS.Tests.Integration;

[TestFixture]
public class ReportIntegrationTests
{
    private ApplicationDbContext _context = null!;
    private ReportRepository _repo = null!;
    private ReportService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                "Data Source=103.92.235.56;Initial Catalog=Dev_ArnaHRMS;Integrated Security=False;Persist Security Info=False;User ID=devhrms;Password=hrms@123;TrustServerCertificate=True;",
                sql => sql.UseCompatibilityLevel(110)
            )
            .Options;
        _context = new ApplicationDbContext(options);
        _repo = new ReportRepository(_context);
        _service = new ReportService(_repo);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task Test_GetEmployeeAttendanceReport()
    {
        var result = await _service.GetEmployeeAttendanceReport(2026, 10, null, null, null);
        Console.WriteLine($"Attendance Report Success: {result.IsSuccess}, Count: {result.Data?.Count}, Message: {result.Message}");
        if (result.Data != null)
        {
            foreach (var item in result.Data)
            {
                Console.WriteLine($"Emp: {item.EmployeeNumber} - {item.EmployeeName}, Present: {item.TotalPresent}, Absent: {item.TotalAbsent}, Img: {item.ProfileImage}");
            }
        }
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task Test_GetLeaveSummaryReport()
    {
        var result = await _service.GetLeaveSummaryReport(2026, null, null, null, null);
        Console.WriteLine($"Leave Report Success: {result.IsSuccess}, Count: {result.Data?.Count}, Message: {result.Message}");
        if (result.Data != null)
        {
            foreach (var item in result.Data)
            {
                Console.WriteLine($"Emp: {item.EmployeeNumber} - {item.EmployeeName}, LeaveNameId: {item.LeaveNameId}, Used: {item.UsedLeave}, Img: {item.ProfileImage}");
            }
        }
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task Test_GetDailyAttendanceReport()
    {
        var result = await _service.GetDailyAttendanceReport(2026, 10, null, null, null, null, null);
        Console.WriteLine($"Daily Attendance Success: {result.IsSuccess}, Count: {result.Data?.Count}, Message: {result.Message}");
        Assert.That(result.IsSuccess, Is.True);
    }
}
