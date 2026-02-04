using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Infrastructure.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Services;

[TestFixture]
public class AttendanceServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private AttendanceService _attendanceService = null!;
    private IMapper _mapper = null!;
    private Mock<FestivalHolidayRepository> _festivalHolidayRepositoryMock = null!;

    private Mock<IEmployeeService> _employeeServiceMock = null!;
    private Mock<IFestivalHolidayService> _festivalHolidayServiceMock = null!;
    private Mock<ILeaveService> _leaveServiceMock = null!;

    // ---------------- SETUP ----------------

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // ---------- Repositories ----------
        var attendanceBaseRepo = new BaseRepository<Attendance>(_dbContext);
        var festivalBaseRepo = new BaseRepository<FestivalHoliday>(_dbContext);

        var festivalHolidayRepository =
            new FestivalHolidayRepository(festivalBaseRepo);

        var attendanceRepository =
            new AttendanceRepository(attendanceBaseRepo, festivalHolidayRepository);


        // ---------- Mocks ----------
        _employeeServiceMock = new Mock<IEmployeeService>();
        _festivalHolidayServiceMock = new Mock<IFestivalHolidayService>();
        _leaveServiceMock = new Mock<ILeaveService>();
        _festivalHolidayRepositoryMock = new Mock<FestivalHolidayRepository>();

        // ---------- Mapper ----------
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AttendanceProfile>();
        });

        _mapper = mapperConfig.CreateMapper();

        // ---------- Service ----------
        _attendanceService = new AttendanceService(
            attendanceRepository,
            _mapper,
            _employeeServiceMock.Object,
            _festivalHolidayServiceMock.Object,
            _leaveServiceMock.Object
        );
    }

    // ---------------- GET ALL ----------------

    [Test]
    public async Task GetAttendanceAsync_ReturnsAllAttendances()
    {
        var employee = new Employee
        {
            EmployeeNumber = "Emp003",
            FirstName = "Alex",
            LastName = "Brown",
            Email = "alex@test.com",
            PhoneNumber = "3333333333",
            Position = "Manager",
            Salary = 80000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-30)
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        _dbContext.Attendances.AddRange(
            new Attendance
            {
                EmployeeId = 1,
                Employee = employee,
                Date = new DateTime(2026, 1, 15),
                StatusId = AttendanceStatus.Present,
                IsActive = true,
                IsDeleted = false,
                Notes = "On time"
            },
            new Attendance
            {
                EmployeeId = 1,
                Employee = employee,
                Date = new DateTime(2026, 1, 16),
                StatusId = AttendanceStatus.Absent,
                IsActive = true,
                IsDeleted = false,
                Notes = "Absent"
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceService.GetAttendanceAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(2));
    }

    // ---------------- GET BY ID ----------------

    [Test]
    public async Task GetAttendenceByIdAsync_ReturnsAttendance_WhenFound()
    {
        var attendance = new Attendance
        {
            EmployeeId = 1,
            Date = DateTime.Today,
            StatusId = AttendanceStatus.Present,
            Notes = "On time"
        };

        _dbContext.Attendances.Add(attendance);
        await _dbContext.SaveChangesAsync();

        var result = await _attendanceService.GetAttendenceByIdAsync(attendance.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.EmployeeId, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAttendenceByIdAsync_ReturnsFail_WhenNotFound()
    {
        var result = await _attendanceService.GetAttendenceByIdAsync(999);

        Assert.That(result.IsSuccess, Is.False);
    }

    // ---------------- CREATE ----------------

    [Test]
    public async Task CreateAttendanceAsync_CreatesAttendance()
    {
        // ---------------- ARRANGE ----------------

        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            EmployeeNumber = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "1234567890",
            Position = "Developer",
            HireDate = DateTime.Today.AddMonths(-3),
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        // 2️⃣ Mock EmployeeService (required for service validation)
        _employeeServiceMock
            .Setup(e => e.GetEmployeeByIdAsync(1))
            .ReturnsAsync(
                ServiceResult<EmployeeDto?>.Success(
                    new EmployeeDto
                    {
                        Id = 1,
                        HireDate = DateTime.Today.AddMonths(-3),
                        IsActive = true
                    }
                )
            );

        var dto = new AttendanceDto
        {
            EmployeeId = 1,
            Date = new DateTime(2026, 2, 2),
            ClockInTime = TimeSpan.FromHours(9),
            ClockOutTime = TimeSpan.FromHours(18),
            StatusId = AttendanceStatus.Present,
            WorkingHours = TimeSpan.FromHours(9),
            Notes = "Present"
        };

        _festivalHolidayServiceMock
            .Setup(f => f.GetFestivalHolidayAsync())
            .ReturnsAsync(
                ServiceResult<List<FestivalHolidayDto>>
                    .Success(new List<FestivalHolidayDto>())
            );

        _leaveServiceMock
            .Setup(l => l.GetLeaveRequestAsync())
            .ReturnsAsync(
                ServiceResult<List<LeaveRequestDto>>
                    .Success(new List<LeaveRequestDto>())
            );

        // ---------------- ACT ----------------

        var result = await _attendanceService.CreateAttendanceAsync(dto);

        // ---------------- ASSERT ----------------

        Assert.That(result.IsSuccess, Is.True);

        var attendanceInDb = await _dbContext.Attendances.FirstOrDefaultAsync();

        Assert.That(attendanceInDb, Is.Not.Null);
        Assert.That(attendanceInDb!.EmployeeId, Is.EqualTo(1));
        Assert.That(attendanceInDb.TotalHours, Is.EqualTo(TimeSpan.FromHours(9)));
    }


    // ---------------- TODAY ----------------

    [Test]
    public async Task GetTodayLastEntryAsync_ReturnsLastAttendance()
    {
        _dbContext.Attendances.Add(new Attendance
        {
            EmployeeId = 1,
            Date = DateTime.Today,
            ClockIn = DateTime.Today.AddHours(9),
            Notes= "Morning entry"
        });

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceService.GetTodayLastEntryAsync(1);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.EmployeeId, Is.EqualTo(1));
    }

    // ---------------- BY MONTH ----------------

    [Test]
    public async Task GetAttendanceByMonthAsync_ReturnsMonthlyAttendance()
    {
        int empId = 1;
        int year = DateTime.Today.Year;
        int month = DateTime.Today.Month;

        _dbContext.Attendances.AddRange(
            new Attendance
            {
                EmployeeId = empId,
                Date = new DateTime(year, month, 1),
                ClockIn = new DateTime(year, month, 5, 9, 0, 0),
                ClockOut = new DateTime(year, month, 5, 18, 0, 0),
                TotalHours = TimeSpan.FromHours(9),
                StatusId = AttendanceStatus.Present,
                Notes = "On time"
            },
            new Attendance
            {
                EmployeeId = empId,
                Date = new DateTime(year, month, 10),
                ClockIn = new DateTime(year, month, 10, 9, 30, 0),
                ClockOut = new DateTime(year, month, 10, 17, 30, 0),
                TotalHours = TimeSpan.FromHours(8),
                StatusId = AttendanceStatus.Present,
                Notes = "Late arrival"
            },
            new Attendance
            {
                EmployeeId = 999, // other employee
                Date = new DateTime(year, month, 15),
                StatusId = AttendanceStatus.Absent,
                Notes = "Not present"
            },
            new Attendance
            {
                EmployeeId = 900, // other employee
                Date = new DateTime(year, month, 15),
                StatusId = AttendanceStatus.Late,
                Notes = "Sick leave"
            }
        );

        await _dbContext.SaveChangesAsync();

        // Act
        var result =
            await _attendanceService.GetAttendanceByMonthAsync(year, month, empId);
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.EqualTo(9));

        var first = result.Data![0];
        Assert.That(first.EmployeeId, Is.EqualTo(empId));
        Assert.That(first.Date.Month, Is.EqualTo(month));
        Assert.That(first.ClockIn, Is.Not.Null);
        Assert.That(first.ClockOut, Is.Not.Null);
        Assert.That(first.TotalHours, Is.Not.Null);
        Assert.That(first.Status, Is.EqualTo("Present"));
    }

}
