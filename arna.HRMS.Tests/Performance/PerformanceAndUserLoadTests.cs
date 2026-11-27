using System.Diagnostics;
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

namespace arna.HRMS.Tests.Performance;

[TestFixture]
public class PerformanceAndUserLoadTests
{
    private DbContextOptions<ApplicationDbContext> _dbContextOptions = null!;
    private ApplicationDbContext _dbContext = null!;
    private LeaveService _leaveService = null!;
    private AttendanceService _attendanceService = null!;
    private AttendanceRequestService _attendanceRequestService = null!;
    private FestivalHolidayService _festivalHolidayService = null!;
    private AttendanceRepository _attendanceRepository = null!;
    private LeaveRepository _leaveRepository = null!;
    private AttendanceRequestRepository _attendanceRequestRepository = null!;
    private FestivalHolidayRepository _festivalHolidayRepository = null!;
    private EmployeeRepository _employeeRepository = null!;
    private Mock<IEmployeeService> _employeeServiceMock = null!;
    private IMapper _mapper = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContextOptions = options;
        _dbContext = new ApplicationDbContext(options);

        var leaveRequestBaseRepo = new BaseRepository<LeaveRequest>(_dbContext);
        var leaveTypeBaseRepo = new BaseRepository<LeaveType>(_dbContext);
        var attendanceBaseRepo = new BaseRepository<Attendance>(_dbContext);
        var attendanceRequestBaseRepo = new BaseRepository<AttendanceRequest>(_dbContext);
        var festivalBaseRepo = new BaseRepository<FestivalHoliday>(_dbContext);
        var employeeBaseRepo = new BaseRepository<Employee>(_dbContext);

        _leaveRepository = new LeaveRepository(leaveTypeBaseRepo, leaveRequestBaseRepo, _dbContext);
        _attendanceRequestRepository = new AttendanceRequestRepository(attendanceRequestBaseRepo);
        _festivalHolidayRepository = new FestivalHolidayRepository(festivalBaseRepo);
        _employeeRepository = new EmployeeRepository(employeeBaseRepo);
        _attendanceRepository = new AttendanceRepository(attendanceBaseRepo, _employeeRepository, _festivalHolidayRepository);

        var leaveValidator = new LeaveValidator(_leaveRepository);
        var attendanceValidator = new AttendanceValidator(_attendanceRepository);
        var attendanceRequestValidator = new AttendanceRequestValidator(_attendanceRequestRepository);
        var festivalValidator = new FestivalHolidayValidator(_festivalHolidayRepository);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AttendanceProfile>();
            cfg.AddProfile<AttendanceRequestProfile>();
            cfg.AddProfile<LeaveProfile>();
            cfg.AddProfile<FestivalHolidayProfile>();
            cfg.AddProfile<EmployeeProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _festivalHolidayService = new FestivalHolidayService(
            _festivalHolidayRepository,
            _mapper,
            festivalValidator
        );

        _leaveService = new LeaveService(
            _leaveRepository,
            _festivalHolidayService,
            _attendanceRepository,
            _mapper,
            leaveValidator
        );

        _employeeServiceMock = new Mock<IEmployeeService>();

        _attendanceService = new AttendanceService(
            _attendanceRepository,
            _mapper,
            _employeeServiceMock.Object,
            _festivalHolidayService,
            _leaveService,
            attendanceValidator
        );

        _attendanceRequestService = new AttendanceRequestService(
            _attendanceRequestRepository,
            _mapper,
            _attendanceService,
            _leaveService,
            attendanceRequestValidator
        );

        // Seed basic department
        _dbContext.Departments.Add(new Department { Id = 1, Name = "Engineering", Code = "ENG", Description = "Engineering" });

        // Seed Paid Leave (5 days quota) & Unpaid Leave (30 days quota)
        _dbContext.LeaveTypes.AddRange(
            new LeaveType
            {
                Id = 1,
                LeaveNameId = LeaveName.PaidLeave,
                Description = "Paid Leave",
                MaxPerYear = 5,
                IsPaid = true,
                IsActive = true,
                IsDeleted = false
            },
            new LeaveType
            {
                Id = 2,
                LeaveNameId = LeaveName.UnpaidLeave,
                Description = "Unpaid Leave",
                MaxPerYear = 30,
                IsPaid = false,
                IsActive = true,
                IsDeleted = false
            }
        );
        _dbContext.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private AttendanceService CreateScopedAttendanceService(ApplicationDbContext dbContext)
    {
        var leaveRequestBaseRepo = new BaseRepository<LeaveRequest>(dbContext);
        var leaveTypeBaseRepo = new BaseRepository<LeaveType>(dbContext);
        var attendanceBaseRepo = new BaseRepository<Attendance>(dbContext);
        var festivalBaseRepo = new BaseRepository<FestivalHoliday>(dbContext);
        var employeeBaseRepo = new BaseRepository<Employee>(dbContext);

        var leaveRepo = new LeaveRepository(leaveTypeBaseRepo, leaveRequestBaseRepo, dbContext);
        var festivalRepo = new FestivalHolidayRepository(festivalBaseRepo);
        var empRepo = new EmployeeRepository(employeeBaseRepo);
        var attRepo = new AttendanceRepository(attendanceBaseRepo, empRepo, festivalRepo);

        var leaveValidator = new LeaveValidator(leaveRepo);
        var attendanceValidator = new AttendanceValidator(attRepo);
        var festivalValidator = new FestivalHolidayValidator(festivalRepo);

        var festService = new FestivalHolidayService(festivalRepo, _mapper, festivalValidator);
        var leaveService = new LeaveService(leaveRepo, festService, attRepo, _mapper, leaveValidator);

        return new AttendanceService(
            attRepo,
            _mapper,
            _employeeServiceMock.Object,
            festService,
            leaveService,
            attendanceValidator
        );
    }

    private async Task<Employee> CreateEmployeeAsync(int id, string name)
    {
        var emp = new Employee
        {
            Id = id,
            EmployeeNumber = $"EMP{id:D4}",
            FirstName = name,
            LastName = "User",
            Email = $"{name.ToLower()}@arna.com",
            PhoneNumber = "9876543210",
            Position = "Software Engineer",
            Salary = 75000,
            DepartmentId = 1,
            HireDate = new DateTime(2025, 1, 1),
            DateOfBirth = new DateTime(1996, 5, 20),
            IsActive = true,
            IsDeleted = false
        };

        await _dbContext.Employees.AddAsync(emp);
        await _dbContext.SaveChangesAsync();

        _employeeServiceMock.Setup(s => s.GetEmployeeByIdAsync(id))
            .ReturnsAsync(ServiceResult<EmployeeDto>.Success(_mapper.Map<EmployeeDto>(emp)));

        return emp;
    }

    // =========================================================================
    // 1. PERFORMANCE BENCHMARK TESTS
    // =========================================================================

    [Test]
    public async Task Performance_LongAbsenceAutoLeave_90Days_ExecutesWithinThreshold()
    {
        // Arrange: Employee absent for 90 days (quarter of a year)
        var emp = await CreateEmployeeAsync(1001, "BenchmarkUser");
        var startDate = new DateTime(2026, 1, 5); // Monday
        var endDate = new DateTime(2026, 4, 5);   // ~90 days later

        // Seed initial attendance on startDate
        await _dbContext.Attendances.AddAsync(new Attendance
        {
            EmployeeId = emp.Id,
            Date = startDate,
            StatusId = AttendanceStatus.Present,
            ClockIn = startDate.AddHours(9),
            ClockOut = startDate.AddHours(18),
            TotalHours = TimeSpan.FromHours(9),
            Notes = "Clock In",
            IsActive = true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        // Act: Measure execution time of auto-leave calculation and insertion for 90 days
        var sw = Stopwatch.StartNew();

        var result = await _attendanceService.CreateAttendanceAsync(new AttendanceDto
        {
            EmployeeId = emp.Id,
            Date = endDate,
            ClockInTime = TimeSpan.FromHours(9),
            StatusId = AttendanceStatus.Present,
            Notes = "Present after 90 days"
        });

        sw.Stop();

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        // Performance assertion: 90 days processing must complete in less than 1500 ms (typically ~50-100 ms)
        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(1500),
            $"Processing 90 days absence took {sw.ElapsedMilliseconds} ms, exceeding threshold.");

        // Data integrity check: working days marked as Leave
        var totalLeavesMarked = await _dbContext.Attendances
            .CountAsync(a => a.EmployeeId == emp.Id && a.StatusId == AttendanceStatus.Leave && !a.IsDeleted);

        Assert.That(totalLeavesMarked, Is.GreaterThan(50), "Expected ~65 working days marked as leave.");

        // Paid leave received exactly 5 days (reaching quota), remaining days went to Unpaid Leave
        var paidRequests = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == emp.Id && l.LeaveTypeId == 1 && !l.IsDeleted)
            .ToListAsync();
        Assert.That(paidRequests.Sum(l => l.LeaveDays), Is.EqualTo(5));

        var unpaidRequests = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == emp.Id && l.LeaveTypeId == 2 && !l.IsDeleted)
            .ToListAsync();
        Assert.That(unpaidRequests.Sum(l => l.LeaveDays), Is.EqualTo(totalLeavesMarked - 5));
    }

    [Test]
    public async Task Performance_ConcurrentRushHourClockIn_50EmployeesSimultaneous()
    {
        // Arrange: 50 distinct employees clocking in simultaneously at 9:00 AM on Monday
        int count = 50;
        var employees = new List<Employee>();
        for (int i = 1; i <= count; i++)
        {
            employees.Add(await CreateEmployeeAsync(2000 + i, $"Employee{i}"));
        }

        var previousFriday = new DateTime(2026, 5, 29);
        foreach (var emp in employees)
        {
            await _dbContext.Attendances.AddAsync(new Attendance
            {
                EmployeeId = emp.Id,
                Date = previousFriday,
                ClockIn = previousFriday.AddHours(9),
                ClockOut = previousFriday.AddHours(18),
                TotalHours = TimeSpan.FromHours(9),
                StatusId = AttendanceStatus.Present,
                Notes = "Friday Punch",
                IsActive = true,
                IsDeleted = false
            });
        }
        await _dbContext.SaveChangesAsync();

        var clockInDate = new DateTime(2026, 6, 1); // Monday rush hour

        // Act: Execute 50 clock-ins concurrently using simulated scoped DI per request
        var sw = Stopwatch.StartNew();

        var tasks = employees.Select(async emp =>
        {
            using var scopedContext = new ApplicationDbContext(_dbContextOptions);
            var scopedAttendanceService = CreateScopedAttendanceService(scopedContext);
            return await scopedAttendanceService.CreateAttendanceAsync(new AttendanceDto
            {
                EmployeeId = emp.Id,
                Date = clockInDate,
                ClockInTime = TimeSpan.FromHours(9),
                StatusId = AttendanceStatus.Present,
                Notes = "Morning Rush Punch"
            });
        }).ToList();

        var results = await Task.WhenAll(tasks);
        sw.Stop();

        // Assert
        Assert.That(results.All(r => r.IsSuccess), Is.True, "All 50 concurrent clock-in requests must succeed.");
        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(3000),
            $"50 concurrent clock-ins took {sw.ElapsedMilliseconds} ms, exceeding threshold.");

        using var verifyContext = new ApplicationDbContext(_dbContextOptions);
        var savedAttendances = await verifyContext.Attendances
            .CountAsync(a => a.Date == clockInDate && a.StatusId == AttendanceStatus.Present);
        Assert.That(savedAttendances, Is.EqualTo(count));
    }

    [Test]
    public async Task Performance_BulkApproveThroughput_100RequestsBenchmark()
    {
        // Arrange: 100 pending leave requests across multiple employees
        var emp = await CreateEmployeeAsync(3001, "BulkUser");
        var requestIds = new List<int>();

        for (int i = 1; i <= 100; i++)
        {
            var req = new LeaveRequest
            {
                Id = 1000 + i,
                EmployeeId = emp.Id,
                LeaveTypeId = 1,
                StartDate = new DateTime(2026, 7, 1).AddDays(i * 3),
                EndDate = new DateTime(2026, 7, 2).AddDays(i * 3),
                LeaveDays = 2,
                Reason = $"Leave #{i}",
                StatusId = Status.Pending,
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.Now
            };
            await _dbContext.LeaveRequests.AddAsync(req);
            requestIds.Add(req.Id);
        }
        await _dbContext.SaveChangesAsync();

        // Act: Measure bulk approval throughput
        var sw = Stopwatch.StartNew();
        var bulkResult = await _leaveService.BulkApproveLeaveRequestsAsync(requestIds, 999);
        sw.Stop();

        // Assert
        Assert.That(bulkResult.IsSuccess, Is.True);
        Assert.That(bulkResult.Data, Is.EqualTo(100));
        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(5000),
            $"Bulk approval of 100 requests took {sw.ElapsedMilliseconds} ms.");

        var approvedCount = await _dbContext.LeaveRequests
            .CountAsync(l => requestIds.Contains(l.Id) && l.StatusId == Status.Approved);
        Assert.That(approvedCount, Is.EqualTo(100));
    }

    // =========================================================================
    // 2. USER-BASE & PERSONA-BASED REAL-WORLD TESTING
    // =========================================================================

    [Test]
    public async Task UserPersona_StandardEmployee_TransitionsFromPaidToUnpaidAsQuotaExhausts()
    {
        // Scenario:
        // Employee Alex has Paid Leave (5 days quota) and Unpaid Leave active.
        // Step 1: Pre-applies 3 days of Paid Leave. (Used = 3, Remaining = 2)
        // Step 2: Absent for 5 working days (June 2..8).
        // Step 3: Clocks in on June 9.
        // Expected: Auto-leave detects remaining Paid = 2 days, so 2 days go to Paid Leave (Used = 5, Remaining = 0)
        // and remaining 3 days go to Unpaid Leave.
        var alex = await CreateEmployeeAsync(4001, "Alex");

        // Step 1: Pre-applied 3 days
        await _dbContext.LeaveRequests.AddAsync(new LeaveRequest
        {
            EmployeeId = alex.Id,
            LeaveTypeId = 1, // Paid
            StartDate = new DateTime(2026, 5, 4),
            EndDate = new DateTime(2026, 5, 6),
            LeaveDays = 3,
            Reason = "Personal",
            StatusId = Status.Approved,
            IsActive = true,
            IsDeleted = false
        });

        // Last attendance: Monday 1 June 2026
        await _dbContext.Attendances.AddAsync(new Attendance
        {
            EmployeeId = alex.Id,
            Date = new DateTime(2026, 6, 1),
            StatusId = AttendanceStatus.Present,
            ClockIn = new DateTime(2026, 6, 1, 9, 0, 0),
            ClockOut = new DateTime(2026, 6, 1, 18, 0, 0),
            TotalHours = TimeSpan.FromHours(9),
            Notes = "Present",
            IsActive = true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        // Step 2 & 3: Absent for 5 working days: June 2, 3, 4, 5 (Tue-Fri) + June 8 (Mon) (June 6-7 are weekend)
        // Clocks in on Tuesday June 9
        var clockInResult = await _attendanceService.CreateAttendanceAsync(new AttendanceDto
        {
            EmployeeId = alex.Id,
            Date = new DateTime(2026, 6, 9),
            ClockInTime = TimeSpan.FromHours(9),
            StatusId = AttendanceStatus.Present,
            Notes = "Back to work"
        });

        Assert.That(clockInResult.IsSuccess, Is.True);

        // Verification of Paid Leave
        var paidLeaves = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == alex.Id && l.LeaveTypeId == 1 && !l.IsDeleted)
            .ToListAsync();
        Assert.That(paidLeaves.Sum(l => l.LeaveDays), Is.EqualTo(5), "Paid leave must reach exactly 5 days (3 + 2).");

        // Verification of Unpaid Leave
        var unpaidLeaves = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == alex.Id && l.LeaveTypeId == 2 && !l.IsDeleted)
            .ToListAsync();
        Assert.That(unpaidLeaves.Sum(l => l.LeaveDays), Is.EqualTo(3), "Excess 3 days must be allocated to Unpaid Leave.");
    }

    [Test]
    public async Task UserPersona_NegativeBalanceEmployee_WhenOnlyPaidLeaveEnabled()
    {
        // Scenario:
        // Admin disables Unpaid Leave. Only Paid Leave (quota 5) is allowed.
        // Employee Jordan is absent for 9 working days.
        // Clocks in -> All 9 days deducted from Paid Leave.
        // Balance = 5 - 9 = -4 (negative balance).
        var unpaidType = await _dbContext.LeaveTypes.FindAsync(2);
        if (unpaidType != null) unpaidType.IsActive = false; // Disable Unpaid Leave
        await _dbContext.SaveChangesAsync();

        var jordan = await CreateEmployeeAsync(4002, "Jordan");

        // Last attendance: Friday 1 May 2026
        await _dbContext.Attendances.AddAsync(new Attendance
        {
            EmployeeId = jordan.Id,
            Date = new DateTime(2026, 5, 1),
            StatusId = AttendanceStatus.Present,
            ClockIn = new DateTime(2026, 5, 1, 9, 0, 0),
            ClockOut = new DateTime(2026, 5, 1, 18, 0, 0),
            TotalHours = TimeSpan.FromHours(9),
            Notes = "Present",
            IsActive = true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        // Absent for 9 working days: May 4-8 (5 days) + May 11-14 (4 days). Clocks in May 15.
        var clockInResult = await _attendanceService.CreateAttendanceAsync(new AttendanceDto
        {
            EmployeeId = jordan.Id,
            Date = new DateTime(2026, 5, 15),
            ClockInTime = TimeSpan.FromHours(9),
            StatusId = AttendanceStatus.Present,
            Notes = "Present"
        });

        Assert.That(clockInResult.IsSuccess, Is.True);

        var paidLeaves = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == jordan.Id && l.LeaveTypeId == 1 && !l.IsDeleted)
            .ToListAsync();

        Assert.That(paidLeaves.Sum(l => l.LeaveDays), Is.EqualTo(9));

        // Verify balance calculation
        int quota = 5;
        int used = paidLeaves.Sum(l => l.LeaveDays);
        int remaining = quota - used;

        Assert.That(remaining, Is.EqualTo(-4), "Leave balance must accurately drop to negative -4.");
    }

    [Test]
    public async Task UserPersona_AttendanceRegularization_SoftDeletesAutoLeaveAndRestoresBalance()
    {
        // Scenario:
        // Employee Sam was auto-marked on Days 10, 11, 12, 13, 14 May (5 days).
        // Sam submits Attendance Request for Day 12.
        // HR Admin approves request -> Day 10..11 and Day 13..14 stay as auto-leaves.
        // Day 12 becomes Present.
        // Paid Leave used decreases from 5 to 4, restoring 1 day of balance!
        var sam = await CreateEmployeeAsync(4003, "Sam");

        // Initial attendance on Friday 8 May
        await _dbContext.Attendances.AddAsync(new Attendance
        {
            EmployeeId = sam.Id,
            Date = new DateTime(2026, 5, 8),
            StatusId = AttendanceStatus.Present,
            ClockIn = new DateTime(2026, 5, 8, 9, 0, 0),
            ClockOut = new DateTime(2026, 5, 8, 18, 0, 0),
            TotalHours = TimeSpan.FromHours(9),
            Notes = "Present",
            IsActive = true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        // Absent 11, 12, 13, 14, 15 May (5 days). Clocks in Monday 18 May.
        var clockInResult = await _attendanceService.CreateAttendanceAsync(new AttendanceDto
        {
            EmployeeId = sam.Id,
            Date = new DateTime(2026, 5, 18),
            ClockInTime = TimeSpan.FromHours(9),
            StatusId = AttendanceStatus.Present,
            Notes = "Present"
        });

        Assert.That(clockInResult.IsSuccess, Is.True);

        // Before regularization: 5 days of auto-leave
        var initialLeaves = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == sam.Id && !l.IsDeleted && l.StatusId == Status.Approved)
            .ToListAsync();
        Assert.That(initialLeaves.Sum(l => l.LeaveDays), Is.EqualTo(5));

        // Sam creates attendance request for Wednesday 13 May
        var createReqResult = await _attendanceRequestService.CreateAttendanceRequestAsync(new AttendanceRequestDto
        {
            EmployeeId = sam.Id,
            FromDate = new DateTime(2026, 5, 13),
            ToDate = new DateTime(2026, 5, 13),
            ClockIn = new DateTime(2026, 5, 13, 9, 0, 0),
            ClockOut = new DateTime(2026, 5, 13, 18, 0, 0),
            TotalHours = TimeSpan.FromHours(9),
            BreakDuration = TimeSpan.Zero,
            Description = "Forgot to punch",
            ReasonTypeId = AttendanceReasonType.ForgotToClockIn,
            LocationId = AttendanceLocation.Office
        });
        Assert.That(createReqResult.IsSuccess, Is.True);
        int attReqId = createReqResult.Data!.Id;

        // HR Admin approves the attendance request
        var approveResult = await _attendanceRequestService.UpdateAttendanceRequestStatusAsync(attReqId, Status.Approved, 999);
        Assert.That(approveResult.IsSuccess, Is.True);

        // After regularization:
        // 1. Attendance for 13 May is Present
        var day13Att = await _dbContext.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == sam.Id && a.Date == new DateTime(2026, 5, 13) && !a.IsDeleted);
        Assert.That(day13Att, Is.Not.Null);
        Assert.That(day13Att!.StatusId, Is.EqualTo(AttendanceStatus.Present));

        // 2. Active approved leaves now total 4 days (11-12 May = 2 days, 14-15 May = 2 days)
        var activeLeaves = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == sam.Id && !l.IsDeleted && l.StatusId == Status.Approved)
            .ToListAsync();
        Assert.That(activeLeaves.Sum(l => l.LeaveDays), Is.EqualTo(4), "Leave balance must be credited back by 1 day.");

        // 3. Original 5-day leave is soft-deleted
        var softDeleted = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == sam.Id && l.IsDeleted)
            .ToListAsync();
        Assert.That(softDeleted.Count, Is.GreaterThanOrEqualTo(1));
    }
}
