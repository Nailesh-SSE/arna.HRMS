using arna.HRMS.API.Controllers;
using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Controllers;

[TestFixture]
public class AttendanceControllerTests
{
    private Mock<IAttendanceService> _serviceMock = null!;
    private AttendanceController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IAttendanceService>();
        _controller = new AttendanceController(_serviceMock.Object);
    }

    [Test]
    public async Task GetAttendance_ShouldOk_WhenDataFound()
    {
        // Arrange
        var list = new List<AttendanceDto>
        {
            new AttendanceDto { Id = 1, EmployeeId = 1, Date = DateTime.Today, ClockInTime = DateTime.Now.AddHours(-2).TimeOfDay, ClockOutTime=null },
            new AttendanceDto { Id = 2, EmployeeId = 2, Date = DateTime.Today, ClockInTime = null, ClockOutTime=DateTime.Now.TimeOfDay }
        };
        _serviceMock
            .Setup(s => s.GetEmployeeAttendanceByStatusAsync(null, 1))
            .ReturnsAsync(ServiceResult<List<AttendanceDto>>.Success(list));
        // Act
        var result = await _controller.GetAsync(null, 1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetAttendance_ShouldOk_WhenDataNotFound()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetEmployeeAttendanceByStatusAsync(null , 1))
            .ReturnsAsync(ServiceResult<List<AttendanceDto>>.Success(null!, "No data found"));
        // Act
        var result = await _controller.GetAsync(null, 1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetAttendanceById_ShouldOk_WhenDataFound()
    {
        // Arrange
        var attendance = new AttendanceDto { Id = 1, EmployeeId = 1, Date = DateTime.Today, ClockInTime = DateTime.Now.AddHours(-2).TimeOfDay, ClockOutTime = null };
        _serviceMock
            .Setup(s => s.GetAttendanceByIdAsync(1))!
            .ReturnsAsync(ServiceResult<AttendanceDto>.Success(attendance));
        // Act
        var result = await _controller.GetByIdAsync(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetAttendanceById_ShouldNotFound_WhenDataNotFound()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetAttendanceByIdAsync(1))!
            .ReturnsAsync(ServiceResult<AttendanceDto>.Fail("Attendance not found"));
        // Act
        var result = await _controller.GetByIdAsync(1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetAttendanceById_ShouldTail_WhenIdIsZeroOrNegative()
    {
        _serviceMock
            .Setup(s => s.GetAttendanceByIdAsync(It.Is<int>(id => id <= 0)))!
            .ReturnsAsync(ServiceResult<AttendanceDto>.Fail("Invalid ID"));
        var result = await _controller.GetByIdAsync(0);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        Assert.That((result as NotFoundObjectResult)?.Value, Is.EqualTo("Invalid ID"));

        result = await _controller.GetByIdAsync(-1);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        Assert.That((result as NotFoundObjectResult)?.Value, Is.EqualTo("Invalid ID"));
    }

    [Test]
    public async Task CreateAttendance_ShouldOk_WhenDataIsValid()
    {
        // Arrange
        var attendanceDto = new AttendanceDto { EmployeeId = 1, Date = DateTime.Today, ClockInTime = DateTime.Now.AddHours(-2).TimeOfDay };
        _serviceMock
            .Setup(s => s.CreateAttendanceAsync(attendanceDto))
            .ReturnsAsync(ServiceResult<AttendanceDto>.Success(attendanceDto));
        // Act
        var result = await _controller.CreateAsync(attendanceDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task CreateAttendance_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        var dto = new AttendanceDto();

        _controller.ModelState.AddModelError("Date", "Required");

        var result = await _controller.CreateAsync(dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateAttendance_ShouldReturnBadRequest_WhenServiceFails()
    {
        var attendanceDto = new AttendanceDto { EmployeeId = 1, Date = DateTime.Today, ClockInTime = DateTime.Now.AddHours(-2).TimeOfDay };
        _serviceMock
            .Setup(s => s.CreateAttendanceAsync(attendanceDto))
            .ReturnsAsync(ServiceResult<AttendanceDto>.Fail("Creation failed"));
        var result = await _controller.CreateAsync(attendanceDto);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That((result as BadRequestObjectResult)?.Value, Is.EqualTo("Creation failed"));
    }
    //private List<MonthlyAttendanceDto> MonthlyDetails()
    //{
    //    return new List<MonthlyAttendanceDto>
    //    {
    //        new MonthlyAttendanceDto
    //        {
    //            Date = new DateOnly(2026, 02, 10),
    //            Day = new DateOnly(2026, 02, 10).DayOfWeek.ToString(),
    //            Employees = new List<EmployeeDailyAttendanceDto>
    //            {
    //                new EmployeeDailyAttendanceDto
    //                {
    //                    EmployeeId = 1,
    //                    EmployeeNumber = "EMP001",
    //                    EmployeeName = "John Doe",
    //                    ClockIn = new TimeSpan(9, 0, 0),
    //                    ClockOut = new TimeSpan(18, 0, 0),
    //                    WorkingHours = new TimeSpan(8, 0, 0),
    //                    BreakDuration = new TimeSpan(1, 0, 0),
    //                    TotalHours = new TimeSpan(9, 0, 0),
    //                    Status = "Present"
    //                },
    //                new EmployeeDailyAttendanceDto
    //                {
    //                    EmployeeId = 2,
    //                    EmployeeNumber = "EMP002",
    //                    EmployeeName = "Jane Smith",
    //                    ClockIn = new TimeSpan(9, 30, 0),
    //                    ClockOut = new TimeSpan(18, 30, 0),
    //                    WorkingHours = new TimeSpan(8, 0, 0),
    //                    BreakDuration = new TimeSpan(1, 0, 0),
    //                    TotalHours = new TimeSpan(9, 0, 0),
    //                    Status = "Present"
    //                }
    //            }
    //        },

    //        new MonthlyAttendanceDto
    //        {
    //            Date = new DateOnly(2026, 02, 11),
    //            Day = new DateOnly(2026, 02, 11).DayOfWeek.ToString(),
    //            Employees = new List<EmployeeDailyAttendanceDto>
    //            {
    //                new EmployeeDailyAttendanceDto
    //                {
    //                    EmployeeId = 1,
    //                    EmployeeNumber = "EMP001",
    //                    EmployeeName = "John Doe",
    //                    ClockIn = new TimeSpan(9, 15, 0),
    //                    ClockOut = new TimeSpan(17, 45, 0),
    //                    WorkingHours = new TimeSpan(7, 30, 0),
    //                    BreakDuration = new TimeSpan(1, 0, 0),
    //                    TotalHours = new TimeSpan(8, 30, 0),
    //                    Status = "Late"
    //                }
    //            }
    //        },

    //        new MonthlyAttendanceDto
    //        {
    //            Date = new DateOnly(2026, 02, 12),
    //            Day = new DateOnly(2026, 02, 12).DayOfWeek.ToString(),
    //            Employees = new List<EmployeeDailyAttendanceDto>
    //            {
    //                new EmployeeDailyAttendanceDto
    //                {
    //                    EmployeeId = 2,
    //                    EmployeeNumber = "EMP002",
    //                    EmployeeName = "Jane Smith",
    //                    ClockIn = null,
    //                    ClockOut = null,
    //                    WorkingHours = TimeSpan.Zero,
    //                    BreakDuration = TimeSpan.Zero,
    //                    TotalHours = TimeSpan.Zero,
    //                    Status = "Absent"
    //                }
    //            }
    //        }
    //    };
    //}

    //[Test]
    //public async Task GetAttendanceByMonth_ShouldOk_WhenDataFound()
    //{
    //    var list = MonthlyDetails();

    //    _serviceMock
    //        .Setup(s => s.GetAttendanceByMonthAsync(2024, 6, 1, null))
    //        .ReturnsAsync(ServiceResult<List<MonthlyAttendanceDto>>.Success(list));
    //    var result = await _controller.GetAttendanceByMonth(2024, 6, 1, null);
    //    Assert.That(result, Is.TypeOf<OkObjectResult>());
    //}

    //[Test]
    //public async Task GetAttendanceByMonth_ShouldBadRequest_WhenServiceFails()
    //{
    //    _serviceMock
    //        .Setup(s => s.GetAttendanceByMonthAsync(2024, 6, 1, null))
    //        .ReturnsAsync(ServiceResult<List<MonthlyAttendanceDto>>.Fail("Failed to retrieve data"));
    //    var result = await _controller.GetAttendanceByMonth(2024, 6, 1, null);
    //    Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    //    Assert.That((result as BadRequestObjectResult)?.Value, Is.EqualTo("Failed to retrieve data"));
    //}

    //[Test]
    //public async Task GetAttendanceByMonth_ShouldBadRequest_WhenInvalidParameters()
    //{
    //    _serviceMock
    //        .Setup(s => s.GetAttendanceByMonthAsync(2024, 13, 1, null))
    //        .ReturnsAsync(ServiceResult<List<MonthlyAttendanceDto>>.Fail("invalid Month"));

    //    var result = await _controller.GetAttendanceByMonth(2024, 13, 1, null);
    //    Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    //}

    //[Test]
    //public async Task GetAttendanceByMonth_ShouldBadRequest_WhenYearIsInvalid()
    //{
    //    _serviceMock
    //        .Setup(s => s.GetAttendanceByMonthAsync(1899, 6, 1, null))
    //        .ReturnsAsync(ServiceResult<List<MonthlyAttendanceDto>>.Fail("invalid Year"));
    //    var result = await _controller.GetAttendanceByMonth(1899, 6, 1, null);
    //    Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    //}

    //[Test]
    //public async Task GetAttendanceByMonth_ShouldBadRequest_WhenEmpIdIsInvalid()
    //{
    //    _serviceMock
    //        .Setup(s => s.GetAttendanceByMonthAsync(2024, 6, -1, null))
    //        .ReturnsAsync(ServiceResult<List<MonthlyAttendanceDto>>.Fail("invalid Employee ID"));
    //    var result = await _controller.GetAttendanceByMonth(2024, 6, -1, null);
    //    Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    //}

    //[Test]
    //public async Task GetAttendanceByMonth_ShouldFail_WhenDataNotFound()
    //{
    //    _serviceMock
    //        .Setup(s => s.GetAttendanceByMonthAsync(2024, 6, 1, null))
    //        .ReturnsAsync(ServiceResult<List<MonthlyAttendanceDto>>.Success(null!, "No data found"));
    //    var result = await _controller.GetAttendanceByMonth(2024, 6, 1, null);
    //    Assert.That(result, Is.TypeOf<OkObjectResult>());
    //}

    //[Test]
    //public async Task GetAttendanceByMonth_ShouldFail_WhenAllIsZero()
    //{
    //    _serviceMock
    //        .Setup(s => s.GetAttendanceByMonthAsync(0, 0, 0, null))
    //        .ReturnsAsync(ServiceResult<List<MonthlyAttendanceDto>>.Fail("No data found"));
    //    var result = await _controller.GetAttendanceByMonth(0, 0, 0, null);
    //    Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    //}

    //[Test]
    //public async Task GetAttendanceByMonth_ShouldFail_WhenYearAndMonthIsZero()
    //{
    //    _serviceMock
    //        .Setup(s => s.GetAttendanceByMonthAsync(0, 0, 1, null))
    //        .ReturnsAsync(ServiceResult<List<MonthlyAttendanceDto>>.Fail("No data found"));
    //    var result = await _controller.GetAttendanceByMonth(0, 0, 1, null);
    //    Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    //}
    [Test]
    public async Task GetLastToday_ShouldSuccess_WhenFound()
    {
        _serviceMock
            .Setup(s => s.GetTodayLastEntryAsync(1))!
            .ReturnsAsync(
                ServiceResult<AttendanceDto>.Success(
                    new AttendanceDto { 
                        Id = 1, EmployeeId = 1, 
                        Date = DateTime.Today, 
                        ClockInTime = DateTime.Now.AddHours(-2).TimeOfDay, 
                        ClockOutTime = null 
                    }
                )
            );
        var result = await _controller.GetLastTodayAsync(1);
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetLastToday_ShouldNotFound_WhenNotFound()
    {
        _serviceMock
            .Setup(s => s.GetTodayLastEntryAsync(1))!
            .ReturnsAsync(ServiceResult<AttendanceDto>.Fail("No entry found for today"));
        var result = await _controller.GetLastTodayAsync(1);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        Assert.That((result as NotFoundObjectResult)?.Value, Is.EqualTo("No entry found for today"));
    }

    [Test]
    public async Task GetLastToday_ShouldBadRequest_WhenIdIsZeroOrNegative()
    {
        _serviceMock
            .Setup(s => s.GetTodayLastEntryAsync(It.Is<int>(id => id <= 0)))!
            .ReturnsAsync(ServiceResult<AttendanceDto>.Fail("Invalid Employee ID"));
        var result = await _controller.GetLastTodayAsync(0);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        Assert.That((result as NotFoundObjectResult)?.Value, Is.EqualTo("Invalid Employee ID"));
        result = await _controller.GetLastTodayAsync(-1);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        Assert.That((result as NotFoundObjectResult)?.Value, Is.EqualTo("Invalid Employee ID"));
    }
}
