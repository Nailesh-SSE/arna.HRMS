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
public class AutoLeaveDuplicatePreventionTests
{
    private ApplicationDbContext _dbContext = null!;
    private LeaveService _leaveService = null!;
    private AttendanceService _attendanceService = null!;
    private FestivalHolidayService _festivalHolidayService = null!;
    private AttendanceRepository _attendanceRepository = null!;
    private LeaveRepository _leaveRepository = null!;
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

        _dbContext = new ApplicationDbContext(options);

        var leaveRequestBaseRepo = new BaseRepository<LeaveRequest>(_dbContext);
        var leaveTypeBaseRepo = new BaseRepository<LeaveType>(_dbContext);
        var attendanceBaseRepo = new BaseRepository<Attendance>(_dbContext);
        var festivalBaseRepo = new BaseRepository<FestivalHoliday>(_dbContext);
        var employeeBaseRepo = new BaseRepository<Employee>(_dbContext);

        _leaveRepository = new LeaveRepository(leaveTypeBaseRepo, leaveRequestBaseRepo);
        _festivalHolidayRepository = new FestivalHolidayRepository(festivalBaseRepo);
        _employeeRepository = new EmployeeRepository(employeeBaseRepo);
        _attendanceRepository = new AttendanceRepository(attendanceBaseRepo, _employeeRepository, _festivalHolidayRepository);

        var leaveValidator = new LeaveValidator(_leaveRepository);
        var attendanceValidator = new AttendanceValidator(_attendanceRepository);
        var festivalValidator = new FestivalHolidayValidator(_festivalHolidayRepository);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AttendanceProfile>();
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

        // Seed basic department and leave type
        _dbContext.Departments.Add(new Department { Id = 1, Name = "IT", Code = "IT", Description = "IT" });
        _dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = 1,
            LeaveNameId = LeaveName.CasualLeave,
            Description = "Casual Leave",
            MaxPerYear = 12,
            IsPaid = true,
            IsActive = true,
            IsDeleted = false
        });
        _dbContext.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private async Task<Employee> SeedEmployee(int empId = 101)
    {
        var emp = new Employee
        {
            Id = empId,
            EmployeeNumber = $"EMP{empId}",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@arna.com",
            PhoneNumber = "9999999999",
            Position = "Developer",
            Salary = 60000,
            DepartmentId = 1,
            HireDate = new DateTime(2025, 1, 1),
            DateOfBirth = new DateTime(1995, 1, 1),
            IsActive = true,
            IsDeleted = false
        };
        await _dbContext.Employees.AddAsync(emp);
        await _dbContext.SaveChangesAsync();

        _employeeServiceMock.Setup(e => e.GetEmployeeByIdAsync(empId))
            .ReturnsAsync(ServiceResult<EmployeeDto?>.Success(new EmployeeDto
            {
                Id = empId,
                HireDate = emp.HireDate,
                FirstName = emp.FirstName,
                LastName = emp.LastName
            }));

        return emp;
    }

    [Test]
    public async Task ClockIn_WhenEmployeeHasPendingLeave_DoesNotMarkAutoLeaveOnThosePendingDays()
    {
        var emp = await SeedEmployee(101);

        // Previous attendance on Mon 4 May 2026
        await _dbContext.Attendances.AddAsync(new Attendance
        {
            EmployeeId = emp.Id,
            Date = new DateTime(2026, 5, 4),
            StatusId = AttendanceStatus.Present,
            ClockIn = new DateTime(2026, 5, 4, 9, 0, 0),
            ClockOut = new DateTime(2026, 5, 4, 18, 0, 0),
            TotalHours = TimeSpan.FromHours(9),
            Notes = "Normal Clock In",
            IsActive = true,
            IsDeleted = false
        });

        // Pending Leave Request for 5-7 May
        await _dbContext.LeaveRequests.AddAsync(new LeaveRequest
        {
            Id = 1,
            EmployeeId = emp.Id,
            LeaveTypeId = 1,
            StartDate = new DateTime(2026, 5, 5),
            EndDate = new DateTime(2026, 5, 7),
            LeaveDays = 3,
            Reason = "Personal Emergency",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        });

        await _dbContext.SaveChangesAsync();

        // Act: Employee clocks in on Mon 11 May 2026
        var clockInResult = await _attendanceService.CreateAttendanceAsync(new AttendanceDto
        {
            EmployeeId = emp.Id,
            Date = new DateTime(2026, 5, 11),
            ClockInTime = TimeSpan.FromHours(9),
            StatusId = AttendanceStatus.Present,
            Notes = "Clock In after leave"
        });

        Assert.That(clockInResult.IsSuccess, Is.True);

        // Assert: Check Attendance records between 5 May and 10 May
        var attendances = await _dbContext.Attendances
            .Where(a => a.EmployeeId == emp.Id && !a.IsDeleted &&
                        a.Date >= new DateTime(2026, 5, 5) && a.Date <= new DateTime(2026, 5, 10))
            .ToListAsync();

        // Only 8 May should have been marked as Auto Leave
        Assert.That(attendances.Count, Is.EqualTo(1));
        Assert.That(attendances[0].Date, Is.EqualTo(new DateTime(2026, 5, 8)));
        Assert.That(attendances[0].StatusId, Is.EqualTo(AttendanceStatus.Leave));
        Assert.That(attendances[0].Notes, Is.EqualTo("Auto Leave Marked"));

        // Check Leave Requests: Only original Pending request (1) and new Auto Leave for 8 May (2)
        var allLeaves = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == emp.Id && !l.IsDeleted)
            .OrderBy(l => l.StartDate)
            .ToListAsync();

        Assert.That(allLeaves.Count, Is.EqualTo(2));

        // Original Pending leave remains untouched
        Assert.That(allLeaves[0].Id, Is.EqualTo(1));
        Assert.That(allLeaves[0].StatusId, Is.EqualTo(Status.Pending));
        Assert.That(allLeaves[0].StartDate, Is.EqualTo(new DateTime(2026, 5, 5)));
        Assert.That(allLeaves[0].EndDate, Is.EqualTo(new DateTime(2026, 5, 7)));

        // Auto leave was ONLY created for Friday 8 May
        Assert.That(allLeaves[1].Reason, Is.EqualTo("Auto Leave Marked"));
        Assert.That(allLeaves[1].StartDate, Is.EqualTo(new DateTime(2026, 5, 8)));
        Assert.That(allLeaves[1].EndDate, Is.EqualTo(new DateTime(2026, 5, 8)));
        Assert.That(allLeaves[1].LeaveDays, Is.EqualTo(1));
    }

    [Test]
    public async Task WhenAdminApprovesPendingLeave_AfterAutoLeaveCreated_NoDuplicateLeavesAndLeaveDaysConsolidated()
    {
        // Scenario 2:
        // Auto-leave was already marked from 5 May to 8 May (4 days) as "Auto Leave Marked" (Status.Approved).
        // Employee subsequently had a formal leave request for 6 May to 7 May (2 days) that Admin approves now.
        // Expected:
        // Pre-existing overlapping auto-leave is soft-deleted.
        // Left segment (5 May, 1 day) is created as "Auto Adjusted".
        // Right segment (8 May, 1 day) is created as "Auto Adjusted".
        // The admin-approved leave (6-7 May, 2 days) is Approved.
        // Total active approved leave days = 1 + 2 + 1 = 4 days (no duplication, no double deduction!).
        var emp = await SeedEmployee(102);

        // Pre-existing Auto Leave (5-8 May)
        var autoLeave = new LeaveRequest
        {
            Id = 10,
            EmployeeId = emp.Id,
            LeaveTypeId = 1,
            StartDate = new DateTime(2026, 5, 5),
            EndDate = new DateTime(2026, 5, 8),
            LeaveDays = 4,
            Reason = "Auto Leave Marked",
            StatusId = Status.Approved,
            ApprovalNotes = "Auto Approved",
            ApprovedBy = 99,
            ApprovedDate = DateTime.Now,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        };
        await _dbContext.LeaveRequests.AddAsync(autoLeave);

        // Pending formal leave request for 6-7 May
        var formalLeave = new LeaveRequest
        {
            Id = 11,
            EmployeeId = emp.Id,
            LeaveTypeId = 1,
            StartDate = new DateTime(2026, 5, 6),
            EndDate = new DateTime(2026, 5, 7),
            LeaveDays = 2,
            Reason = "Family Function",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        };
        await _dbContext.LeaveRequests.AddAsync(formalLeave);
        await _dbContext.SaveChangesAsync();

        // Act: Admin approves the formal leave request (Id: 11)
        var approveResult = await _leaveService.UpdateLeaveRequestStatusAsync(11, Status.Approved, 99);

        Assert.That(approveResult.IsSuccess, Is.True);

        // Assert: Original auto leave (10) must be soft deleted
        var oldAutoLeave = await _dbContext.LeaveRequests.FindAsync(10);
        Assert.That(oldAutoLeave!.IsDeleted, Is.True);
        Assert.That(oldAutoLeave.IsActive, Is.False);

        // Active approved leaves
        var activeLeaves = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == emp.Id && !l.IsDeleted && l.IsActive && l.StatusId == Status.Approved)
            .OrderBy(l => l.StartDate)
            .ToListAsync();

        Assert.That(activeLeaves.Count, Is.EqualTo(3));

        // 1. Left segment: 5 May (1 day)
        Assert.That(activeLeaves[0].StartDate, Is.EqualTo(new DateTime(2026, 5, 5)));
        Assert.That(activeLeaves[0].EndDate, Is.EqualTo(new DateTime(2026, 5, 5)));
        Assert.That(activeLeaves[0].LeaveDays, Is.EqualTo(1));
        Assert.That(activeLeaves[0].ApprovalNotes, Is.EqualTo("Auto Adjusted"));

        // 2. Formal approved leave: 6-7 May (2 days)
        Assert.That(activeLeaves[1].Id, Is.EqualTo(11));
        Assert.That(activeLeaves[1].StartDate, Is.EqualTo(new DateTime(2026, 5, 6)));
        Assert.That(activeLeaves[1].EndDate, Is.EqualTo(new DateTime(2026, 5, 7)));
        Assert.That(activeLeaves[1].LeaveDays, Is.EqualTo(2));
        Assert.That(activeLeaves[1].Reason, Is.EqualTo("Family Function"));

        // 3. Right segment: 8 May (1 day)
        Assert.That(activeLeaves[2].StartDate, Is.EqualTo(new DateTime(2026, 5, 8)));
        Assert.That(activeLeaves[2].EndDate, Is.EqualTo(new DateTime(2026, 5, 8)));
        Assert.That(activeLeaves[2].LeaveDays, Is.EqualTo(1));
        Assert.That(activeLeaves[2].ApprovalNotes, Is.EqualTo("Auto Adjusted"));

        // Sum of all leave days = exactly 4 days
        Assert.That(activeLeaves.Sum(l => l.LeaveDays), Is.EqualTo(4));
    }

    [Test]
    public async Task WhenAdminApprovesPendingLeave_MatchingExactAutoLeaveDates_AutoLeaveIsSoftDeletedWithNoDuplicate()
    {
        // Scenario 3:
        // Auto-leave exists for 5-7 May (3 days).
        // Formal leave request is also for 5-7 May (3 days).
        // When Admin approves formal request, auto-leave is soft-deleted.
        // Exactly ONE approved leave record remains active for 5-7 May with 3 days total.
        var emp = await SeedEmployee(103);

        var autoLeave = new LeaveRequest
        {
            Id = 20,
            EmployeeId = emp.Id,
            LeaveTypeId = 1,
            StartDate = new DateTime(2026, 5, 5),
            EndDate = new DateTime(2026, 5, 7),
            LeaveDays = 3,
            Reason = "Auto Leave Marked",
            StatusId = Status.Approved,
            ApprovalNotes = "Auto Approved",
            ApprovedBy = 99,
            ApprovedDate = DateTime.Now,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        };
        await _dbContext.LeaveRequests.AddAsync(autoLeave);

        var formalLeave = new LeaveRequest
        {
            Id = 21,
            EmployeeId = emp.Id,
            LeaveTypeId = 1,
            StartDate = new DateTime(2026, 5, 5),
            EndDate = new DateTime(2026, 5, 7),
            LeaveDays = 3,
            Reason = "Medical Leave",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        };
        await _dbContext.LeaveRequests.AddAsync(formalLeave);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _leaveService.UpdateLeaveRequestStatusAsync(21, Status.Approved, 99);
        Assert.That(result.IsSuccess, Is.True);

        // Assert: Auto leave is soft-deleted
        var old = await _dbContext.LeaveRequests.FindAsync(20);
        Assert.That(old!.IsDeleted, Is.True);

        // Only formal leave is active
        var active = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == emp.Id && !l.IsDeleted && l.IsActive && l.StatusId == Status.Approved)
            .ToListAsync();

        Assert.That(active.Count, Is.EqualTo(1));
        Assert.That(active[0].Id, Is.EqualTo(21));
        Assert.That(active[0].LeaveDays, Is.EqualTo(3));
        Assert.That(active[0].Reason, Is.EqualTo("Medical Leave"));
    }

    [Test]
    public async Task AutoLeave_SkipsHolidaysAndWeekendsProperly()
    {
        // Scenario 4:
        // Last attendance on Thursday 7 May 2026.
        // Friday 8 May 2026 is a Festival Holiday ("Victory Day").
        // Sat 9 and Sun 10 May are weekends.
        // Mon 11 May is a normal working day.
        // Tue 12 May employee clocks in.
        // Auto-leave should ONLY mark Mon 11 May as Leave.
        var emp = await SeedEmployee(104);

        // Add festival holiday on Friday 8 May
        await _dbContext.FestivalHoliday.AddAsync(new FestivalHoliday
        {
            Id = 1,
            FestivalName = "Victory Day",
            Description = "Holiday",
            Date = new DateTime(2026, 5, 8),
            DayOfWeek = DayOfWeek.Friday.ToString(),
            IsActive = true,
            IsDeleted = false
        });

        // Last attendance on Thursday 7 May
        await _dbContext.Attendances.AddAsync(new Attendance
        {
            EmployeeId = emp.Id,
            Date = new DateTime(2026, 5, 7),
            StatusId = AttendanceStatus.Present,
            ClockIn = new DateTime(2026, 5, 7, 9, 0, 0),
            ClockOut = new DateTime(2026, 5, 7, 18, 0, 0),
            TotalHours = TimeSpan.FromHours(9),
            Notes = "Clock In",
            IsActive = true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        // Act: Employee clocks in on Tue 12 May
        var clockInResult = await _attendanceService.CreateAttendanceAsync(new AttendanceDto
        {
            EmployeeId = emp.Id,
            Date = new DateTime(2026, 5, 12),
            ClockInTime = TimeSpan.FromHours(9),
            StatusId = AttendanceStatus.Present,
            Notes = "Present"
        });

        Assert.That(clockInResult.IsSuccess, Is.True);

        // Assert: Auto leave should only exist for Mon 11 May
        var markedLeaves = await _dbContext.Attendances
            .Where(a => a.EmployeeId == emp.Id && !a.IsDeleted &&
                        a.Date >= new DateTime(2026, 5, 8) && a.Date <= new DateTime(2026, 5, 11))
            .ToListAsync();

        Assert.That(markedLeaves.Count, Is.EqualTo(1));
        Assert.That(markedLeaves[0].Date, Is.EqualTo(new DateTime(2026, 5, 11)));
        Assert.That(markedLeaves[0].StatusId, Is.EqualTo(AttendanceStatus.Leave));

        // Leave request created only for 11 May
        var leaveRequests = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == emp.Id && !l.IsDeleted && l.Reason == "Auto Leave Marked")
            .ToListAsync();

        Assert.That(leaveRequests.Count, Is.EqualTo(1));
        Assert.That(leaveRequests[0].StartDate, Is.EqualTo(new DateTime(2026, 5, 11)));
        Assert.That(leaveRequests[0].EndDate, Is.EqualTo(new DateTime(2026, 5, 11)));
        Assert.That(leaveRequests[0].LeaveDays, Is.EqualTo(1));
    }

    [Test]
    public async Task AutoLeave_WhenBothPaidAndUnpaidAllowed_DeductsFromPaidLeaveFirst_ThenUnpaidLeave_WhenPaidQuotaExceeded()
    {
        // Arrange
        // Admin configures Paid Leave (quota = 5) and Unpaid Leave (active)
        var paidType = new LeaveType
        {
            Id = 10,
            LeaveNameId = LeaveName.PaidLeave,
            Description = "Paid Leave",
            MaxPerYear = 5,
            IsPaid = true,
            IsActive = true,
            IsDeleted = false
        };
        var unpaidType = new LeaveType
        {
            Id = 20,
            LeaveNameId = LeaveName.UnpaidLeave,
            Description = "Unpaid Leave",
            MaxPerYear = 30,
            IsPaid = false,
            IsActive = true,
            IsDeleted = false
        };
        // Disable default leave type from setup
        var defaultLt = await _dbContext.LeaveTypes.FindAsync(1);
        if (defaultLt != null) defaultLt.IsActive = false;

        await _dbContext.LeaveTypes.AddRangeAsync(paidType, unpaidType);
        await _dbContext.SaveChangesAsync();

        var emp = await SeedEmployee(201);

        // Last attendance: Monday 1 June 2026
        await _dbContext.Attendances.AddAsync(new Attendance
        {
            EmployeeId = emp.Id,
            Date = new DateTime(2026, 6, 1),
            StatusId = AttendanceStatus.Present,
            ClockIn = new DateTime(2026, 6, 1, 9, 0, 0),
            ClockOut = new DateTime(2026, 6, 1, 18, 0, 0),
            TotalHours = TimeSpan.FromHours(9),
            Notes = "Clock In",
            IsActive = true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        // Employee is absent from Tue 2 June to Wed 10 June (7 working days: 2, 3, 4, 5 June + 8, 9, 10 June; 6 & 7 June are weekend)
        // Act: Employee clocks in on Thursday 11 June
        var clockInResult = await _attendanceService.CreateAttendanceAsync(new AttendanceDto
        {
            EmployeeId = emp.Id,
            Date = new DateTime(2026, 6, 11),
            ClockInTime = TimeSpan.FromHours(9),
            StatusId = AttendanceStatus.Present,
            Notes = "Present"
        });

        Assert.That(clockInResult.IsSuccess, Is.True);

        // Assert:
        // 1. Paid Leave request should have exactly 5 days (reaching the 5-day quota)
        var paidRequests = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == emp.Id && !l.IsDeleted && l.LeaveTypeId == paidType.Id)
            .ToListAsync();
        Assert.That(paidRequests.Sum(l => l.LeaveDays), Is.EqualTo(5));

        // 2. Unpaid Leave request should have the remaining 2 days (7 - 5 = 2)
        var unpaidRequests = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == emp.Id && !l.IsDeleted && l.LeaveTypeId == unpaidType.Id)
            .ToListAsync();
        Assert.That(unpaidRequests.Sum(l => l.LeaveDays), Is.EqualTo(2));

        // 3. Paid balance calculation: Quota (5) - Used (5) = 0 remaining
        int usedPaid = paidRequests.Sum(l => l.LeaveDays);
        int remainingPaid = paidType.MaxPerYear - usedPaid;
        Assert.That(remainingPaid, Is.EqualTo(0));
    }

    [Test]
    public async Task AutoLeave_WhenPaidQuotaAlreadyExhausted_DeductsEntirelyFromUnpaidLeave()
    {
        // Arrange
        var paidType = new LeaveType
        {
            Id = 11,
            LeaveNameId = LeaveName.PaidLeave,
            Description = "Paid Leave",
            MaxPerYear = 5,
            IsPaid = true,
            IsActive = true,
            IsDeleted = false
        };
        var unpaidType = new LeaveType
        {
            Id = 21,
            LeaveNameId = LeaveName.UnpaidLeave,
            Description = "Unpaid Leave",
            MaxPerYear = 30,
            IsPaid = false,
            IsActive = true,
            IsDeleted = false
        };
        var defaultLt = await _dbContext.LeaveTypes.FindAsync(1);
        if (defaultLt != null) defaultLt.IsActive = false;

        await _dbContext.LeaveTypes.AddRangeAsync(paidType, unpaidType);
        await _dbContext.SaveChangesAsync();

        var emp = await SeedEmployee(202);

        // Pre-existing approved Paid Leave that uses up all 5 days of Paid Leave quota
        await _dbContext.LeaveRequests.AddAsync(new LeaveRequest
        {
            EmployeeId = emp.Id,
            LeaveTypeId = paidType.Id,
            StartDate = new DateTime(2026, 4, 6),
            EndDate = new DateTime(2026, 4, 10),
            LeaveDays = 5,
            Reason = "Vacation",
            StatusId = Status.Approved,
            IsActive = true,
            IsDeleted = false
        });

        // Last attendance: Monday 15 June 2026
        await _dbContext.Attendances.AddAsync(new Attendance
        {
            EmployeeId = emp.Id,
            Date = new DateTime(2026, 6, 15),
            StatusId = AttendanceStatus.Present,
            ClockIn = new DateTime(2026, 6, 15, 9, 0, 0),
            ClockOut = new DateTime(2026, 6, 15, 18, 0, 0),
            TotalHours = TimeSpan.FromHours(9),
            Notes = "Clock In",
            IsActive = true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        // Employee absent Tue 16, Wed 17, Thu 18 June (3 days) and clocks in on Friday 19 June
        var clockInResult = await _attendanceService.CreateAttendanceAsync(new AttendanceDto
        {
            EmployeeId = emp.Id,
            Date = new DateTime(2026, 6, 19),
            ClockInTime = TimeSpan.FromHours(9),
            StatusId = AttendanceStatus.Present,
            Notes = "Present"
        });

        Assert.That(clockInResult.IsSuccess, Is.True);

        // Assert:
        // Since Paid Leave was already completed (5/5 used), ALL 3 days must be deducted from Unpaid Leave
        var newPaidRequests = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == emp.Id && !l.IsDeleted && l.LeaveTypeId == paidType.Id && l.Reason == "Auto Leave Marked")
            .ToListAsync();
        Assert.That(newPaidRequests, Is.Empty);

        var unpaidRequests = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == emp.Id && !l.IsDeleted && l.LeaveTypeId == unpaidType.Id)
            .ToListAsync();
        Assert.That(unpaidRequests.Sum(l => l.LeaveDays), Is.EqualTo(3));
    }

    [Test]
    public async Task AutoLeave_WhenOnlyPaidLeaveAllowed_DeductsAllFromPaidLeave_AndAllowsNegativeBalance()
    {
        // Arrange
        // Admin ONLY allows Paid Leave (Unpaid Leave is not active)
        var paidType = new LeaveType
        {
            Id = 12,
            LeaveNameId = LeaveName.PaidLeave,
            Description = "Paid Leave",
            MaxPerYear = 5,
            IsPaid = true,
            IsActive = true,
            IsDeleted = false
        };
        var defaultLt = await _dbContext.LeaveTypes.FindAsync(1);
        if (defaultLt != null) defaultLt.IsActive = false;

        await _dbContext.LeaveTypes.AddAsync(paidType);
        await _dbContext.SaveChangesAsync();

        var emp = await SeedEmployee(203);

        // Last attendance: Monday 1 June 2026
        await _dbContext.Attendances.AddAsync(new Attendance
        {
            EmployeeId = emp.Id,
            Date = new DateTime(2026, 6, 1),
            StatusId = AttendanceStatus.Present,
            ClockIn = new DateTime(2026, 6, 1, 9, 0, 0),
            ClockOut = new DateTime(2026, 6, 1, 18, 0, 0),
            TotalHours = TimeSpan.FromHours(9),
            Notes = "Clock In",
            IsActive = true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        // Employee absent for 7 working days (June 2..10) and clocks in on June 11
        var clockInResult = await _attendanceService.CreateAttendanceAsync(new AttendanceDto
        {
            EmployeeId = emp.Id,
            Date = new DateTime(2026, 6, 11),
            ClockInTime = TimeSpan.FromHours(9),
            StatusId = AttendanceStatus.Present,
            Notes = "Present"
        });

        Assert.That(clockInResult.IsSuccess, Is.True);

        // Assert:
        // When only Paid Leave is allowed, all 7 days are deducted from Paid Leave
        var paidRequests = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == emp.Id && !l.IsDeleted && l.LeaveTypeId == paidType.Id)
            .ToListAsync();
        Assert.That(paidRequests.Sum(l => l.LeaveDays), Is.EqualTo(7));

        // Balance calculation: Quota (5) - Used (7) = -2 (negative balance)
        int usedPaid = paidRequests.Sum(l => l.LeaveDays);
        int remainingPaid = paidType.MaxPerYear - usedPaid;
        Assert.That(remainingPaid, Is.EqualTo(-2));
        Assert.That(remainingPaid < 0, Is.True);
    }

    [Test]
    public async Task AutoLeave_WhenOnlyUnpaidLeaveAllowed_DeductsAllFromUnpaidLeave()
    {
        // Arrange
        // Admin ONLY allows Unpaid Leave (Paid Leave is not active)
        var unpaidType = new LeaveType
        {
            Id = 22,
            LeaveNameId = LeaveName.UnpaidLeave,
            Description = "Unpaid Leave",
            MaxPerYear = 30,
            IsPaid = false,
            IsActive = true,
            IsDeleted = false
        };
        var defaultLt = await _dbContext.LeaveTypes.FindAsync(1);
        if (defaultLt != null) defaultLt.IsActive = false;

        await _dbContext.LeaveTypes.AddAsync(unpaidType);
        await _dbContext.SaveChangesAsync();

        var emp = await SeedEmployee(204);

        // Last attendance: Monday 1 June 2026
        await _dbContext.Attendances.AddAsync(new Attendance
        {
            EmployeeId = emp.Id,
            Date = new DateTime(2026, 6, 1),
            StatusId = AttendanceStatus.Present,
            ClockIn = new DateTime(2026, 6, 1, 9, 0, 0),
            ClockOut = new DateTime(2026, 6, 1, 18, 0, 0),
            TotalHours = TimeSpan.FromHours(9),
            Notes = "Clock In",
            IsActive = true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        // Employee absent Tue 2, Wed 3, Thu 4 June (3 days) and clocks in on Friday 5 June
        var clockInResult = await _attendanceService.CreateAttendanceAsync(new AttendanceDto
        {
            EmployeeId = emp.Id,
            Date = new DateTime(2026, 6, 5),
            ClockInTime = TimeSpan.FromHours(9),
            StatusId = AttendanceStatus.Present,
            Notes = "Present"
        });

        Assert.That(clockInResult.IsSuccess, Is.True);

        // Assert:
        // All 3 days deducted from Unpaid Leave
        var unpaidRequests = await _dbContext.LeaveRequests
            .Where(l => l.EmployeeId == emp.Id && !l.IsDeleted && l.LeaveTypeId == unpaidType.Id)
            .ToListAsync();
        Assert.That(unpaidRequests.Sum(l => l.LeaveDays), Is.EqualTo(3));
    }
}
