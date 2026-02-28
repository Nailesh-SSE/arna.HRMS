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
            StatusId=Status.Pending,
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
        dto.FromDate = DateTime.Today;

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Future or current From Date is not allowed"));
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
        Assert.That(result.Message, Does.Contain("Future or current To Date is not allowed"));
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
        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
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

        // 🔹 Now add AttendanceRequest
        _dbContext.AttendanceRequest.Add(new AttendanceRequest
        {
            EmployeeId = 1,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.LateClockIn,
            LocationId = AttendanceLocation.Office,
            IsActive = true,
            IsDeleted = false,
            StatusId = Status.Pending
        });

        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAttendanceRequestsAsync(null, null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(1));
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
        _dbContext.Employees.AddRange(
            new Employee
            {
                Id = 1,
                FirstName = "A",
                LastName = "A",
                Email = "a@test.com",
                PhoneNumber = "111",
                IsActive = true,
                IsDeleted = false,
                HireDate = DateTime.Today,
                DateOfBirth = DateTime.Today,
                Position = "Dev",
                Salary = 100,
                DepartmentId = 1,
                EmployeeNumber = "EMP001"
            },
            new Employee
            {
                Id = 2,
                FirstName = "B",
                LastName = "B",
                Email = "b@test.com",
                PhoneNumber = "222",
                IsActive = true,
                IsDeleted = false,
                HireDate = DateTime.Today,
                DateOfBirth = DateTime.Today,
                Position = "Dev",
                Salary = 100,
                DepartmentId = 1,
                EmployeeNumber = "EMP002"
            });

        await _dbContext.SaveChangesAsync();

        _dbContext.AttendanceRequest.AddRange(
            new AttendanceRequest
            {
                EmployeeId = 1,
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
                EmployeeId = 2,
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

        var result = await _service.GetAttendanceRequestsAsync(2, null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(1));
        Assert.That(result.Data.All(x => x.EmployeeId == 2), Is.True);
    }

    [Test]
    public async Task GetAttendanceRequests_WhenStatusProvided_ReturnsMatchingStatus()
    {
        _dbContext.Employees.Add(new Employee
        {
            Id = 5,
            FirstName = "X",
            LastName = "Y",
            Email = "x@test.com",
            PhoneNumber = "555",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today,
            DateOfBirth = DateTime.Today,
            Position = "Dev",
            Salary = 100,
            DepartmentId = 1,
            EmployeeNumber = "EMP005"
        });

        await _dbContext.SaveChangesAsync();

        _dbContext.AttendanceRequest.AddRange(
            new AttendanceRequest
            {
                EmployeeId = 5,
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
                EmployeeId = 5,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.ClientVisit,
                LocationId = AttendanceLocation.Office,
                StatusId = Status.Rejected,
                IsActive = true,
                IsDeleted = false
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAttendanceRequestsAsync(null, Status.Rejected);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(1));
        Assert.That(result.Data.First().StatusId, Is.EqualTo(Status.Rejected));
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Invalid AttendanceRequest ID"));
    }

    [Test]
    public async Task GetAttendenceRequestByIdAsync_WhenIdNegative_ReturnsFail()
    {
        var result = await _service.GetAttendanceRequestByIdAsync(-10);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Invalid AttendanceRequest ID"));
    }

    [Test]
    public async Task GetAttendenceRequestByIdAsync_ShouldMapAllFieldsCorrectly()
    {
        _dbContext.Employees.Add(new Employee
        {
            Id = 200,
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@test.com",
            PhoneNumber = "8888888888",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today,
            DateOfBirth = DateTime.Today,
            Position = "Dev",
            Salary = 100,
            DepartmentId = 1,
            EmployeeNumber = "EMP200"
        });

        await _dbContext.SaveChangesAsync();

        var entity = new AttendanceRequest
        {
            EmployeeId = 200,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.ForgotToClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(9),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(8),
            Description = "Full mapping test",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.AttendanceRequest.Add(entity);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAttendanceRequestByIdAsync(entity.Id);
        var dto = result.Data!;

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(dto.Id, Is.EqualTo(entity.Id));
        Assert.That(dto.EmployeeId, Is.EqualTo(200));
        Assert.That(dto.FromDate!.Value.Date, Is.EqualTo(DateTime.Today));
        Assert.That(dto.ToDate!.Value.Date, Is.EqualTo(DateTime.Today));
        Assert.That(dto.ReasonTypeId, Is.EqualTo(AttendanceReasonType.ForgotToClockIn));
        Assert.That(dto.LocationId, Is.EqualTo(AttendanceLocation.Office));
        Assert.That(dto.ClockIn, Is.EqualTo(entity.ClockIn));
        Assert.That(dto.ClockOut, Is.EqualTo(entity.ClockOut));
        Assert.That(dto.BreakDuration, Is.EqualTo(TimeSpan.FromHours(1)));
        Assert.That(dto.TotalHours, Is.EqualTo(TimeSpan.FromHours(8)));
        Assert.That(dto.Description, Is.EqualTo("Full mapping test"));
        Assert.That(dto.StatusId, Is.EqualTo(Status.Pending));
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
            .Setup(x => x.GetEmployeeAttendanceByStatusAsync(null , null))
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
        var employee = CreateEmployee(1);
        _dbContext.Employees.Add(employee);

        var request = CreateValidEntity(1);
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
        var employee = CreateEmployee(2);
        _dbContext.Employees.Add(employee);

        var request = CreateValidEntity(2);
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _service.UpdateAttendanceRequestStatusAsync(
            request.Id,
            Status.Pending,
            10
        );

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Not Allow to Pending"));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_WhenApprovedByInvalid_ReturnsFail()
    {
        var employee = CreateEmployee(3);
        _dbContext.Employees.Add(employee);

        var request = CreateValidEntity(3);
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _service.UpdateAttendanceRequestStatusAsync(
            request.Id,
            Status.Approved,
            0
        );

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid Approved ID"));
    }

    [Test]
public async Task UpdateAttendanceRequestStatusAsync_WhenRepositoryReturnsFalse_ReturnsFail()
{
    // ---------- Arrange ----------

    _dbContext.Employees.Add(new Employee
    {
        Id = 1,
        FirstName = "Test",
        LastName = "User",
        Email = "test@test.com",
        PhoneNumber = "9999999999",
        HireDate = DateTime.Today.AddYears(-1),
        DateOfBirth = DateTime.Today.AddYears(-25),
        Position = "Developer",
        Salary = 50000,
        DepartmentId = 1,
        EmployeeNumber = "Emp001",
        IsActive = true,
        IsDeleted = false
    });

    await _dbContext.SaveChangesAsync();

    var request = new AttendanceRequest
    {
        EmployeeId = 1,
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

    _dbContext.AttendanceRequest.Add(request);
    await _dbContext.SaveChangesAsync();

    var id = request.Id;

    // Detach so we simulate separate DB call
    _dbContext.Entry(request).State = EntityState.Detached;

    // NOW make it inactive directly in DB BEFORE update
    var record = await _dbContext.AttendanceRequest.FindAsync(id);
    record!.IsActive = false;
    record.IsDeleted = true;
    await _dbContext.SaveChangesAsync();

    // ---------- Act ----------
    var result = await _service.UpdateAttendanceRequestStatusAsync(
        id,
        Status.Approved,
        99
    );

    // ---------- Assert ----------
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
            .Setup(x => x.GetEmployeeAttendanceByStatusAsync(null, null))
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

        var result = await _service.UpdateAttendanceRequestStatusAsync(request.Id, Status.Rejected, approvedBy:99);

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

        var employee = CreateEmployee(6);
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
        var employee = CreateEmployee(7);
        _dbContext.Employees.Add(employee);

        var request = CreateValidEntity(7);
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        _dbContext.AttendanceRequest.Remove(request);
        await _dbContext.SaveChangesAsync();

        var result = await _service.UpdateAttendanceRequestStatusAsync(
            request.Id,
            Status.Approved,
            10
        );

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public async Task GetPendingAttendanceRequestes_WhenSuccess()
    {
        _dbContext.Employees.AddRange(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
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
            FirstName = "John2",
            LastName = "Doe2",
            Email = "john2@test.com",
            PhoneNumber = "5999999999",
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
            FirstName = "John3",
            LastName = "Doe3",
            Email = "john3@test.com",
            PhoneNumber = "9989999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            EmployeeNumber = "EMP003",
            IsActive = true,
            IsDeleted = false
        }
        );

        await _dbContext.SaveChangesAsync();
        _dbContext.AttendanceRequest.AddRange(
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
        );
        await _dbContext.SaveChangesAsync();
        var result2 = await _service.GetAttendanceRequestsAsync(null, null);
        var count = result2.Data!.Count;
        var result = await _service.GetPendingAttendanceRequestsAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(2));
        Assert.That(result.Data!.All(r => r.StatusId == Status.Pending), Is.True);
    }

    [Test]
public async Task GetPendingAttendanceRequestesAsync_ShouldMapFieldsCorrectly()
{
    _dbContext.Employees.Add(new Employee
    {
        Id = 5,
        FirstName = "Alice",
        LastName = "Smith",
        Email = "alice@test.com",
        PhoneNumber = "5555555555",
        IsActive = true,
        IsDeleted = false,
        HireDate = DateTime.Today,
        DateOfBirth = DateTime.Today,
        Position = "Developer",
        Salary = 50000,
        DepartmentId = 1,
        EmployeeNumber = "EMP005"
    });

    await _dbContext.SaveChangesAsync();

    var entity = new AttendanceRequest
    {
        EmployeeId = 5,
        FromDate = DateTime.Today,
        ToDate = DateTime.Today,
        ReasonTypeId = AttendanceReasonType.ForgotToClockIn,
        LocationId = AttendanceLocation.Office,
        Description = "Forgot punch",
        TotalHours = TimeSpan.FromHours(8),
        StatusId = Status.Pending,
        IsActive = true,
        IsDeleted = false
    };

    _dbContext.AttendanceRequest.Add(entity);
    await _dbContext.SaveChangesAsync();

    var result = await _service.GetPendingAttendanceRequestsAsync();
    var dto = result.Data!.First();

    Assert.That(dto.Id, Is.EqualTo(entity.Id));
    Assert.That(dto.EmployeeId, Is.EqualTo(5));
    Assert.That(dto.FromDate!.Value.Date, Is.EqualTo(DateTime.Today));
    Assert.That(dto.ToDate!.Value.Date, Is.EqualTo(DateTime.Today));
    Assert.That(dto.ReasonTypeId, Is.EqualTo(AttendanceReasonType.ForgotToClockIn));
    Assert.That(dto.LocationId, Is.EqualTo(AttendanceLocation.Office));
    Assert.That(dto.Description, Is.EqualTo("Forgot punch"));
    Assert.That(dto.TotalHours, Is.EqualTo(TimeSpan.FromHours(8)));
    Assert.That(dto.StatusId, Is.EqualTo(Status.Pending));
}

    [Test]
    public async Task GetPendingAttendanceRequestesAsync_ShouldMapAllFields()
    {
        _dbContext.Employees.Add(new Employee
        {
            Id = 3,
            FirstName = "Mapped",
            LastName = "User",
            Email = "map@test.com",
            PhoneNumber = "333",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today,
            DateOfBirth = DateTime.Today,
            Position = "Dev",
            Salary = 100,
            DepartmentId = 1,
            EmployeeNumber = "EMP003"
        });

        await _dbContext.SaveChangesAsync();

        var entity = new AttendanceRequest
        {
            EmployeeId = 3,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Remote,
            ClockIn = DateTime.Today.AddHours(9),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(8),
            Description = "Mapping test",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.AttendanceRequest.Add(entity);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetPendingAttendanceRequestsAsync();

        var dto = result.Data!.First();

        Assert.That(dto.EmployeeId, Is.EqualTo(3));
        Assert.That(dto.ReasonTypeId, Is.EqualTo(AttendanceReasonType.WorkFromHome));
        Assert.That(dto.LocationId, Is.EqualTo(AttendanceLocation.Remote));
        Assert.That(dto.Description, Is.EqualTo("Mapping test"));
        Assert.That(dto.StatusId, Is.EqualTo(Status.Pending));
        Assert.That(dto.TotalHours, Is.EqualTo(TimeSpan.FromHours(8)));
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
        _dbContext.Employees.Add(new Employee
        {
            Id = 5,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
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

        var request = new AttendanceRequest
        {
            EmployeeId = 5,
            FromDate = DateTime.Now.AddDays(-1),
            ToDate = DateTime.Now.AddDays(-1),
            ReasonTypeId = AttendanceReasonType.ForgotToClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Now.AddDays(-1).AddHours(9),
            ClockOut = DateTime.Now.AddDays(-1).AddHours(18),
            BreakDuration = TimeSpan.FromHours(2),
            TotalHours = TimeSpan.FromHours(7),
            Description = "Updated reason",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(request).State = EntityState.Detached;

        var dto = new AttendanceRequestDto
        {
            Id = request.Id,
            EmployeeId = 5,
            FromDate = DateTime.Now.AddDays(-1),
            ToDate = DateTime.Now.AddDays(-1),
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Remote,
            ClockIn = DateTime.Today.AddHours(10),
            ClockOut = DateTime.Today.AddHours(19),
            BreakDuration = TimeSpan.FromHours(1),
            TotalHours = TimeSpan.FromHours(8),
            Description = "Updated reason"
        };
        var result = await _service.UpdateAttendanceRequestAsync(dto);

        var updated = await _dbContext.AttendanceRequest.FindAsync(request.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(updated!.ToDate.Date, Is.EqualTo(DateTime.Now.AddDays(-1).Date));
        Assert.That(updated.ReasonTypeId, Is.EqualTo(AttendanceReasonType.WorkFromHome));
        Assert.That(updated.LocationId, Is.EqualTo(AttendanceLocation.Remote));
        Assert.That(updated.ClockIn, Is.EqualTo(DateTime.Today.AddHours(10)));
        Assert.That(updated.ClockOut, Is.EqualTo(DateTime.Today.AddHours(19)));
        Assert.That(updated.BreakDuration, Is.EqualTo(TimeSpan.FromHours(1)));
        Assert.That(updated.Description, Is.EqualTo("Updated reason"));
        Assert.That(updated.StatusId, Is.EqualTo(Status.Pending));
        Assert.That(updated.ApprovedBy, Is.Null);
        Assert.That(updated.ApprovedOn, Is.Null);
        Assert.That(updated.EmployeeId, Is.EqualTo(5));
        Assert.That(updated.FromDate.Date, Is.EqualTo(DateTime.Now.AddDays(-1).Date));
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
        _dbContext.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
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
        dto.FromDate = DateTime.Today.AddDays(-1);
        dto.ToDate = DateTime.Today.AddDays(-2);

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("From Date cannot be greater than To Date"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenFutureDate_ReturnsFail()
    {
        _dbContext.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
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
        dto.FromDate = DateTime.Today.AddDays(1);
        dto.ToDate = DateTime.Today.AddDays(1);

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Future or current From Date is not allowed"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenClockOutLessThanClockIn_ReturnsFail()
    {
        _dbContext.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
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
        dto.ClockIn = DateTime.Today.AddHours(10);
        dto.ClockOut = DateTime.Today.AddHours(9);

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("ClockOut must be greater than ClockIn"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenBreakGreaterThanTotalHours_ReturnsFail()
    {
        _dbContext.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
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
        dto.BreakDuration = TimeSpan.FromHours(9);
        dto.TotalHours = TimeSpan.FromHours(8);

        await _dbContext.SaveChangesAsync();

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Break duration cannot be greater than total hours"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenLocationMissing_ReturnsFail()
    {
        _dbContext.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
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
        dto.LocationId = null;

        var result = await _service.UpdateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Location is required"));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenReasonMissing_ReturnsFail()
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

        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
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

        _dbContext.ChangeTracker.Clear();

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
        var employee = CreateEmployee(employeeId);
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
        Assert.That(result.Message, Is.EqualTo("Attendance request not found"));
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenNotFound_ReturnsFail()
    {
        var result = await _service.DeleteAttendanceRequestAsync(9999);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Attendance request not found"));
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenDeleteFails_ReturnsFail()
    {
        // Arrange

        var employee = CreateEmployee(1);
        _dbContext.Employees.Add(employee);

        var request = CreateValidEntity(1);
        request.IsActive = false;   // simulate invalid state
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAttendanceRequestAsync(request.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Attendance request not found"));
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenSuccessful_ReturnsSuccess()
    {
        // Arrange
        var employee = CreateEmployee(2);
        _dbContext.Employees.Add(employee);

        var request = CreateValidEntity(2);
        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _service.DeleteAttendanceRequestAsync(request.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That(result.Message, Is.EqualTo("Attendance request deleted successfully"));
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenGetByIdFails_ReturnsFail()
    {
        var result = await _service.DeleteAttendanceRequestAsync(-10);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Attendance request not found"));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusCancleAsync_WhenIdInvalid_ReturnsFail()
    {
        var result = await _service.CancelAttendanceRequestAsync(0, 1);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusCancleAsync_WhenEmployeeIdInvalid_ReturnsFail()
    {
        var result = await _service.CancelAttendanceRequestAsync(1, 0);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }


    [Test]
    public async Task UpdateAttendanceRequestStatusCancleAsync_WhenRequestNotFound_ReturnsFail()
    {
        var result = await _service.CancelAttendanceRequestAsync(999, 1);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to cancel attendance request"));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusCancleAsync_WhenStatusNotPending_ReturnsFail()
    {
        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Dev",
            Salary = 1000,
            DepartmentId = 1,
            EmployeeNumber = "EMP001",
            IsActive = true,
            IsDeleted = false
        });

        _dbContext.AttendanceRequest.Add(new AttendanceRequest
        {
            EmployeeId = 1,
            FromDate = DateTime.Today.AddDays(-2),
            ToDate = DateTime.Today.AddDays(-2),
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Remote,
            StatusId = Status.Approved, // Not Pending
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        var requestId = _dbContext.AttendanceRequest.First().Id;

        var result = await _service.CancelAttendanceRequestAsync(requestId, 1);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to cancel attendance request"));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusCancleAsync_WhenValid_ReturnsSuccess()
    {
        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "9999999999",
            HireDate = DateTime.Today.AddYears(-1),
            DateOfBirth = DateTime.Today.AddYears(-25),
            Position = "Dev",
            Salary = 1000,
            DepartmentId = 1,
            EmployeeNumber = "EMP001",
            IsActive = true,
            IsDeleted = false
        });

        _dbContext.AttendanceRequest.Add(new AttendanceRequest
        {
            EmployeeId = 1,
            FromDate = DateTime.Today.AddDays(-2),
            ToDate = DateTime.Today.AddDays(-2),
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Remote,
            StatusId = Status.Pending, // Important
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        var request = _dbContext.AttendanceRequest.First();

        var result = await _service.CancelAttendanceRequestAsync(request.Id, 1);

        var updated = await _dbContext.AttendanceRequest.FindAsync(request.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That(result.Message, Is.EqualTo("Attendance request cancelled successfully"));
        Assert.That(updated!.StatusId, Is.EqualTo(Status.Cancelled));
    }

}
