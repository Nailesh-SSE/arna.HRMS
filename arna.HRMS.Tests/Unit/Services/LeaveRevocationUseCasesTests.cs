using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Mapping;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Services;

[TestFixture]
public class LeaveRevocationUseCasesTests
{
    private ApplicationDbContext _dbContext = null!;
    private LeaveService _leaveService = null!;
    private AttendanceRepository _attendanceRepository = null!;
    private LeaveRepository _leaveRepository = null!;
    private Mock<IFestivalHolidayService> _festivalHolidayServiceMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var leaveRequestBaseRepo = new BaseRepository<LeaveRequest>(_dbContext);
        var leaveTypeBaseRepo = new BaseRepository<LeaveType>(_dbContext);
        var attendanceBaseRepo = new BaseRepository<Attendance>(_dbContext);
        var festivalBaseRepo = new BaseRepository<FestivalHoliday>(_dbContext);
        var employeeBaseRepo = new BaseRepository<Employee>(_dbContext);

        _leaveRepository = new LeaveRepository(leaveTypeBaseRepo, leaveRequestBaseRepo);
        var festivalHolidayRepository = new FestivalHolidayRepository(festivalBaseRepo);
        var employeeRepository = new EmployeeRepository(employeeBaseRepo);
        var leaveValidator = new LeaveValidator(_leaveRepository);

        _attendanceRepository = new AttendanceRepository(attendanceBaseRepo, employeeRepository, festivalHolidayRepository);
        _festivalHolidayServiceMock = new Mock<IFestivalHolidayService>();
        _festivalHolidayServiceMock.Setup(x => x.GetFestivalHolidaysAsync())
            .ReturnsAsync(ServiceResult<List<FestivalHolidayDto>>.Success(new List<FestivalHolidayDto>()));

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AttendanceProfile>();
            cfg.AddProfile<LeaveProfile>();
        });

        _leaveService = new LeaveService(
            _leaveRepository,
            _festivalHolidayServiceMock.Object,
            _attendanceRepository,
            mapperConfig.CreateMapper(),
            leaveValidator
        );
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private async Task<LeaveRequest> SeedLeaveRequest(DateTime start, DateTime end, int empId = 1)
    {
        // Calculate days (e.g. Mon-Thu = 4 days)
        int days = (end.Date - start.Date).Days + 1;
        var leave = new LeaveRequest
        {
            EmployeeId = empId,
            LeaveTypeId = 1,
            StartDate = start,
            EndDate = end,
            LeaveDays = days,
            Reason = "Vacation",
            StatusId = Status.Approved,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        };
        await _dbContext.LeaveRequests.AddAsync(leave);

        // Also seed Attendance records with StatusId = Leave
        for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
        {
            await _dbContext.Attendances.AddAsync(new Attendance
            {
                EmployeeId = empId,
                Date = d,
                StatusId = AttendanceStatus.Leave,
                Notes = "On Leave",
                IsActive = true,
                IsDeleted = false
            });
        }
        await _dbContext.SaveChangesAsync();
        return leave;
    }

    [Test]
    public async Task Case1_WhenStartDateRegularized_SoftDeletesOldLeave_AndCreatesNewLeaveForRemainingEndDays()
    {
        // Arrange: Mon 11 May to Thu 14 May 2026 (4 days)
        var start = new DateTime(2026, 5, 11);
        var end = new DateTime(2026, 5, 14);
        var original = await SeedLeaveRequest(start, end);

        // Act: Employee worked on 11 May
        var result = await _leaveService.RevokeLeaveRequestAsync(1, start, start);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        // 1. Original leave is soft-deleted
        var oldLeave = await _dbContext.LeaveRequests.FindAsync(original.Id);
        Assert.That(oldLeave!.IsDeleted, Is.True);
        Assert.That(oldLeave.IsActive, Is.False);
        Assert.That(oldLeave.StatusId, Is.EqualTo(Status.Cancelled));

        // 2. New leave created: 12 May to 14 May (3 days)
        var activeLeaves = await _dbContext.LeaveRequests
            .Where(x => x.EmployeeId == 1 && !x.IsDeleted && x.IsActive && x.StatusId == Status.Approved)
            .ToListAsync();

        Assert.That(activeLeaves.Count, Is.EqualTo(1));
        var remaining = activeLeaves[0];
        Assert.That(remaining.StartDate, Is.EqualTo(new DateTime(2026, 5, 12)));
        Assert.That(remaining.EndDate, Is.EqualTo(new DateTime(2026, 5, 14)));
        Assert.That(remaining.LeaveDays, Is.EqualTo(3));

        // 3. Attendance record for 11 May has StatusId = Leave soft-deleted
        var attMay11 = await _dbContext.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == 1 && a.Date == start && a.StatusId == AttendanceStatus.Leave);
        Assert.That(attMay11!.IsDeleted, Is.True);

        // 4. Attendance records for 12, 13, 14 are still active
        var activeAtt = await _dbContext.Attendances
            .Where(a => a.EmployeeId == 1 && !a.IsDeleted && a.StatusId == AttendanceStatus.Leave)
            .ToListAsync();
        Assert.That(activeAtt.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task Case2_WhenEndDateRegularized_SoftDeletesOldLeave_AndCreatesNewLeaveForRemainingStartDays()
    {
        // Arrange: Mon 11 May to Thu 14 May 2026 (4 days)
        var start = new DateTime(2026, 5, 11);
        var end = new DateTime(2026, 5, 14);
        var original = await SeedLeaveRequest(start, end);

        // Act: Employee worked on 14 May
        var result = await _leaveService.RevokeLeaveRequestAsync(1, end, end);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        // 1. Original leave is soft-deleted
        var oldLeave = await _dbContext.LeaveRequests.FindAsync(original.Id);
        Assert.That(oldLeave!.IsDeleted, Is.True);
        Assert.That(oldLeave.IsActive, Is.False);
        Assert.That(oldLeave.StatusId, Is.EqualTo(Status.Cancelled));

        // 2. New leave created: 11 May to 13 May (3 days)
        var activeLeaves = await _dbContext.LeaveRequests
            .Where(x => x.EmployeeId == 1 && !x.IsDeleted && x.IsActive && x.StatusId == Status.Approved)
            .ToListAsync();

        Assert.That(activeLeaves.Count, Is.EqualTo(1));
        var remaining = activeLeaves[0];
        Assert.That(remaining.StartDate, Is.EqualTo(new DateTime(2026, 5, 11)));
        Assert.That(remaining.EndDate, Is.EqualTo(new DateTime(2026, 5, 13)));
        Assert.That(remaining.LeaveDays, Is.EqualTo(3));

        // 3. Attendance on 14th is soft-deleted
        var attMay14 = await _dbContext.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == 1 && a.Date == end && a.StatusId == AttendanceStatus.Leave);
        Assert.That(attMay14!.IsDeleted, Is.True);
    }

    [Test]
    public async Task Case3_WhenMiddleDateRegularized_SoftDeletesOldLeave_AndSplitsIntoTwoNewLeaves()
    {
        // Arrange: Mon 11 May to Thu 14 May 2026 (4 days)
        var start = new DateTime(2026, 5, 11);
        var end = new DateTime(2026, 5, 14);
        var middle = new DateTime(2026, 5, 13);
        var original = await SeedLeaveRequest(start, end);

        // Act: Employee worked on 13 May
        var result = await _leaveService.RevokeLeaveRequestAsync(1, middle, middle);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        // 1. Original leave is soft-deleted
        var oldLeave = await _dbContext.LeaveRequests.FindAsync(original.Id);
        Assert.That(oldLeave!.IsDeleted, Is.True);
        Assert.That(oldLeave.IsActive, Is.False);

        // 2. Two new leaves: 11-12 (2 days) and 14-14 (1 day)
        var activeLeaves = await _dbContext.LeaveRequests
            .Where(x => x.EmployeeId == 1 && !x.IsDeleted && x.IsActive && x.StatusId == Status.Approved)
            .OrderBy(x => x.StartDate)
            .ToListAsync();

        Assert.That(activeLeaves.Count, Is.EqualTo(2));

        Assert.That(activeLeaves[0].StartDate, Is.EqualTo(new DateTime(2026, 5, 11)));
        Assert.That(activeLeaves[0].EndDate, Is.EqualTo(new DateTime(2026, 5, 12)));
        Assert.That(activeLeaves[0].LeaveDays, Is.EqualTo(2));

        Assert.That(activeLeaves[1].StartDate, Is.EqualTo(new DateTime(2026, 5, 14)));
        Assert.That(activeLeaves[1].EndDate, Is.EqualTo(new DateTime(2026, 5, 14)));
        Assert.That(activeLeaves[1].LeaveDays, Is.EqualTo(1));

        // Total leave days = 2 + 1 = 3 days (1 day refunded to balance)
        Assert.That(activeLeaves.Sum(x => x.LeaveDays), Is.EqualTo(3));

        // 3. Attendance for 13th is soft-deleted
        var attMay13 = await _dbContext.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == 1 && a.Date == middle && a.StatusId == AttendanceStatus.Leave);
        Assert.That(attMay13!.IsDeleted, Is.True);
    }

    [Test]
    public async Task Case7_WhenSingleDayLeaveRegularized_SoftDeletesLeave_AndCreatesNoNewLeaves()
    {
        // Arrange: 1 day leave on 11 May
        var date = new DateTime(2026, 5, 11);
        var original = await SeedLeaveRequest(date, date);

        // Act: Employee worked on 11 May
        var result = await _leaveService.RevokeLeaveRequestAsync(1, date, date);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        // Original is soft-deleted
        var oldLeave = await _dbContext.LeaveRequests.FindAsync(original.Id);
        Assert.That(oldLeave!.IsDeleted, Is.True);
        Assert.That(oldLeave.StatusId, Is.EqualTo(Status.Cancelled));

        // No active leaves remain
        var activeLeaves = await _dbContext.LeaveRequests
            .Where(x => x.EmployeeId == 1 && !x.IsDeleted && x.IsActive && x.StatusId == Status.Approved)
            .ToListAsync();
        Assert.That(activeLeaves.Count, Is.EqualTo(0));
    }
}
