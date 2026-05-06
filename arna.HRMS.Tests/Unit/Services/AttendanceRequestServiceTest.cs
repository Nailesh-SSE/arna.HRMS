using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Mapping;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Net.WebSockets;

namespace arna.HRMS.Tests.Unit.Services;

[TestFixture]
public class AttendanceRequestServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private AttendanceRequestService _service = null!;
    private Mock<IAttendanceService> _attendanceServiceMock = null!;
    private IMapper _mapper = null!;

    // =========================
    // SETUP
    // =========================

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // ---------- Repository ----------
        var requestBaseRepo = new BaseRepository<AttendanceRequest>(_dbContext);
        var attendanceRequestRepository =
            new AttendanceRequestRepository(requestBaseRepo);
        var validator = new AttendanceRequestValidator(attendanceRequestRepository);

        // ---------- MOCK AttendanceService ----------
        _attendanceServiceMock = new Mock<IAttendanceService>();

        // ---------- Mapper ----------
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AttendanceRequestProfile>();
        });

        _mapper = mapperConfig.CreateMapper();

        // ---------- Service ----------
        _service = new AttendanceRequestService(
            attendanceRequestRepository,
            _mapper,
            _attendanceServiceMock.Object,
            validator
        );
    }


    // =====================================================
    // CREATE
    // =====================================================

    private AttendanceRequestDto CreateValidDto()
    {
        return new AttendanceRequestDto
        {
            EmployeeId = 1,
            FromDate = DateTime.Today.AddDays(-1),
            ToDate = DateTime.Today.AddDays(-1),
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Remote,
            ClockIn = DateTime.Today.AddDays(-1).AddHours(9),
            ClockOut = DateTime.Today.AddDays(-1).AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(8),
            StatusId = Status.Pending,
            Description = "Valid"
        };
    }


    [Test]
    public async Task CreateAttendanceRequestAsync_WhenDtoNull_ReturnsFail()
    {
        var result = await _service.CreateAttendanceRequestAsync(null!);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Does.Contain("Invalid request"));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenEmployeeIdInvalid_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.EmployeeId = 0;

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("EmployeeId is required"));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenFromDateNull_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.FromDate = null;

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("From Date is required"));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenToDateNull_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.ToDate = null;

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("To Date is required"));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenFromDateFuture_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.FromDate = DateTime.Today.AddDays(1);

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Future Date is not allowed"));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenClockOutInvalid_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.ClockOut = dto.ClockIn;

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("ClockOut must be greater than ClockIn"));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenBreakGreaterThanTotal_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.BreakDuration = TimeSpan.FromHours(9);
        dto.TotalHours = TimeSpan.FromHours(8);

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Break duration cannot be greater than total hours"));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenMultipleErrors_ReturnsCombinedErrors()
    {
        var dto = new AttendanceRequestDto();

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("EmployeeId is required"));
        Assert.That(result.Message, Does.Contain("From Date is required"));
        Assert.That(result.Message, Does.Contain("To Date is required"));
        Assert.That(result.Message, Does.Contain("ClockIn is required"));
        Assert.That(result.Message, Does.Contain("ClockOut is required"));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_ShouldPersistData()
    {
        var dto = CreateValidDto();

        var result = await _service.CreateAttendanceRequestAsync(dto);

        var saved = await _dbContext.AttendanceRequest.FirstOrDefaultAsync();

        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.EmployeeId, Is.EqualTo(dto.EmployeeId));
        Assert.That(saved.StatusId, Is.EqualTo(Status.Pending));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_ShouldMapFieldsCorrectly()
    {
        var dto = CreateValidDto();
        dto.Description = "Mapping Test";

        var result = await _service.CreateAttendanceRequestAsync(dto);

        var saved = await _dbContext.AttendanceRequest.FirstAsync();

        Assert.That(saved.EmployeeId, Is.EqualTo(dto.EmployeeId));
        Assert.That(saved.ReasonTypeId, Is.EqualTo(dto.ReasonTypeId));
        Assert.That(saved.LocationId, Is.EqualTo(dto.LocationId));
        Assert.That(saved.Description, Is.EqualTo("Mapping Test"));
        Assert.That(saved.TotalHours, Is.EqualTo(dto.TotalHours));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenDtoIsNull_ReturnsFail()
    {
        var result = await _service.CreateAttendanceRequestAsync(null!);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenToDateFuture_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.ToDate = DateTime.Today.AddDays(1);

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Future Date is not allowed"));

    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenFromDateGreaterThanToDate_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.FromDate = DateTime.Today.AddDays(-1);
        dto.ToDate = DateTime.Today.AddDays(-5);

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("From Date cannot be greater than To Date"));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenClockInNull_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.ClockIn = null;

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("ClockIn is required"));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenClockOutNull_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.ClockOut = null;

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("ClockOut is required"));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenClockOutLessThanClockIn_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.ClockIn = DateTime.Today.AddHours(10);
        dto.ClockOut = DateTime.Today.AddHours(9);

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("ClockOut must be greater than ClockIn"));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenLocationNull_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.LocationId = null;

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Location is required"));
    }
    [Test]
    public async Task CreateAttendanceRequestAsync_WhenReasonNull_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.ReasonTypeId = null;

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Reason is required"));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenBreakDurationNull_ReturnsFail()
    {
        var dto = CreateValidDto();
        dto.BreakDuration = null;

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Break duration is required"));
    }


    [Test]
    public async Task CreateAttendanceRequestAsync_ShouldCreateRequest()
    {
        var dto = new AttendanceRequestDto
        {
            EmployeeId = 1,
            FromDate = DateTime.Now.AddDays(-1),
            ToDate = DateTime.Now.AddDays(-1),
            ReasonTypeId = AttendanceReasonType.ForgotToClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Now.AddDays(-1).AddHours(9),
            ClockOut = DateTime.Now.AddDays(-1).AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            Description = "Forgot to clock in",
            TotalHours = TimeSpan.FromHours(8)
        };

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(await _dbContext.AttendanceRequest.CountAsync(), Is.EqualTo(1));
    }

    // =====================================================
    // GET ALL
    // =====================================================
    [Test]
    public async Task GetAttendanceRequests_ReturnsAllRequests()
    {
        // 🔹 Seed Employee FIRST (required because of Include)
        var employee = new Employee
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            OfficeEmail = "john@office.com",
            PhoneNumber = "9999999999",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP001"
        };

        _dbContext.Employees.Add(employee);

        var returnRequests = new List<AttendanceRequest>
        {

            new AttendanceRequest
            {
                EmployeeId = 1,
                FromDate = DateTime.Today.AddDays(-5),
                ToDate = DateTime.Today.AddDays(-5),
                ReasonTypeId = AttendanceReasonType.ForgotToClockOut,
                LocationId = AttendanceLocation.Office,
                ClockIn = DateTime.Today.AddDays(-5).AddHours(9),
                ClockOut = DateTime.Today.AddDays(-5).AddHours(18),
                BreakDuration = TimeSpan.FromHours(1),
                TotalHours = TimeSpan.FromHours(8),
                StatusId = Status.Pending,
                IsActive = true,
                IsDeleted = false,
                ApprovedBy = null,
                ApprovedOn = null
            },
            new AttendanceRequest
            {
                EmployeeId = 1,
                FromDate = DateTime.Today.AddDays(-4),
                ToDate = DateTime.Today.AddDays(-4),
                ReasonTypeId = AttendanceReasonType.WorkFromHome,
                LocationId = AttendanceLocation.Remote,
                ClockIn = DateTime.Today.AddDays(-4).AddHours(9),
                ClockOut = DateTime.Today.AddDays(-4).AddHours(18),
                BreakDuration = TimeSpan.FromHours(1),
                TotalHours = TimeSpan.FromHours(8),
                StatusId = Status.Approved,
                IsActive = true,
                IsDeleted = false,
                ApprovedBy = 99,
                ApprovedOn = DateTime.Today.AddDays(-3)
            }
        };
        _dbContext.AttendanceRequest.AddRange(returnRequests);

        await _dbContext.SaveChangesAsync();
        var result = await _service.GetAttendanceRequestsAsync(null, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAttendanceRequests_WhenNoData_ReturnsEmptySuccess()
    {
        var result = await _service.GetAttendanceRequestsAsync(null, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetAttendanceRequests_WhenDataExists_ReturnsAll()
    {
        _dbContext.Employees.Add(new Employee
        {
            Id = 10,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            OfficeEmail = "john@office.com",
            PhoneNumber = "9999999999",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Dev",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP010"
        });

        await _dbContext.SaveChangesAsync();

        _dbContext.AttendanceRequest.AddRange(
            new AttendanceRequest
            {
                EmployeeId = 10,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.WorkFromHome,
                LocationId = AttendanceLocation.Remote,
                StatusId = Status.Pending,
                IsActive = true,
                IsDeleted = false
            },
            new AttendanceRequest
            {
                EmployeeId = 10,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.ClientVisit,
                LocationId = AttendanceLocation.ClientSite,
                StatusId = Status.Approved,
                IsActive = true,
                IsDeleted = false
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAttendanceRequestsAsync(null, null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.EqualTo(2));
        Assert.That(result.Data.All(x => x.EmployeeId == 10), Is.True);
    }

    [Test]
    public async Task GetAttendanceRequests_WhenEmployeeIdProvided_ReturnsOnlyThatEmployee()
    {
        var employees = new List<Employee>
        {
            new Employee
            {
                Id = 20,
                FirstName = "A",
                LastName = "alphabet",
                Email = "a.123@gmial.com",
                OfficeEmail = "a.987@gmail.com",
                PhoneNumber = "123",
                IsActive = true,
                IsDeleted = false,
                HireDate = DateTime.Today,
                DateOfBirth = DateTime.Today,
                Position = "Dev",
                Salary = 100,
                DepartmentId = 1,
                EmployeeNumber = "EMP020"
            },
            new Employee
            {
                Id = 21,
                FirstName = "T",
                LastName = "alphabet",
                Email = "T.123@gmial.com",
                OfficeEmail = "T.987@gmail.com",
                PhoneNumber = "1234567",
                IsActive = true,
                IsDeleted = false,
                HireDate = DateTime.Today,
                DateOfBirth = DateTime.Today,
                Position = "Dev",
                Salary = 100,
                DepartmentId = 1,
                EmployeeNumber = "EMP021"
            },
        };
        _dbContext.Employees.AddRange(employees);

        await _dbContext.SaveChangesAsync();

        _dbContext.AttendanceRequest.AddRange(
            new AttendanceRequest
            {
                EmployeeId = 20,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.WorkFromHome,
                LocationId = AttendanceLocation.Remote,
                StatusId = Status.Pending,
                IsActive = true,
                IsDeleted = false
            },
            new AttendanceRequest
            {
                EmployeeId = 21,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.ClientVisit,
                LocationId = AttendanceLocation.Office,
                StatusId = Status.Pending,
                IsActive = true,
                IsDeleted = false
            }
        );
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAttendanceRequestsAsync(20, null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.EqualTo(1));
        Assert.That(result.Data.First().EmployeeId, Is.EqualTo(20));
    }

    [Test]
    public async Task GetAttendanceRequests_WhenStatusProvided_ReturnsMatchingStatus()
    {
        var employee = new Employee
        {
            Id = 20,
            FirstName = "A",
            LastName = "alphabet",
            Email = "a.123@gmial.com",
            OfficeEmail = "a.987@gmail.com",
            PhoneNumber = "123",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today,
            DateOfBirth = DateTime.Today,
            Position = "Dev",
            Salary = 100,
            DepartmentId = 1,
            EmployeeNumber = "EMP020"
        };
        _dbContext.Employees.Add(employee);

        var attendanceRequests = new List<AttendanceRequest>
        {
            new AttendanceRequest
            {
                EmployeeId = 20,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.WorkFromHome,
                LocationId = AttendanceLocation.Remote,
                StatusId = Status.Pending,
                IsActive = true,
                IsDeleted = false
            },
            new AttendanceRequest
            {
                EmployeeId = 20,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.ClientVisit,
                LocationId = AttendanceLocation.Office,
                StatusId = Status.Approved,
                IsActive = true,
                IsDeleted = false
            }
        };

        _dbContext.AttendanceRequest.AddRange(attendanceRequests);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAttendanceRequestsAsync(null, Status.Approved);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(1));
        Assert.That(result.Data.First().StatusId, Is.EqualTo(Status.Approved));
    }

    [Test]
    public async Task GetAttendanceRequests_WhenEmployeeAndStatusProvided_ReturnsFiltered()
    {
        _dbContext.Employees.Add(new Employee
        {
            Id = 7,
            FirstName = "Z",
            LastName = "Z",
            Email = "z@test.com",
            OfficeEmail = "z@office.com",
            PhoneNumber = "777",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today,
            DateOfBirth = DateTime.Today,
            Position = "Dev",
            Salary = 100,
            DepartmentId = 1,
            EmployeeNumber = "EMP007"
        });

        await _dbContext.SaveChangesAsync();

        _dbContext.AttendanceRequest.AddRange(
            new AttendanceRequest
            {
                EmployeeId = 7,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.WorkFromHome,
                LocationId = AttendanceLocation.Remote,
                StatusId = Status.Pending,
                IsActive = true,
                IsDeleted = false
            },
            new AttendanceRequest
            {
                EmployeeId = 7,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.ClientVisit,
                LocationId = AttendanceLocation.Office,
                StatusId = Status.Approved,
                IsActive = true,
                IsDeleted = false
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAttendanceRequestsAsync(7, Status.Approved);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(1));
        Assert.That(result.Data.First().EmployeeId, Is.EqualTo(7));
        Assert.That(result.Data.First().StatusId, Is.EqualTo(Status.Approved));
    }


    // =====================================================
    // GET BY ID - FOUND
    // =====================================================

    [Test]
    public async Task GetAttendenceRequestByIdAsync_WhenIdZero_ReturnsFail()
    {
        var result = await _service.GetAttendanceRequestByIdAsync(0);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Invalid attendance request ID."));
    }

    [Test]
    public async Task GetAttendenceRequestByIdAsync_WhenIdNegative_ReturnsFail()
    {
        var result = await _service.GetAttendanceRequestByIdAsync(-10);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Invalid attendance request ID."));
    }

    [Test]
    public async Task GetAttendenceRequestByIdAsync_ShouldMapAllFieldsCorrectly()
    {
        var employee = new Employee
        {
            Id = 7,
            FirstName = "Z",
            LastName = "Z",
            Email = "z@test.com",
            OfficeEmail = "z@office.com",
            PhoneNumber = "777",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today,
            DateOfBirth = DateTime.Today,
            Position = "Dev",
            Salary = 100,
            DepartmentId = 1,
            EmployeeNumber = "EMP007"
        };

        _dbContext.Employees.Add(employee);

        await _dbContext.SaveChangesAsync();

        var request = new AttendanceRequest
        {
            EmployeeId = 7,
            FromDate = DateTime.Today.AddDays(-4),
            ToDate = DateTime.Today.AddDays(-4),
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Remote,
            StatusId = Status.Pending,
            ClockIn = DateTime.Today.AddHours(9),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(8),
            Description = "Full mapping test",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAttendanceRequestByIdAsync(request.Id);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Id, Is.EqualTo(request.Id));
        Assert.That(result.Data.EmployeeId, Is.EqualTo(request.EmployeeId));
        Assert.That(result.Data.FromDate, Is.EqualTo(request.FromDate));
        Assert.That(result.Data.ToDate, Is.EqualTo(request.ToDate));
        Assert.That(result.Data.ReasonTypeId, Is.EqualTo(request.ReasonTypeId));
        Assert.That(result.Data.LocationId, Is.EqualTo(request.LocationId));
        Assert.That(result.Data.StatusId, Is.EqualTo(request.StatusId));
        Assert.That(result.Data.ClockIn, Is.EqualTo(request.ClockIn));
        Assert.That(result.Data.ClockOut, Is.EqualTo(request.ClockOut));
        Assert.That(result.Data.BreakDuration, Is.EqualTo(request.BreakDuration));
        Assert.That(result.Data.TotalHours, Is.EqualTo(request.TotalHours));
        Assert.That(result.Data.Description, Is.EqualTo(request.Description));
    }

    [Test]
    public async Task GetAttendenceRequestByIdAsync_WhenMapperReturnsNull_ShouldReturnSuccessWithNullData()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 300,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            OfficeEmail = "test@office.com",
            PhoneNumber = "123",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today,
            DateOfBirth = DateTime.Today,
            Position = "Dev",
            Salary = 100,
            DepartmentId = 1,
            EmployeeNumber = "EMP300"
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var entity = new AttendanceRequest
        {
            EmployeeId = 300,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Remote,
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.AttendanceRequest.Add(entity);
        await _dbContext.SaveChangesAsync();

        // Mock mapper to return null
        var mapperMock = new Mock<IMapper>();
        mapperMock
            .Setup(m => m.Map<AttendanceRequestDto>(It.IsAny<AttendanceRequest>()))
            .Returns((AttendanceRequestDto)null!);

        var repository = new AttendanceRequestRepository(
            new BaseRepository<AttendanceRequest>(_dbContext)
        );

        var service = new AttendanceRequestService(
            repository,
            mapperMock.Object,
            _attendanceServiceMock.Object,
            new AttendanceRequestValidator(repository)
        );

        // Act
        var result = await service.GetAttendanceRequestByIdAsync(entity.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);   // because service does not check null mapping
        Assert.That(result.Data, Is.Null);
    }


    [Test]
    public async Task GetAttendanceRequestByIdAsync_WhenFound_ReturnsDto()
    {
        _dbContext.Employees.Add(new Employee
        {
            Id = 2,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            OfficeEmail = "john@office.com",
            PhoneNumber = "9999999999",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP001"
        });

        await _dbContext.SaveChangesAsync();

        var entity = new AttendanceRequest
        {
            EmployeeId = 2,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Remote,
            StatusId = Status.Pending
        };

        _dbContext.AttendanceRequest.Add(entity);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAttendanceRequestByIdAsync(entity.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.EmployeeId, Is.EqualTo(2));
    }

    // =====================================================
    // GET BY ID - NOT FOUND
    // =====================================================
    [Test]
    public async Task GetAttendanceRequestByIdAsync_WhenNotFound_ReturnsFail()
    {
        var result = await _service.GetAttendanceRequestByIdAsync(999);

        Assert.That(result.IsSuccess, Is.False);
    }

    // =====================================================
    // UPDATE = APPROVE
    // =====================================================
    [Test]
    public async Task ApproveAttendanceRequestAsync_ShouldApproveRequest()
    {
        // ---------- MOCK DEPENDENCIES ----------
        _attendanceServiceMock
            .Setup(x => x.GetAttendanceByStatusAndEmployeeIdAsync(null, null))
            .ReturnsAsync(
                ServiceResult<List<AttendanceDto>>.Success(new List<AttendanceDto>())
            );

        _attendanceServiceMock
            .Setup(x => x.CreateAttendanceAsync(It.IsAny<AttendanceDto>()))
            .ReturnsAsync(
                ServiceResult<AttendanceDto>.Success(new AttendanceDto())
            );

        // ---------- SEED EMPLOYEE ----------
        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            OfficeEmail = "john@office.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP001",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        // ---------- SEED REQUEST ----------
        var request = new AttendanceRequest
        {
            EmployeeId = 1,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.ClientVisit,
            LocationId = AttendanceLocation.ClientSite,
            ClockIn = DateTime.Today.AddHours(9),
            ClockOut = DateTime.Today.AddHours(17),
            TotalHours = TimeSpan.FromHours(8),
            BreakDuration = TimeSpan.FromHours(1),
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        // ---------- ACT ----------
        var result = await _service.UpdateAttendanceRequestStatusAsync(
            request.Id,
            Status.Approved,
            approvedBy: 99
        );

        var updated = await _dbContext.AttendanceRequest.FindAsync(request.Id);

        // ---------- ASSERT ----------
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(updated!.StatusId, Is.EqualTo(Status.Approved));
        Assert.That(updated.ApprovedBy, Is.EqualTo(99));
        Assert.That(updated.ApprovedOn, Is.Not.Null);
    }

    //--------------------UPDATE--STATUS--------------------
    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_WhenIdInvalid_ReturnsFail()
    {
        var result = await _service.UpdateAttendanceRequestStatusAsync(0, Status.Approved, 1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid AttendanceRequest ID"));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_WhenRequestNotFound_ReturnsFail()
    {
        var result = await _service.UpdateAttendanceRequestStatusAsync(999, Status.Approved, 1);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid Attendance Request ID"));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_WhenStatusCancelled_ReturnsFail()
    {
        var employee = CreateEmployeeNew(10);
        _dbContext.Employees.Add(employee);

        var request = CreateValidEntity(10);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        var result = await _service.UpdateAttendanceRequestStatusAsync(
            request.Id,
            Status.Cancelled,
            10
        );

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Not Allow to Cancel"));

    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_WhenStatusPending_ReturnsFail()
    {
        var employee = CreateEmployeeNew(2);
        _dbContext.Employees.Add(employee);

        var request = CreateValidEntity(2);
        _dbContext.AttendanceRequest.Add(request);

        _dbContext.SaveChanges();

        var result = await _service.UpdateAttendanceRequestStatusAsync(
            request.Id,
            Status.Pending,
            2
        );

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Not Allow to Pending"));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_WhenApprovedByInvalid_ReturnsFail()
    {
        var employee = CreateEmployeeNew(2);
        _dbContext.Employees.Add(employee);

        var request = CreateValidEntity(2);
        _dbContext.AttendanceRequest.Add(request);

        var result = await _service.UpdateAttendanceRequestStatusAsync(
            request.Id,
            Status.Approved,
            0 // Invalid approver ID
        );

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Attendance Request ID"));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_WhenRepositoryReturnsFalse_ReturnsFail()
    {
        // ---------- Arrange ----------
        var employee = CreateEmployeeNew(7);
        _dbContext.Employees.Add(employee);

        var request = CreateValidEntity(7);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        request.IsActive = false;
        request.IsDeleted = true;

        await _dbContext.SaveChangesAsync();

        var result = await _service.UpdateAttendanceRequestStatusAsync(
            request.Id,
            Status.Approved,
            10
        );

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Attendance Request ID"));
    }




    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_RejectAttendanceRequestAsync_ShouldRejectRequest()
    {
        // ---------- MOCK DEPENDENCIES ----------
        _attendanceServiceMock
            .Setup(x => x.GetAttendanceByStatusAndEmployeeIdAsync(null, null))
            .ReturnsAsync(
                ServiceResult<List<AttendanceDto>>.Success(new List<AttendanceDto>())
            );

        _attendanceServiceMock
            .Setup(x => x.CreateAttendanceAsync(It.IsAny<AttendanceDto>()))
            .ReturnsAsync(
                ServiceResult<AttendanceDto>.Success(new AttendanceDto())
            );

        // ---------- SEED EMPLOYEE ----------
        _dbContext.Employees.Add(new Employee
        {
            Id = 3,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            OfficeEmail = "john@office.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP001",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();
        var request = new AttendanceRequest
        {
            EmployeeId = 3,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.EarlyClockOut,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(9),
            ClockOut = DateTime.Today.AddHours(17),
            TotalHours = TimeSpan.FromHours(8),
            BreakDuration = TimeSpan.FromHours(1),
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _service.UpdateAttendanceRequestStatusAsync(request.Id, Status.Rejected, approvedBy: 99);

        var updated = await _dbContext.AttendanceRequest.FindAsync(request.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(updated!.StatusId, Is.EqualTo(Status.Rejected));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_WhenApproved_CreatesAttendance()
    {
        _attendanceServiceMock
            .Setup(x => x.CreateAttendanceAsync(It.IsAny<AttendanceDto>()))
            .ReturnsAsync(ServiceResult<AttendanceDto>.Success(new AttendanceDto()));

        var employee = CreateEmployeeNew(6);
        _dbContext.Employees.Add(employee);

        var request = CreateValidEntity(6);
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _service.UpdateAttendanceRequestStatusAsync(
            request.Id,
            Status.Approved,
            50
        );

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);

        _attendanceServiceMock.Verify(
            x => x.CreateAttendanceAsync(It.IsAny<AttendanceDto>()),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_WhenApproved_ButRequestNotFetched_ShouldStillReturnSuccess()
    {
        var employee = CreateEmployeeNew(99);
        _dbContext.Employees.Add(employee);

        var request = CreateValidEntity(99);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        _dbContext.AttendanceRequest.Remove(request);

        await _dbContext.SaveChangesAsync();

        var result = await _service.UpdateAttendanceRequestStatusAsync(
            request.Id,
            Status.Approved,
            50
        );

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid Attendance Request ID"));
    }

    [Test]
    public async Task GetPendingAttendanceRequestes_WhenSuccess()
    {
        var employees = new List<Employee>
        {
            new Employee
            {
                Id = 1,
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob@gmail.com",
                OfficeEmail = "bob@office.com",
                PhoneNumber = "9999999999",
                HireDate = DateTime.Today.AddYears(-1),
                DateOfBirth = DateTime.Today.AddYears(-25),
                Position = "Developer",
                Salary = 50000,
                DepartmentId = 1,
                EmployeeNumber = "EMP001",
                IsActive = true,
                IsDeleted = false
            },
            new Employee
            {
                Id = 2,
                FirstName = "Nick",
                LastName = "Jostar",
                Email = "nick@gmail.com",
                OfficeEmail = "nick@office.com",
                PhoneNumber = "9999999999",
                HireDate = DateTime.Today.AddYears(-1),
                DateOfBirth = DateTime.Today.AddYears(-25),
                Position = "Developer",
                Salary = 50000,
                DepartmentId = 1,
                EmployeeNumber = "EMP002",
                IsActive = true,
                IsDeleted = false
            },
            new Employee
            {
                Id = 3,
                FirstName = "amma",
                LastName = "John",
                Email = "amma@gmail.com",
                OfficeEmail = "amma@office.com",
                PhoneNumber = "9999999999",
                HireDate = DateTime.Today.AddYears(-1),
                DateOfBirth = DateTime.Today.AddYears(-25),
                Position = "Developer",
                Salary = 50000,
                DepartmentId = 1,
                EmployeeNumber = "EMP003",
                IsActive = true,
                IsDeleted = false
            }
        };
        _dbContext.Employees.AddRange(employees);
        
        await _dbContext.SaveChangesAsync();

        var requests = new List<AttendanceRequest>
        {
            new AttendanceRequest
            {
                EmployeeId = 1,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.ForgotToClockIn,
                LocationId = AttendanceLocation.Office,
                StatusId = Status.Pending,
                IsActive = true,
                IsDeleted = false
            },
            new AttendanceRequest
            {
                EmployeeId = 2,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.LateClockIn,
                LocationId = AttendanceLocation.Office,
                StatusId = Status.Pending,
                IsActive = true,
                IsDeleted = false
            },
            new AttendanceRequest
            {
                EmployeeId = 3,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.WorkFromHome,
                LocationId = AttendanceLocation.Remote,
                StatusId = Status.Approved,
                IsActive = true,
                IsDeleted = false
            }
        };

        _dbContext.AttendanceRequest.AddRange(requests);

        await _dbContext.SaveChangesAsync();

        var result = await _service.GetPendingAttendanceRequestsAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count(), Is.EqualTo(2));
        Assert.That(result.Data!.All(ar =>ar.StatusId == Status.Pending), Is.True);
    }

    [Test]
    public async Task GetPendingAttendanceRequestesAsync_ShouldMapFieldsCorrectly()
    {
        var employee = new Employee
        {
            Id = 2,
            FirstName = "Nick",
            LastName = "Jostar",
            Email = "nick@gmail.com",
            OfficeEmail = "nick@office.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP002",
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var request = new AttendanceRequest
        {
            EmployeeId = 2,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.LateClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(10),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(7),
            Description = "Late clock in due to traffic",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.AttendanceRequest.Add(request);  
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetPendingAttendanceRequestsAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.First().EmployeeId, Is.EqualTo(2));
        Assert.That(result.Data!.First().FromDate, Is.EqualTo(DateTime.Today));
        Assert.That(result.Data!.First().ToDate, Is.EqualTo(DateTime.Today));
        Assert.That(result.Data!.First().ClockIn, Is.EqualTo(DateTime.Today.AddHours(10)));
        Assert.That(result.Data!.First().ClockOut, Is.EqualTo(DateTime.Today.AddHours(18)));
        Assert.That(result.Data!.First().BreakDuration, Is.EqualTo(TimeSpan.FromHours(1)));
        Assert.That(result.Data!.First().TotalHours, Is.EqualTo(TimeSpan.FromHours(7)));
        Assert.That(result.Data!.First().Description, Is.EqualTo("Late clock in due to traffic"));
        Assert.That(result.Data!.First().StatusId, Is.EqualTo(Status.Pending));
        Assert.That(result.Data!.First().IsActive, Is.EqualTo(true));
        Assert.That(result.Data!.First().IsDeleted, Is.EqualTo(false));

    }

    [Test]
    public async Task GetPendingAttendanceRequestesAsync_WhenNoData_ReturnsEmptySuccess()
    {
        var result = await _service.GetPendingAttendanceRequestsAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.EqualTo(0));
    }

    //-------------------- UPDATE --------------------

    [Test]
    public async Task UpdateAttendanceRequestAsync_ShouldUpdateRequest()
    {
        var employee = new Employee
        {
            Id = 2,
            FirstName = "Jonathan",
            LastName = "Jostar",
            Email = "nick@gmail.com",
            OfficeEmail = "nick@office.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP002",
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var request = new AttendanceRequest
        {
            EmployeeId = 2,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.LateClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(10),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(7),
            Description = "Late clock in due to traffic",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var updatedrequest = new AttendanceRequestDto
        {
            Id = request.Id,
            EmployeeId = 2,
            FromDate = DateTime.Today.AddDays(-1),
            ToDate = DateTime.Today.AddDays(-1),
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Remote,
            ClockIn = DateTime.Today.AddDays(-1).AddHours(9),
            ClockOut = DateTime.Today.AddDays(-1).AddHours(17),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(7),
            Description = "Updated to work from home",
            StatusId = Status.Pending
        };

        var result = await _service.UpdateAttendanceRequestAsync(updatedrequest);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.EmployeeId, Is.EqualTo(2));
        Assert.That(result.Data.FromDate, Is.EqualTo(DateTime.Today.AddDays(-1)));
        Assert.That(result.Data.ToDate, Is.EqualTo(DateTime.Today.AddDays(-1)));
        Assert.That(result.Data.ClockIn, Is.EqualTo(DateTime.Today.AddDays(-1).AddHours(9)));
        Assert.That(result.Data.ClockOut, Is.EqualTo(DateTime.Today.AddDays(-1).AddHours(17)));
        Assert.That(result.Data.BreakDuration, Is.EqualTo(TimeSpan.FromHours(1)));
        Assert.That(result.Data.TotalHours, Is.EqualTo(TimeSpan.FromHours(7)));
        Assert.That(result.Data.Description, Is.EqualTo("Updated to work from home"));
        Assert.That(result.Data.StatusId, Is.EqualTo(Status.Pending));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenDtoIsNull_ReturnsFail()
    {
        var result = await _service.UpdateAttendanceRequestAsync(null!);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenInvalidId_ReturnsFail()
    {
        var dto = new AttendanceRequestDto
        {
            Id = 0
        };

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid Attendance Request ID"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenEmployeeIdInvalid_ReturnsFail()
    {
        _dbContext.Add(new AttendanceRequest
        {
            Id = 1,
            EmployeeId = 1,
            FromDate = DateTime.Today.AddDays(-2),
            ToDate = DateTime.Today.AddDays(-2),
            ClockIn = DateTime.Today.AddDays(-2).AddHours(9),
            ClockOut = DateTime.Today.AddDays(-2).AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(8),
            Description = "Old",
            LocationId = AttendanceLocation.Remote,
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            StatusId = Status.Pending,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        var dto = CreateValidUpdateDto();
        dto.EmployeeId = 0;

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("EmployeeId is required"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenFromDateGreaterThanToDate_ReturnsFail()
    {
        var employee = new Employee
        {
            Id = 2,
            FirstName = "Jonathan",
            LastName = "Jostar",
            Email = "nick@gmail.com",
            OfficeEmail = "nick@office.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP002",
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var request = new AttendanceRequest
        {
            EmployeeId = 2,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.LateClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(10),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(7),
            Description = "Late clock in due to traffic",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var dto = CreateValidUpdateDto();
        dto.FromDate = DateTime.Today.AddDays(-1);
        dto.ToDate = DateTime.Today.AddDays(-2);

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("From Date cannot be greater than To Date"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenFutureDate_ReturnsFail()
    {
        var employee = new Employee
        {
            Id = 2,
            FirstName = "Jonathan",
            LastName = "Jostar",
            Email = "nick@gmail.com",
            OfficeEmail = "nick@office.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP002",
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var request = new AttendanceRequest
        {
            EmployeeId = 2,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.LateClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(10),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(7),
            Description = "Late clock in due to traffic",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var dto = CreateValidUpdateDto();
        dto.FromDate = DateTime.Today.AddDays(1);
        dto.ToDate = DateTime.Today.AddDays(1);

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Future Date is not allowed"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenClockOutLessThanClockIn_ReturnsFail()
    {
        var emplyoee = new Employee
        {
            Id = 2,
            FirstName = "Jonathan",
            LastName = "Jostar",
            Email = "nick@gmail.com",
            OfficeEmail = "nick@office.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP002",
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.Employees.Add(emplyoee);
        await _dbContext.SaveChangesAsync();

        var request = new AttendanceRequest
        {
            EmployeeId = 2,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.LateClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(10),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(7),
            Description = "Late clock in due to traffic",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var dto = CreateValidUpdateDto();
        dto.ClockIn = DateTime.Today.AddHours(18);
        dto.ClockOut = DateTime.Today.AddHours(10);

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("ClockOut must be greater than ClockIn for same-day requests"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenBreakGreaterThanTotalHours_ReturnsFail()
    {
        var employee = new Employee 
        {
            Id = 2,
            FirstName = "Jonathan",
            LastName = "Jostar",
            Email = "nick@gmail.com",
            OfficeEmail = "nick@office.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP002",
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var request = new AttendanceRequest
        {
            EmployeeId = 2,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.LateClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(10),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(7),
            Description = "Late clock in due to traffic",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var dto = CreateValidUpdateDto();
        dto.BreakDuration = TimeSpan.FromHours(8);
        dto.TotalHours = TimeSpan.FromHours(7);

        var result = await _service.UpdateAttendanceRequestAsync(dto);  

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Break duration cannot be greater than total hours"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenLocationMissing_ReturnsFail()
    {
        var employee = new Employee
        {
            Id = 2,
            FirstName = "Jonathan",
            LastName = "Jostar",
            Email = "nick@gmail.com",
            OfficeEmail = "nick@office.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP002",
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var request = new AttendanceRequest
        {
            EmployeeId = 2,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.LateClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(10),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(7),
            Description = "Late clock in due to traffic",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var dto = CreateValidUpdateDto();
        dto.LocationId = null;

        var result = await _service.UpdateAttendanceRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Location is required"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenReasonMissing_ReturnsFail()
    {
        var employee = new Employee
        {
            Id = 2,
            FirstName = "Jonathan",
            LastName = "Jostar",
            Email = "nick@gmail.com",
            OfficeEmail = "nick@office.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP002",
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var request = new AttendanceRequest
        {
            EmployeeId = 2,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.LateClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(10),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(7),
            Description = "Late clock in due to traffic",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();
        
        var dto = CreateValidUpdateDto();
        dto.ReasonTypeId = null;

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Reason is required"));
    }

    private AttendanceRequestDto CreateValidUpdateDto()
    {
        return new AttendanceRequestDto
        {
            Id = 1,
            EmployeeId = 1,
            FromDate = DateTime.Today.AddDays(-2),
            ToDate = DateTime.Today.AddDays(-2),
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Remote,
            ClockIn = DateTime.Today.AddDays(-2).AddHours(9),
            ClockOut = DateTime.Today.AddDays(-2).AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(8),
            Description = "Updated"
        };
    }

    private AttendanceRequest CreateValidEntity(int employeeId)
    {
        return new AttendanceRequest
        {
            EmployeeId = employeeId,
            FromDate = DateTime.Today.AddDays(-2),
            ToDate = DateTime.Today.AddDays(-2),
            ReasonTypeId = AttendanceReasonType.ForgotToClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddDays(-2).AddHours(9),
            ClockOut = DateTime.Today.AddDays(-2).AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(8),
            Description = "Old",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };
    }

    private Employee CreateEmployee(int id)
    {
        return new Employee
        {
            Id = id,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = $"EMP{id}",
            IsActive = true,
            IsDeleted = false
        };
    }

    private async Task<int> SeedValidRecordAsync(int employeeId = 1)
    {
        var employee = CreateEmployeeNew(employeeId);
        _dbContext.Employees.Add(employee);

        var entity = CreateValidEntity(employeeId);
        _dbContext.AttendanceRequest.Add(entity);

        await _dbContext.SaveChangesAsync();

        return entity.Id;
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenFromDateNull_ReturnsFail()
    {
        var id = await SeedValidRecordAsync();

        var dto = CreateValidUpdateDto();
        dto.Id = id;
        dto.FromDate = null;

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("From Date is required"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenToDateNull_ReturnsFail()
    {
        var id = await SeedValidRecordAsync();

        var dto = CreateValidUpdateDto();
        dto.Id = id;
        dto.ToDate = null;

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("To Date is required"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenClockInNull_ReturnsFail()
    {
        var id = await SeedValidRecordAsync();

        var dto = CreateValidUpdateDto();
        dto.Id = id;
        dto.ClockIn = null;

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("ClockIn is required"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenClockOutNull_ReturnsFail()
    {
        var id = await SeedValidRecordAsync();

        var dto = CreateValidUpdateDto();
        dto.Id = id;
        dto.ClockOut = null;

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("ClockOut is required"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenBreakDurationNull_ReturnsFail()
    {
        var id = await SeedValidRecordAsync();

        var dto = CreateValidUpdateDto();
        dto.Id = id;
        dto.BreakDuration = null;

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Break duration is required"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenRecordNotFound_ReturnsFail()
    {
        var dto = CreateValidUpdateDto();
        dto.Id = 9999;

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("No Data Found"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenMultipleErrors_ReturnsAllMessages()
    {
        _dbContext.Add(new AttendanceRequest
        {
            Id = 1,
            EmployeeId = 0,
            FromDate = DateTime.Today.AddDays(-2),
            ToDate = DateTime.Today.AddDays(-2),
            ClockIn = DateTime.Today.AddDays(-2).AddHours(9),
            ClockOut = DateTime.Today.AddDays(-2).AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(8),
            Description = "Old",
            LocationId = AttendanceLocation.Remote,
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            StatusId = Status.Pending,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        var dto = new AttendanceRequestDto
        {
            Id = 1,
            EmployeeId = 0,
            FromDate = null,
            ToDate = null,
            ClockIn = null,
            ClockOut = null,
            LocationId = null,
            ReasonTypeId = null,
            BreakDuration = null,
            TotalHours = TimeSpan.Zero
        };

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("EmployeeId is required"));
        Assert.That(result.Message, Does.Contain("From Date is required"));
        Assert.That(result.Message, Does.Contain("To Date is required"));
        Assert.That(result.Message, Does.Contain("ClockIn is required"));
        Assert.That(result.Message, Does.Contain("ClockOut is required"));
        Assert.That(result.Message, Does.Contain("Location is required"));
        Assert.That(result.Message, Does.Contain("Reason is required"));
        Assert.That(result.Message, Does.Contain("Break duration is required"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenDescriptionTooLong_ShouldPassDueToBug()
    {
        var id = await SeedValidRecordAsync();

        var dto = CreateValidUpdateDto();
        dto.Id = id;
        dto.Description = new string('A', 600);

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
    }


    [Test]
    public async Task AttendanceRequestCancel()
    {
        // ---------- Seed Employee ----------
        _dbContext.Employees.Add(new Employee
        {
            Id = 5,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            OfficeEmail = "john@office.com",
            PhoneNumber = "9999999999",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP001"
        });

        await _dbContext.SaveChangesAsync();

        // ---------- Seed AttendanceRequests ----------
        var pendingToCancel = new AttendanceRequest
        {
            EmployeeId = 5,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.ForgotToClockIn,
            LocationId = AttendanceLocation.Office,
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        var approved = new AttendanceRequest
        {
            EmployeeId = 5,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.LateClockIn,
            LocationId = AttendanceLocation.Office,
            StatusId = Status.Approved,
            IsActive = true,
            IsDeleted = false
        };

        var pendingOther = new AttendanceRequest
        {
            EmployeeId = 5,
            FromDate = DateTime.Today.AddDays(4),
            ToDate = DateTime.Today.AddDays(5),
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Remote,
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.AttendanceRequest.AddRange(
            pendingToCancel,
            approved,
            pendingOther
        );

        await _dbContext.SaveChangesAsync();

        // ---------- ACT ----------
        var result = await _service.CancelAttendanceRequestAsync(
            pendingToCancel.Id,
            5
        );

        // ---------- ASSERT ----------
        Assert.That(result.IsSuccess, Is.True);

        var cancelled = await _dbContext.AttendanceRequest.FindAsync(pendingToCancel.Id);
        var approvedAfter = await _dbContext.AttendanceRequest.FindAsync(approved.Id);
        var pendingAfter = await _dbContext.AttendanceRequest.FindAsync(pendingOther.Id);

        Assert.That(cancelled!.StatusId, Is.EqualTo(Status.Cancelled));
        Assert.That(approvedAfter!.StatusId, Is.EqualTo(Status.Approved));
        Assert.That(pendingAfter!.StatusId, Is.EqualTo(Status.Pending));
        Assert.That(cancelled.EmployeeId, Is.EqualTo(5));
        Assert.That(pendingAfter.EmployeeId, Is.EqualTo(5));
        Assert.That(approvedAfter.EmployeeId, Is.EqualTo(5));
        Assert.That(cancelled.FromDate, Is.EqualTo(DateTime.Today));
        Assert.That(pendingAfter.FromDate, Is.EqualTo(DateTime.Today.AddDays(4)));
        Assert.That(approvedAfter.FromDate, Is.EqualTo(DateTime.Today));
        Assert.That(cancelled.ToDate, Is.EqualTo(DateTime.Today));
        Assert.That(pendingAfter.ToDate, Is.EqualTo(DateTime.Today.AddDays(5)));
        Assert.That(approvedAfter.ToDate, Is.EqualTo(DateTime.Today));
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenIdInvalid_ReturnsFail()
    {
        var result = await _service.DeleteAttendanceRequestAsync(0);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid attendance request ID."));
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenNotFound_ReturnsFail()
    {
        var result = await _service.DeleteAttendanceRequestAsync(9999);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Attendance request not found."));
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenDeleteFails_ReturnsFail()
    {
        // Arrange
        var employee = CreateEmployeeNew(2);
        _dbContext.Employees.Add(employee);

        var request = CreateValidEntity(2);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        var result = await _service.DeleteAttendanceRequestAsync(request.Id +1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Attendance request not found."));
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenSuccessful_ReturnsSuccess()
    {
        // Arrange
        var employee = CreateEmployeeNew(2);
        _dbContext.Add(employee);

        var request = CreateValidEntity(2);
        _dbContext.Add(request);

        await _dbContext.SaveChangesAsync();

        var result = await _service.DeleteAttendanceRequestAsync(request.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That(result.Message, Is.EqualTo("Attendance request deleted successfully."));
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenGetByIdFails_ReturnsFail()
    {
        var result =await _service.DeleteAttendanceRequestAsync(9999);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Attendance request not found."));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusCancleAsync_WhenIdInvalid_ReturnsFail()
    {
        var result = await _service.CancelAttendanceRequestAsync(0, 1);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request."));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusCancleAsync_WhenEmployeeIdInvalid_ReturnsFail()
    {
        var result = await _service.CancelAttendanceRequestAsync(1, 0);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request."));
    }


    [Test]
    public async Task UpdateAttendanceRequestStatusCancleAsync_WhenRequestNotFound_ReturnsFail()
    {
        var result = await _service.CancelAttendanceRequestAsync(999, 1);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to cancel attendance request."));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusCancleAsync_WhenStatusNotPending_ReturnsFail()
    {
        var employee = new Employee
        {
            Id = 2,
            FirstName = "Jonathan",
            LastName = "Jostar",
            Email = "nick@gmail.com",
            OfficeEmail = "nick@office.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP002",
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var request = new AttendanceRequest
        {
            EmployeeId = 2,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.LateClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(10),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(7),
            Description = "Late clock in due to traffic",
            StatusId = Status.Approved,
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _service.CancelAttendanceRequestAsync(request.Id,employee.Id);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to cancel attendance request."));

    }

    [Test]
    public async Task UpdateAttendanceRequestStatusCancleAsync_WhenValid_ReturnsSuccess()
    {
        var employee = new Employee
        {
            Id = 2,
            FirstName = "Jonathan",
            LastName = "Jostar",
            Email = "nick@gmail.com",
            OfficeEmail = "nick@office.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP002",
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var request = new AttendanceRequest
        {
            EmployeeId = 2,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.LateClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(10),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(7),
            Description = "Late clock in due to traffic",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _service.CancelAttendanceRequestAsync(request.Id,2);

        var updateddata = await _dbContext.AttendanceRequest.FindAsync(request.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That(result.Message, Is.EqualTo("Attendance request cancelled successfully."));
        Assert.That(updateddata.StatusId, Is.EqualTo(Status.Cancelled));

    }
    private Employee CreateEmployeeNew(int id)
    {
        return new Employee
        {
            Id = id,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            OfficeEmail = "Test@office.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = $"EMP{id}",
            IsActive = true,
            IsDeleted = false
        };
    }
}
