using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Infrastructure.Validators;
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
        var employeeBaseRepo = new BaseRepository<Employee>(_dbContext);

        var festivalHolidayRepository =
            new FestivalHolidayRepository(festivalBaseRepo);

        var employeeRepository =
            new EmployeeRepository(employeeBaseRepo);

        var attendanceRepository =
            new AttendanceRepository(attendanceBaseRepo, employeeRepository, festivalHolidayRepository);


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

        // ---------- Validator ----------
        var validator = new AttendanceValidator(attendanceRepository);

        _mapper = mapperConfig.CreateMapper();

        // ---------- Service ----------
        _attendanceService = new AttendanceService(
            attendanceRepository,
            _mapper,
            _employeeServiceMock.Object,
            _festivalHolidayServiceMock.Object,
            _leaveServiceMock.Object,
            validator
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
                IsActive = false,
                IsDeleted = true,
                Notes = "Absent"
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceService.GetAttendanceAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAttendanceAsync_WhenDataExists_ReturnsSuccessWithMappedData()
    {
        var employee = new Employee
        {
            Id = 1,
            EmployeeNumber = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "9999999999",
            Position = "Developer",
            HireDate = DateTime.Today.AddYears(-1),
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Employees.Add(employee);

        _dbContext.Attendances.Add(new Attendance
        {
            EmployeeId = 1,
            Employee = employee,
            Date = new DateTime(2026, 1, 1),
            StatusId = AttendanceStatus.Present,
            TotalHours = TimeSpan.FromHours(8),
            Notes = "On Time",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceService.GetAttendanceAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.EqualTo(1));

        var dto = result.Data.First();

        Assert.That(dto.EmployeeId, Is.EqualTo(1));
        Assert.That(dto.StatusId, Is.EqualTo(AttendanceStatus.Present));
        Assert.That(dto.Notes, Is.EqualTo("On Time"));
        Assert.That(dto.WorkingHours, Is.EqualTo(TimeSpan.FromHours(8)));
    }


    [Test]
    public async Task GetAttendanceAsync_WhenNoData_ReturnsEmptyList()
    {
        var result = await _attendanceService.GetAttendanceAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.EqualTo(0));
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
    public async Task GetAttendenceByIdAsync_WhenIdIsZero_ReturnsInvalidIdFail()
    {
        var result = await _attendanceService.GetAttendenceByIdAsync(0);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Invalid Attendance ID"));
    }

    [Test]
    public async Task GetAttendenceByIdAsync_WhenIdIsNegative_ReturnsInvalidIdFail()
    {
        var result = await _attendanceService.GetAttendenceByIdAsync(-5);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Invalid Attendance ID"));
    }

    [Test]
    public async Task GetAttendenceByIdAsync_WhenMappingReturnsNull_ReturnsFail()
    {
        // Arrange
        var attendance = new Attendance
        {
            Id = 50,
            EmployeeId = 1,
            Date = DateTime.Today,
            Notes = "Test",
            StatusId = AttendanceStatus.Present,
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Attendances.Add(attendance);
        await _dbContext.SaveChangesAsync();

        var mapperMock = new Mock<IMapper>();
        mapperMock
            .Setup(m => m.Map<AttendanceDto>(It.IsAny<Attendance>()))
            .Returns((AttendanceDto)null!);

        var service = new AttendanceService(
            new AttendanceRepository(
                new BaseRepository<Attendance>(_dbContext),
                new EmployeeRepository(new BaseRepository<Employee>(_dbContext)),
                new FestivalHolidayRepository(new BaseRepository<FestivalHoliday>(_dbContext))
            ),
            mapperMock.Object,
            _employeeServiceMock.Object,
            _festivalHolidayServiceMock.Object,
            _leaveServiceMock.Object,
            new AttendanceValidator(
                new AttendanceRepository(
                    new BaseRepository<Attendance>(_dbContext),
                    new EmployeeRepository(new BaseRepository<Employee>(_dbContext)),
                    new FestivalHolidayRepository(new BaseRepository<FestivalHoliday>(_dbContext))
                )
            )
        );

        // Act
        var result = await service.GetAttendenceByIdAsync(50);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Fail to find Attendance"));
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

    [Test]
    public async Task CreateAttendanceAsync_WhenDtoIsNull_ReturnsFail()
    {
        var result = await _attendanceService.CreateAttendanceAsync(null!);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task CreateAttendanceAsync_WhenEmployeeIdInvalid_ReturnsValidationError()
    {
        var dto = new AttendanceDto
        {
            EmployeeId = 0,
            Date = DateTime.Today,
            Notes = "Test"
        };

        var result = await _attendanceService.CreateAttendanceAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("EmployeeId is required"));
    }

    [Test]
    public async Task CreateAttendanceAsync_WhenDateMissing_ReturnsValidationError()
    {
        var dto = new AttendanceDto
        {
            EmployeeId = 1,
            Date = default,
            Notes = "Test"
        };

        var result = await _attendanceService.CreateAttendanceAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Date is required"));
    }

    [Test]
    public async Task CreateAttendanceAsync_WhenNotesMissing_ReturnsValidationError()
    {
        var dto = new AttendanceDto
        {
            EmployeeId = 1,
            Date = DateTime.Today,
            Notes = ""
        };

        var result = await _attendanceService.CreateAttendanceAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Note is Required."));
    }

    [Test]
    public async Task CreateAttendanceAsync_WhenWorkingHoursNegative_ReturnsValidationError()
    {
        var dto = new AttendanceDto
        {
            EmployeeId = 1,
            Date = DateTime.Today,
            Notes = "Test",
            WorkingHours = TimeSpan.FromHours(-5)
        };

        var result = await _attendanceService.CreateAttendanceAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Working hours must be greater than or equal to zero"));
    }

    [Test]
    public async Task CreateAttendanceAsync_WhenMultipleValidationErrors_ReturnsAllErrors()
    {
        var dto = new AttendanceDto
        {
            EmployeeId = 0,
            Date = default,
            Notes = ""
        };

        var result = await _attendanceService.CreateAttendanceAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("EmployeeId is required"));
        Assert.That(result.Message, Does.Contain("Date is required"));
        Assert.That(result.Message, Does.Contain("Note is Required."));
    }

    [Test]
    public async Task CreateAttendanceAsync_WhenEmployeeNotFound_ReturnsFail()
    {
        _employeeServiceMock
            .Setup(e => e.GetEmployeeByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<EmployeeDto?>.Fail("Not found"));

        var dto = new AttendanceDto
        {
            EmployeeId = 99,
            Date = DateTime.Today,
            Notes = "Test"
        };

        var result = await _attendanceService.CreateAttendanceAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Employee not found"));
    }

    [Test]
    public async Task CreateAttendanceAsync_WhenResultMappingNull_ReturnsFail()
    {
        // ---------------- ARRANGE ----------------

        var validEntity = new Attendance
        {
            EmployeeId = 1,
            Date = DateTime.Today,
            Notes = "Test",
            StatusId = AttendanceStatus.Present,
            IsActive = true,
            IsDeleted = false
        };

        var mapperMock = new Mock<IMapper>();

        // First mapping: DTO -> Entity (must be valid to avoid EF crash)
        mapperMock.Setup(m => m.Map<Attendance>(It.IsAny<AttendanceDto>()))
                  .Returns(validEntity);

        // Second mapping: Entity -> DTO returns null (what we want to test)
        mapperMock.Setup(m => m.Map<AttendanceDto>(It.IsAny<Attendance>()))
                  .Returns((AttendanceDto)null!);

        // Employee service must return success
        _employeeServiceMock
            .Setup(e => e.GetEmployeeByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<EmployeeDto?>.Success(
                new EmployeeDto
                {
                    Id = 1,
                    HireDate = DateTime.Today.AddMonths(-1),
                    IsActive = true
                }));

        // VERY IMPORTANT: mock services used inside CreateAbsentAndHolidayAsync

        _festivalHolidayServiceMock
            .Setup(f => f.GetFestivalHolidayAsync())
            .ReturnsAsync(ServiceResult<List<FestivalHolidayDto>>
                .Success(new List<FestivalHolidayDto>()));

        _leaveServiceMock
            .Setup(l => l.GetLeaveRequestAsync())
            .ReturnsAsync(ServiceResult<List<LeaveRequestDto>>
                .Success(new List<LeaveRequestDto>()));

        // Use REAL repository (since you don't want interface)
        var attendanceRepository = new AttendanceRepository(
            new BaseRepository<Attendance>(_dbContext),
            new EmployeeRepository(new BaseRepository<Employee>(_dbContext)),
            new FestivalHolidayRepository(new BaseRepository<FestivalHoliday>(_dbContext))
        );

        var validator = new AttendanceValidator(attendanceRepository);

        var service = new AttendanceService(
            attendanceRepository,
            mapperMock.Object,
            _employeeServiceMock.Object,
            _festivalHolidayServiceMock.Object,
            _leaveServiceMock.Object,
            validator
        );

        var dto = new AttendanceDto
        {
            EmployeeId = 1,
            Date = DateTime.Today,
            Notes = "Test"
        };

        // ---------------- ACT ----------------

        var result = await service.CreateAttendanceAsync(dto);

        // ---------------- ASSERT ----------------

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Fail to Create Attendance"));
    }

    // ---------------- TODAY ----------------

    [Test]
    public async Task GetTodayLastEntryAsync_ReturnsLastAttendance()
    {
        _employeeServiceMock
           .Setup(e => e.GetEmployeeByIdAsync(It.IsAny<int>()))
           .ReturnsAsync(ServiceResult<EmployeeDto?>.Success(
               new EmployeeDto
               {
                   Id = 1,
                   HireDate = DateTime.Today.AddMonths(-1),
                   IsActive = true
               }));
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

    [Test]
    public async Task GetTodayLastEntryAsync_InvalidEmployeeId_ReturnsFail()
    {
        var result = await _attendanceService.GetTodayLastEntryAsync(0);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Employee ID"));
    }

    [Test]
    public async Task GetTodayLastEntryAsync_WhenEmployeeNotFound_ReturnsFail()
    {
        _employeeServiceMock
            .Setup(e => e.GetEmployeeByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((ServiceResult<EmployeeDto?>)null!);

        var result = await _attendanceService.GetTodayLastEntryAsync(1);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("No such Employee Found"));
    }

    [Test]
    public async Task GetTodayLastEntryAsync_WhenAttendanceNotFound_ReturnsFail()
    {
        _employeeServiceMock
            .Setup(e => e.GetEmployeeByIdAsync(1))
            .ReturnsAsync(ServiceResult<EmployeeDto?>.Success(
                new EmployeeDto { Id = 1 }));

        var result = await _attendanceService.GetTodayLastEntryAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Attendance not found"));
    }

    [Test]
    public async Task GetTodayLastEntryAsync_WhenMappingReturnsNull_ReturnsFail()
    {
        var employee = new Employee
        {
            Id = 1,
            EmployeeNumber = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "1234567890",
            Position = "Dev",
            HireDate = DateTime.Today.AddYears(-1),
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Employees.Add(employee);

        var attendance = new Attendance
        {
            EmployeeId = 1,
            Employee = employee,
            Date = DateTime.Today,
            Notes = "Morning",
            StatusId = AttendanceStatus.Present,
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Attendances.Add(attendance);
        await _dbContext.SaveChangesAsync();

        _employeeServiceMock
            .Setup(e => e.GetEmployeeByIdAsync(1))
            .ReturnsAsync(ServiceResult<EmployeeDto?>.Success(
                new EmployeeDto { Id = 1 }));

        var mapperMock = new Mock<IMapper>();

        mapperMock.Setup(m => m.Map<AttendanceDto>(It.IsAny<Attendance>()))
                  .Returns((AttendanceDto)null!);

        var service = new AttendanceService(
            new AttendanceRepository(
                new BaseRepository<Attendance>(_dbContext),
                new EmployeeRepository(new BaseRepository<Employee>(_dbContext)),
                new FestivalHolidayRepository(new BaseRepository<FestivalHoliday>(_dbContext))
            ),
            mapperMock.Object,
            _employeeServiceMock.Object,
            _festivalHolidayServiceMock.Object,
            _leaveServiceMock.Object,
            new AttendanceValidator(
                new AttendanceRepository(
                    new BaseRepository<Attendance>(_dbContext),
                    new EmployeeRepository(new BaseRepository<Employee>(_dbContext)),
                    new FestivalHolidayRepository(new BaseRepository<FestivalHoliday>(_dbContext))
                )
            )
        );

        var result = await service.GetTodayLastEntryAsync(1);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("No data found"));
    }

    //---------------- MONTHLY ----------------

    [Test]
    public async Task GetAttendanceByMonthAsync_WhenYearInvalid_ReturnsFail()
    {
        var result = await _attendanceService
            .GetAttendanceByMonthAsync(0, 5, null, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Invalid year"));
    }

    [Test]
    public async Task GetAttendanceByMonthAsync_WhenMonthLessThanOne_ReturnsFail()
    {
        var result = await _attendanceService
            .GetAttendanceByMonthAsync(2026, 0, null, null);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid month"));
    }

    [Test]
    public async Task GetAttendanceByMonthAsync_WhenMonthGreaterThanTwelve_ReturnsFail()
    {
        var result = await _attendanceService
            .GetAttendanceByMonthAsync(2026, 13, null, null);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid month"));
    }

    [Test]
    public async Task GetAttendanceByMonthAsync_WhenNoData_ReturnsFail()
    {
        var result = await _attendanceService
            .GetAttendanceByMonthAsync(2026, 1, 999, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("No such Employee Found"));
    }

    [Test]
    public async Task GetAttendanceByMonthAsync_WhenDataExists_ReturnsSuccess()
    {
        var employee = new Employee
        {
            Id = 1,
            EmployeeNumber = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "1234567890",
            Position = "Dev",
            HireDate = DateTime.Today.AddYears(-1),
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Employees.Add(employee);

        _dbContext.Attendances.Add(new Attendance
        {
            EmployeeId = 1,
            Employee = employee,
            Date = new DateTime(2026, 2, 10),
            StatusId = AttendanceStatus.Present,
            Notes = "Present",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceService
            .GetAttendanceByMonthAsync(2026, 2, null, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetAttendanceByMonthAsync_WhenEmpIdProvided_ReturnsSuccess()
    {
        _employeeServiceMock
           .Setup(e => e.GetEmployeeByIdAsync(It.IsAny<int>()))
           .ReturnsAsync(ServiceResult<EmployeeDto?>.Success(
               new EmployeeDto
               {
                   Id = 1,
                   HireDate = DateTime.Today.AddMonths(-1),
                   IsActive = true
               }));
        

        _dbContext.Attendances.Add(new Attendance
        {
            EmployeeId = 5,
            Date = new DateTime(2026, 3, 15),
            StatusId = AttendanceStatus.Present,
            Notes = "Present",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceService
            .GetAttendanceByMonthAsync(2026, 3, 5, null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Any(), Is.True);
    }

    [Test]
    public async Task GetAttendanceByMonthAsync_WhenSpecificDateProvided_ReturnsSuccess()
    {
        var employee = new Employee
        {
            Id = 10,
            EmployeeNumber = "EMP010",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "8888888888",
            Position = "HR",
            HireDate = DateTime.Today.AddYears(-2),
            IsActive = true,
            IsDeleted = false
        };

        var specificDate = new DateTime(2026, 4, 20);

        _dbContext.Employees.Add(employee);

        _dbContext.Attendances.Add(new Attendance
        {
            EmployeeId = 10,
            Employee = employee,
            Date = specificDate,
            StatusId = AttendanceStatus.Present,
            Notes = "Present",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceService
            .GetAttendanceByMonthAsync(2026, 4, null, specificDate);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.EqualTo(1));
    }
}
