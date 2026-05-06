using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Tests.Unit.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Repositories;

[TestFixture]
public class ReportRepositoryTests
{
    private ApplicationDbContext _context;
    private ReportRepository _repository;

    [SetUp]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();
        _repository = new ReportRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_ReturnsData_WhenValidInput()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            2026, null, 1036, AttendanceStatus.Present, DeviceType.LaptopDesktop, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_Works_WithAllNullParameters()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, null, null, null, null, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_Works_WithOnlyYearParameter()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            2026, null, null, null, null, null, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_Works_WithOnlyMonth()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, 3, null, null, null, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_Works_WithOnlyEmployeeId()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, null, 1036, null, null, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_Works_WithOnlyStatus()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, null, null, AttendanceStatus.Present, null, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public  async Task GetDailyAttendanceReportAsync_Works_WithOnlyDeviceType()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, null, null, null, DeviceType.LaptopDesktop, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]  
    public async Task GetDailyAttendanceReportAsync_Works_WithOnlyFormDate()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, null, null, null, null, DateTime.Now.AddDays(-15), null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]  
    public async Task GetDailyAttendanceReportAsync_Works_WithOnlyToDate()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, null, null, null, null, null, DateTime.Now.AddDays(-7));
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_FiltersByYearAndMonth()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            2026, 3, null, null, null, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_FiltersByYearAndEmployeeId()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            2026, null, 1036, null, null, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_FiltersByYearAndStatus()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            2026, null, null, AttendanceStatus.Present, null, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_FiltersByYearAndDevice()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            2026, null, null, null, DeviceType.LaptopDesktop, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_FiltersByYearAndFormDate()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            2026, null, null, null, null, DateTime.Now.AddDays(-20),null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_FiltersByYearAndToDate()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            2026, null, null, null, null, null, DateTime.Now.AddDays(-40));
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_FiltersByMonthAndEmployeeId()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, 3, 1036, null, null, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_FiltersByMonthAndStatus()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, 4, null, AttendanceStatus.Present, null, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_FiltersByMonthAndDevice()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, 3, null, null, DeviceType.LaptopDesktop, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_FiltersByMonthAndFormDate()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, 3, null, null, null, DateTime.Now.AddDays(-60), null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }
    [Test]
    public async Task GetDailyAttendanceReportAsync_FiltersByMonthAndToDate()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, 3, null, null, null, null, DateTime.Now.AddDays(-10));
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_FiltersByEmployeeIdAndStatus()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, null, 1036, AttendanceStatus.Present, null, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_FiltersByDateRange()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, null, null, null, null,
            DateTime.Now.AddDays(-7),
            DateTime.Now);
        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetDailyAttendanceReportAsync_ReturnsEmpty_WhenInvalidDateRange()
    {
        //Act
        var result = await _repository.GetDailyAttendanceReportAsync(
            null, null, null, null, null,
            DateTime.Now,
            DateTime.Now.AddDays(-5));
        //Assert
        Assert.That(result, Is.Empty);
    }
    [Test]
    public async Task GetLeaveSummaryReportAsync_ReturnData_WhenValidInput()
    {
        //Act
        var result = await _repository.GetLeaveSummaryReportAsync(
            2026, 3, null, null, null);
        //Assert
        Assert.That(result, Is.Not.Null);
    }
}

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Data Source=103.92.235.56;Initial Catalog=Dev_ArnaHRMS;Integrated Security=False;Persist Security Info=False;User ID=devhrms;Password=hrms@123;TrustServerCertificate=True;")
            .Options;

        var context = new ApplicationDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}