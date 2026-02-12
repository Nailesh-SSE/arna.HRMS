using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Mapping;
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
public class AttendanceRequestServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private AttendanceRequestService _service = null!;
    private Mock<IAttendanceService> _attendanceServiceMock = null;
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
    [Test]
    public async Task CreateAttendanceRequestAsync_ShouldCreateRequest()
    {
        var dto = new AttendanceRequestDto
        {
            EmployeeId = 1,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.ForgotToClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(9),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            Description = "Forgot to clock in"
        };

        var result = await _service.CreateAttendanceRequestAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(await _dbContext.AttendanceRequest.CountAsync(), Is.EqualTo(1));
    }

    // =====================================================
    // GET ALL
    // =====================================================
    [Test]
    public async Task GetAttendanceRequestAsync_ReturnsAllRequests()
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

        var result = await _service.GetAttendanceRequests(null, null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(1));
    }

    // =====================================================
    // GET BY ID - FOUND
    // =====================================================
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

        var result = await _service.GetAttendenceRequestByIdAsync(entity.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.EmployeeId, Is.EqualTo(2));
    }

    // =====================================================
    // GET BY ID - NOT FOUND
    // =====================================================
    [Test]
    public async Task GetAttendanceRequestByIdAsync_WhenNotFound_ReturnsFail()
    {
        var result = await _service.GetAttendenceRequestByIdAsync(999);

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
            .Setup(x => x.GetAttendanceAsync())
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

    // =====================================================
    // UPDATE = REJECT
    // =====================================================
    [Test]
    public async Task RejectAttendanceRequestAsync_ShouldRejectRequest()
    {
        // ---------- MOCK DEPENDENCIES ----------
        _attendanceServiceMock
            .Setup(x => x.GetAttendanceAsync())
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
    public async Task pendingRequestAttendanceRecord()
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
        var result2 = await _service.GetAttendanceRequests(null, null);
        var count = result2.Data!.Count;
        var result = await _service.GetPendingAttendanceRequestesAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(2));
        Assert.That(result.Data!.All(r => r.StatusId == Status.Pending), Is.True);
    }

    [Test]
    public async Task GetAttendanceRequestByEmployee()
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
                StatusId = Status.Pending
            },
            new AttendanceRequest
            {
                EmployeeId = 2,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.LateClockIn,
                LocationId = AttendanceLocation.Office,
                StatusId = Status.Rejected
            },
            new AttendanceRequest
            {
                EmployeeId = 2,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.WorkFromHome,
                LocationId = AttendanceLocation.Remote,
                StatusId = Status.Approved
            },
            new AttendanceRequest
            {
                EmployeeId = 2,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.EarlyClockOut,
                LocationId = AttendanceLocation.Office,
                StatusId = Status.Pending
            },
            new AttendanceRequest
            {
                EmployeeId = 3,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.ClientVisit,
                LocationId = AttendanceLocation.ClientSite,
                StatusId = Status.Pending
            },
            new AttendanceRequest
            {
                EmployeeId = 2,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                ReasonTypeId = AttendanceReasonType.ForgotToClockOut,
                LocationId = AttendanceLocation.Office,
                StatusId = Status.Cancelled
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAttendanceRequests(2, null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(4));
        Assert.That(result.Data!.All(r => r.EmployeeId == 2), Is.True);
        Assert.That(result.Data!.Count(r => r.StatusId == Status.Pending), Is.EqualTo(1));
        Assert.That(result.Data!.Count(r => r.StatusId == Status.Rejected), Is.EqualTo(1));
        Assert.That(result.Data!.Count(r => r.StatusId == Status.Approved), Is.EqualTo(1));
        Assert.That(result.Data!.Count(r => r.StatusId == Status.Cancelled), Is.EqualTo(1));
        Assert.That(result.Data!.Count(r => r.ReasonTypeId == AttendanceReasonType.LateClockIn), Is.EqualTo(1));
        Assert.That(result.Data!.Count(r => r.ReasonTypeId == AttendanceReasonType.WorkFromHome), Is.EqualTo(1));
        Assert.That(result.Data!.Count(r => r.ReasonTypeId == AttendanceReasonType.ForgotToClockOut), Is.EqualTo(1));
        Assert.That(result.Data!.Count(r => r.ReasonTypeId == AttendanceReasonType.EarlyClockOut), Is.EqualTo(1));
        Assert.That(result.Data!.Count(r => r.LocationId == AttendanceLocation.Office), Is.EqualTo(3));
        Assert.That(result.Data!.Count(r => r.LocationId == AttendanceLocation.Remote), Is.EqualTo(1));
        Assert.That(result.Data!.Count(r => r.LocationId == AttendanceLocation.ClientSite), Is.EqualTo(0));
    }

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
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.ForgotToClockIn,
            LocationId = AttendanceLocation.Office,
            ClockIn = DateTime.Today.AddHours(9),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(2),
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
            FromDate = DateTime.Today,
            ToDate = DateTime.Today.AddDays(1),
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Remote,
            ClockIn = DateTime.Today.AddHours(10),
            ClockOut = DateTime.Today.AddHours(19),
            BreakDuration = TimeSpan.FromHours(1),
            Description = "Updated reason"
        };
        var result = await _service.UpdateAttendanceRequestAsync(dto);

        var updated = await _dbContext.AttendanceRequest.FindAsync(request.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(updated!.ToDate, Is.EqualTo(DateTime.Today.AddDays(1)));
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
        Assert.That(updated.FromDate, Is.EqualTo(DateTime.Today));
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
        var result = await _service.UpdateAttendanceRequestStatusCancleAsync(
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

}
