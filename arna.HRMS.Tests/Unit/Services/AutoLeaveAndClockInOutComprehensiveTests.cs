using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Repository;
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
public class AutoLeaveAndClockInOutComprehensiveTests
{
    private ApplicationDbContext _dbContext = null!;
    private IMapper _mapper = null!;
    private AttendanceRepository _attendanceRepository = null!;
    private LeaveRepository _leaveRepository = null!;
    private EmployeeRepository _employeeRepository = null!;
    private AttendanceRequestRepository _attendanceRequestRepository = null!;
    private Mock<IFestivalHolidayService> _festivalHolidayServiceMock = null!;
    private Mock<IEmployeeService> _employeeServiceMock = null!;

    private LeaveService _leaveService = null!;
    private AttendanceService _attendanceService = null!;
    private AttendanceRequestService _attendanceRequestService = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AttendanceProfile>();
            cfg.AddProfile<LeaveProfile>();
            cfg.AddProfile<AttendanceRequestProfile>();
            cfg.AddProfile<EmployeeProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        var attendanceBaseRepo = new BaseRepository<Attendance>(_dbContext);
        var festivalBaseRepo = new BaseRepository<FestivalHoliday>(_dbContext);
        var employeeBaseRepo = new BaseRepository<Employee>(_dbContext);
        var leaveRequestBaseRepo = new BaseRepository<LeaveRequest>(_dbContext);
        var leaveTypeBaseRepo = new BaseRepository<LeaveType>(_dbContext);
        var attReqBaseRepo = new BaseRepository<AttendanceRequest>(_dbContext);

        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "Engineering",
            Code = "ENG",
            Description = "Engineering Department",
            IsActive = true
        });
        _dbContext.SaveChanges();

        _employeeRepository = new EmployeeRepository(employeeBaseRepo);
        var festivalHolidayRepository = new FestivalHolidayRepository(festivalBaseRepo);
        _attendanceRepository = new AttendanceRepository(attendanceBaseRepo, _employeeRepository, festivalHolidayRepository);
        _leaveRepository = new LeaveRepository(leaveTypeBaseRepo, leaveRequestBaseRepo);
        _attendanceRequestRepository = new AttendanceRequestRepository(attReqBaseRepo);

        _festivalHolidayServiceMock = new Mock<IFestivalHolidayService>();
        _festivalHolidayServiceMock.Setup(x => x.GetFestivalHolidaysAsync())
            .ReturnsAsync(ServiceResult<List<FestivalHolidayDto>>.Success(new List<FestivalHolidayDto>()));

        _employeeServiceMock = new Mock<IEmployeeService>();

        var leaveValidator = new LeaveValidator(_leaveRepository);
        _leaveService = new LeaveService(
            _leaveRepository,
            _festivalHolidayServiceMock.Object,
            _attendanceRepository,
            _mapper,
            leaveValidator
        );

        var attendanceValidator = new AttendanceValidator(_attendanceRepository);
        _attendanceService = new AttendanceService(
            _attendanceRepository,
            _mapper,
            _employeeServiceMock.Object,
            _festivalHolidayServiceMock.Object,
            _leaveService,
            attendanceValidator
        );

        var attReqValidator = new AttendanceRequestValidator(_attendanceRequestRepository);
        _attendanceRequestService = new AttendanceRequestService(
            _attendanceRequestRepository,
            _mapper,
            _attendanceService,
            _leaveService,
            attReqValidator
        );
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private async Task<Employee> CreateEmployeeAsync(int id, string firstName, string lastName)
    {
        var emp = new Employee
        {
            Id = id,
            EmployeeNumber = $"EMP{id:D3}",
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLower()}@example.com",
            PhoneNumber = "9876543210",
            Position = "Developer",
            DepartmentId = 1,
            HireDate = new DateTime(2026, 1, 1),
            IsActive = true
        };
        await _dbContext.Employees.AddAsync(emp);
        await _dbContext.SaveChangesAsync();

        _employeeServiceMock.Setup(e => e.GetEmployeeByIdAsync(id))
            .ReturnsAsync(ServiceResult<EmployeeDto>.Success(new EmployeeDto
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Email = emp.Email,
                PhoneNumber = emp.PhoneNumber,
                Position = emp.Position,
                HireDate = emp.HireDate,
                IsActive = true
            }));

        return emp;
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // FUNCTIONALITY 1: CLOCK IN AND CLOCK OUT SCENARIOS
    // ══════════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task ClockIn_WhenFirstPunchOfDay_CreatesPresentRecordWithClockInTime()
    {
        // Arrange
        await CreateEmployeeAsync(1, "John", "Doe");

        var punchDate = new DateTime(2026, 5, 4); // Monday
        var clockInTime = new TimeSpan(9, 15, 0);

        var dto = new AttendanceDto
        {
            EmployeeId = 1,
            Date = punchDate,
            ClockInTime = clockInTime,
            StatusId = AttendanceStatus.Present,
            DeviceId = DeviceType.LaptopDesktop,
            Notes = "Web Punch"
        };

        // Act
        var result = await _attendanceService.CreateAttendanceAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.ClockInTime, Is.EqualTo(clockInTime));
        Assert.That(result.Data.StatusId, Is.EqualTo(AttendanceStatus.Present));

        var recordInDb = await _dbContext.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == 1 && a.Date == punchDate);
        Assert.That(recordInDb, Is.Not.Null);
        Assert.That(recordInDb!.ClockIn!.Value.TimeOfDay, Is.EqualTo(clockInTime));
    }

    [Test]
    public async Task ClockInAndOut_PunchesAreTrackedInMonthlyAttendanceSummary()
    {
        // Arrange: Mon May 4
        var date = new DateTime(2026, 5, 4);
        var clockIn = new DateTime(2026, 5, 4, 9, 0, 0);
        var clockOut = new DateTime(2026, 5, 4, 17, 30, 0); // 8.5 hours

        var emp = await CreateEmployeeAsync(10, "Mark", "Twain");

        var attendance = new Attendance
        {
            EmployeeId = 10,
            Employee = emp,
            Date = date,
            ClockIn = clockIn,
            ClockOut = clockOut,
            TotalHours = TimeSpan.FromHours(8.5),
            StatusId = AttendanceStatus.Present,
            DeviceId = DeviceType.LaptopDesktop,
            Notes = "Present",
            IsActive = true,
            IsDeleted = false
        };
        await _dbContext.Attendances.AddAsync(attendance);
        await _dbContext.SaveChangesAsync();

        // Act
        var monthlyResult = await _attendanceService.GetAttendanceByMonthAsync(2026, 5, 10, null, null);

        // Assert
        Assert.That(monthlyResult.IsSuccess, Is.True);
        Assert.That(monthlyResult.Data, Is.Not.Null);

        var may4Day = monthlyResult.Data!.FirstOrDefault(m => m.Date == DateOnly.FromDateTime(date));
        Assert.That(may4Day, Is.Not.Null);
        var empDay = may4Day!.Employees.FirstOrDefault(e => e.EmployeeId == 10);
        Assert.That(empDay, Is.Not.Null);
        Assert.That(empDay!.Status, Is.EqualTo("Present"));
        Assert.That(empDay.ClockIn, Is.EqualTo(new TimeSpan(9, 0, 0)));
        Assert.That(empDay.ClockOut, Is.EqualTo(new TimeSpan(17, 30, 0)));
        Assert.That(empDay.WorkingHours, Is.EqualTo(TimeSpan.FromHours(8.5)));
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // FUNCTIONALITY 2: AUTO LEAVE SCENARIOS
    // ══════════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task AutoLeave_WhenMissingWorkdaysBetweenPunches_CreatesAutoLeaveExcludingWeekends()
    {
        // Arrange
        // Monday May 4: Clocked in
        // Friday May 8: Clocked in
        // Missing: Tue May 5, Wed May 6, Thu May 7 (3 weekdays)
        await CreateEmployeeAsync(1, "Alice", "Smith");

        var paidType = new LeaveType
        {
            Id = 1,
            LeaveNameId = LeaveName.PaidLeave,
            MaxPerYear = 15,
            IsPaid = true,
            IsActive = true,
            IsDeleted = false
        };
        await _dbContext.LeaveTypes.AddAsync(paidType);

        var prevPunch = new Attendance
        {
            EmployeeId = 1,
            Date = new DateTime(2026, 5, 4), // Monday
            ClockIn = new DateTime(2026, 5, 4, 9, 0, 0),
            ClockOut = new DateTime(2026, 5, 4, 17, 0, 0),
            TotalHours = TimeSpan.FromHours(8),
            StatusId = AttendanceStatus.Present,
            Notes = "Punch",
            IsActive = true,
            IsDeleted = false
        };
        await _dbContext.Attendances.AddAsync(prevPunch);
        await _dbContext.SaveChangesAsync();

        // Act: Clock in on Friday May 8
        var fridayPunch = new AttendanceDto
        {
            EmployeeId = 1,
            Date = new DateTime(2026, 5, 8), // Friday
            ClockInTime = new TimeSpan(9, 0, 0),
            StatusId = AttendanceStatus.Present,
            DeviceId = DeviceType.LaptopDesktop,
            Notes = "Web Clock In"
        };
        var result = await _attendanceService.CreateAttendanceAsync(fridayPunch);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        // Auto leave attendance records created for May 5, 6, 7
        var autoAttendances = await _dbContext.Attendances
            .Where(a => a.EmployeeId == 1 && a.StatusId == AttendanceStatus.Leave && !a.IsDeleted)
            .OrderBy(a => a.Date)
            .ToListAsync();

        Assert.That(autoAttendances.Count, Is.EqualTo(3));
        Assert.That(autoAttendances.Select(a => a.Date.Day).ToList(), Is.EqualTo(new List<int> { 5, 6, 7 }));

        // Auto approved LeaveRequest created covering May 5 to May 7
        var autoLeaves = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == 1 && !l.IsDeleted && l.StatusId == Status.Approved)
            .ToListAsync();

        Assert.That(autoLeaves.Count, Is.EqualTo(1));
        Assert.That(autoLeaves[0].StartDate, Is.EqualTo(new DateTime(2026, 5, 5)));
        Assert.That(autoLeaves[0].EndDate, Is.EqualTo(new DateTime(2026, 5, 7)));
        Assert.That(autoLeaves[0].LeaveDays, Is.EqualTo(3));
        Assert.That(autoLeaves[0].LeaveTypeId, Is.EqualTo(paidType.Id));
    }

    [Test]
    public async Task AutoLeave_WhenAdminAllowsBothPaidAndUnpaid_CutsPaidFirstThenUnpaid()
    {
        // Arrange
        // Quota: Paid Leave has MaxPerYear = 12. Employee has already used 11 days.
        // Remaining paid days = 1.
        // Employee has missing 3 days: May 5, May 6, May 7.
        // Expected: May 5 -> Paid Leave (1 day), May 6-7 -> Unpaid Leave (2 days).
        await CreateEmployeeAsync(2, "Bob", "Taylor");

        var paidType = new LeaveType
        {
            Id = 1,
            LeaveNameId = LeaveName.PaidLeave,
            MaxPerYear = 12,
            IsPaid = true,
            IsActive = true,
            IsDeleted = false
        };
        var unpaidType = new LeaveType
        {
            Id = 2,
            LeaveNameId = LeaveName.UnpaidLeave,
            MaxPerYear = 30,
            IsPaid = false,
            IsActive = true,
            IsDeleted = false
        };
        await _dbContext.LeaveTypes.AddRangeAsync(paidType, unpaidType);

        // Pre-seed 11 used paid days in current FY (starts 2026-04-01)
        await _dbContext.LeaveRequests.AddAsync(new LeaveRequest
        {
            EmployeeId = 2,
            LeaveTypeId = paidType.Id,
            StartDate = new DateTime(2026, 4, 6),
            EndDate = new DateTime(2026, 4, 20),
            LeaveDays = 11,
            Reason = "Pre-existing Leave",
            StatusId = Status.Approved,
            IsActive = true,
            IsDeleted = false
        });

        // Previous punch on Monday May 4
        await _dbContext.Attendances.AddAsync(new Attendance
        {
            EmployeeId = 2,
            Date = new DateTime(2026, 5, 4),
            ClockIn = new DateTime(2026, 5, 4, 9, 0, 0),
            ClockOut = new DateTime(2026, 5, 4, 17, 0, 0),
            TotalHours = TimeSpan.FromHours(8),
            StatusId = AttendanceStatus.Present,
            Notes = "Punch",
            IsActive = true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        // Act: Clock in on Friday May 8
        var punch = new AttendanceDto
        {
            EmployeeId = 2,
            Date = new DateTime(2026, 5, 8),
            ClockInTime = new TimeSpan(9, 0, 0),
            StatusId = AttendanceStatus.Present,
            DeviceId = DeviceType.LaptopDesktop,
            Notes = "Web Clock In"
        };
        var result = await _attendanceService.CreateAttendanceAsync(punch);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var createdLeaves = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == 2 && l.StartDate >= new DateTime(2026, 5, 1) && !l.IsDeleted && l.StatusId == Status.Approved)
            .OrderBy(l => l.StartDate)
            .ToListAsync();

        // Must have created 2 segments: 1 Paid (May 5), 1 Unpaid (May 6-7)
        Assert.That(createdLeaves.Count, Is.EqualTo(2));

        var paidSegment = createdLeaves.FirstOrDefault(l => l.LeaveTypeId == paidType.Id);
        Assert.That(paidSegment, Is.Not.Null);
        Assert.That(paidSegment!.StartDate, Is.EqualTo(new DateTime(2026, 5, 5)));
        Assert.That(paidSegment.EndDate, Is.EqualTo(new DateTime(2026, 5, 5)));
        Assert.That(paidSegment.LeaveDays, Is.EqualTo(1));

        var unpaidSegment = createdLeaves.FirstOrDefault(l => l.LeaveTypeId == unpaidType.Id);
        Assert.That(unpaidSegment, Is.Not.Null);
        Assert.That(unpaidSegment!.StartDate, Is.EqualTo(new DateTime(2026, 5, 6)));
        Assert.That(unpaidSegment.EndDate, Is.EqualTo(new DateTime(2026, 5, 7)));
        Assert.That(unpaidSegment.LeaveDays, Is.EqualTo(2));
    }

    [Test]
    public async Task AutoLeave_WhenAdminAllowsOnlyPaidLeave_CutsFromPaidLeaveOnly()
    {
        // Arrange: Unpaid leave is INACTIVE (Admin disabled it)
        await CreateEmployeeAsync(3, "Charlie", "Brown");

        var paidType = new LeaveType
        {
            Id = 1,
            LeaveNameId = LeaveName.PaidLeave,
            MaxPerYear = 15,
            IsPaid = true,
            IsActive = true,
            IsDeleted = false
        };
        var inactiveUnpaidType = new LeaveType
        {
            Id = 2,
            LeaveNameId = LeaveName.UnpaidLeave,
            MaxPerYear = 0,
            IsPaid = false,
            IsActive = false, // Disabled by Admin!
            IsDeleted = false
        };
        await _dbContext.LeaveTypes.AddRangeAsync(paidType, inactiveUnpaidType);

        await _dbContext.Attendances.AddAsync(new Attendance
        {
            EmployeeId = 3,
            Date = new DateTime(2026, 5, 4),
            ClockIn = new DateTime(2026, 5, 4, 9, 0, 0),
            ClockOut = new DateTime(2026, 5, 4, 17, 0, 0),
            TotalHours = TimeSpan.FromHours(8),
            StatusId = AttendanceStatus.Present,
            Notes = "Punch",
            IsActive = true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        // Act: Clock in on Friday May 8 (May 5, 6, 7 missing)
        var punch = new AttendanceDto
        {
            EmployeeId = 3,
            Date = new DateTime(2026, 5, 8),
            ClockInTime = new TimeSpan(9, 0, 0),
            StatusId = AttendanceStatus.Present,
            DeviceId = DeviceType.LaptopDesktop,
            Notes = "Web Clock In"
        };
        var result = await _attendanceService.CreateAttendanceAsync(punch);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var createdLeaves = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == 3 && !l.IsDeleted && l.StatusId == Status.Approved)
            .ToListAsync();

        // Must cut ONLY from paid leave
        Assert.That(createdLeaves.All(l => l.LeaveTypeId == paidType.Id), Is.True);
        Assert.That(createdLeaves.Any(l => l.LeaveTypeId == inactiveUnpaidType.Id), Is.False);
    }

    [Test]
    public async Task AutoLeave_WhenAttendanceRequestApprovedInMiddleOfLeave_RevertsAutoLeave_SplitsSegments_AndRestoresLeaveBalance()
    {
        // Arrange:
        // Employee forgot to clock in in middle of week.
        // Auto-leave was generated from Mon May 11 to Fri May 15 (5 days).
        var start = new DateTime(2026, 5, 11); // Monday
        var end = new DateTime(2026, 5, 15);   // Friday (5 days)

        var emp = await CreateEmployeeAsync(4, "Diana", "Prince");
        emp.HireDate = start;
        await _dbContext.SaveChangesAsync();

        _employeeServiceMock.Setup(e => e.GetEmployeeByIdAsync(4))
            .ReturnsAsync(ServiceResult<EmployeeDto>.Success(new EmployeeDto
            {
                Id = 4,
                FirstName = "Diana",
                LastName = "Prince",
                HireDate = start,
                IsActive = true
            }));

        var paidType = new LeaveType
        {
            Id = 1,
            LeaveNameId = LeaveName.PaidLeave,
            MaxPerYear = 20,
            IsPaid = true,
            IsActive = true,
            IsDeleted = false
        };
        await _dbContext.LeaveTypes.AddAsync(paidType);

        var autoLeave = new LeaveRequest
        {
            EmployeeId = 4,
            LeaveTypeId = paidType.Id,
            StartDate = start,
            EndDate = end,
            LeaveDays = 5,
            Reason = "Auto Leave Marked",
            StatusId = Status.Approved,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        };
        await _dbContext.LeaveRequests.AddAsync(autoLeave);

        for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
        {
            await _dbContext.Attendances.AddAsync(new Attendance
            {
                EmployeeId = 4,
                Date = d,
                StatusId = AttendanceStatus.Leave,
                Notes = "Auto Leave Marked",
                IsActive = true,
                IsDeleted = false
            });
        }
        await _dbContext.SaveChangesAsync();

        // Balance before: 20 total - 5 used = 15 remaining
        var usedBefore = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == 4 && !l.IsDeleted && l.StatusId == Status.Approved)
            .SumAsync(l => l.LeaveDays);
        Assert.That(usedBefore, Is.EqualTo(5));

        // Act: Employee submits an Attendance Request for Wednesday May 13 (forgot to clock in)
        var wednesday = new DateTime(2026, 5, 13);
        var clockIn = new DateTime(2026, 5, 13, 9, 0, 0);
        var clockOut = new DateTime(2026, 5, 13, 18, 0, 0);

        var attReq = new AttendanceRequest
        {
            EmployeeId = 4,
            FromDate = wednesday,
            ToDate = wednesday,
            ClockIn = clockIn,
            ClockOut = clockOut,
            Description = "Forgot to clock in during shift",
            ReasonTypeId = AttendanceReasonType.ForgotToClockIn,
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        };
        await _dbContext.AttendanceRequest.AddAsync(attReq);
        await _dbContext.SaveChangesAsync();

        // Admin approves the request
        var approveResult = await _attendanceRequestService.UpdateAttendanceRequestStatusAsync(
            attReq.Id,
            Status.Approved,
            approvedBy: 99
        );

        // Assert
        Assert.That(approveResult.IsSuccess, Is.True);

        // 1. The original 5-day auto-leave request must be soft-deleted
        var oldLeave = await _dbContext.LeaveRequests.FindAsync(autoLeave.Id);
        Assert.That(oldLeave!.IsDeleted, Is.True);
        Assert.That(oldLeave.IsActive, Is.False);

        // 2. Two new adjusted leave segments created:
        // Segment A: Mon May 11 to Tue May 12 (2 days)
        // Segment B: Thu May 14 to Fri May 15 (2 days)
        var activeLeaves = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == 4 && !l.IsDeleted && l.StatusId == Status.Approved)
            .OrderBy(l => l.StartDate)
            .ToListAsync();

        Assert.That(activeLeaves.Count, Is.EqualTo(2));

        Assert.That(activeLeaves[0].StartDate, Is.EqualTo(new DateTime(2026, 5, 11)));
        Assert.That(activeLeaves[0].EndDate, Is.EqualTo(new DateTime(2026, 5, 12)));
        Assert.That(activeLeaves[0].LeaveDays, Is.EqualTo(2));

        Assert.That(activeLeaves[1].StartDate, Is.EqualTo(new DateTime(2026, 5, 14)));
        Assert.That(activeLeaves[1].EndDate, Is.EqualTo(new DateTime(2026, 5, 15)));
        Assert.That(activeLeaves[1].LeaveDays, Is.EqualTo(2));

        // 3. Attendance record for Wed May 13 is now Present with 9 hours
        var wednesdayAttendance = await _dbContext.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == 4 && a.Date == wednesday && !a.IsDeleted);

        Assert.That(wednesdayAttendance, Is.Not.Null);
        Assert.That(wednesdayAttendance!.StatusId, Is.EqualTo(AttendanceStatus.Present));
        Assert.That(wednesdayAttendance.TotalHours, Is.EqualTo(TimeSpan.FromHours(9)));

        // 4. Leave Balance restoration:
        // Used days is now 2 + 2 = 4 days instead of 5 days.
        // Exactly 1 day was refunded to employee balance!
        var usedAfter = activeLeaves.Sum(l => l.LeaveDays);
        Assert.That(usedAfter, Is.EqualTo(4));
        var remainingBalance = paidType.MaxPerYear - usedAfter;
        Assert.That(remainingBalance, Is.EqualTo(16)); // 15 + 1 restored = 16!
    }
}
