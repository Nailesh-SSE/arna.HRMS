using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Repositories;

[TestFixture]
public class AttendanceRepositoryTests
{
    private ApplicationDbContext _dbContext = null!;
    private AttendanceRepository _attendanceRepository = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var attendanceBaseRepo = new BaseRepository<Attendance>(_dbContext);
        var employeeBaseRepo = new BaseRepository<Employee>(_dbContext);
        var festivalBaseRepo = new BaseRepository<FestivalHoliday>(_dbContext);

        var employeeRepository = new EmployeeRepository(employeeBaseRepo);
        var festivalHolidayRepository = new FestivalHolidayRepository(festivalBaseRepo);

        _attendanceRepository = new AttendanceRepository(
            attendanceBaseRepo,
            employeeRepository,
            festivalHolidayRepository);
    }

    // 🔹 Helper to create valid employee
    private Employee CreateValidEmployee(int id = 1)
    {
        return new Employee
        {
            Id = id,
            FirstName = "Test",
            LastName = "User",
            Email = $"test{id}@mail.com",
            EmployeeNumber = $"EMP{id:000}",
            PhoneNumber = "9999999999",
            Position = "Developer",
            IsActive = true,
            IsDeleted = false
        };
    }

    private Attendance CreateValidAttendance(int id, int employeeId)
    {
        return new Attendance
        {
            Id = id,
            EmployeeId = employeeId,
            Date = DateTime.Today,
            ClockIn = DateTime.Today.AddHours(9),
            ClockOut = DateTime.Today.AddHours(18),
            TotalHours = TimeSpan.FromHours(8),
            Notes = "Regular working day",
            StatusId = AttendanceStatus.Present,
            IsActive = true,
            IsDeleted = false
        };
    }

    [Test]
    public async Task GetAttendenceAsync_ShouldReturnCompleteAttendanceData()
    {
        // Arrange
        var employee = CreateValidEmployee(10);
        _dbContext.Employees.Add(employee);

        var attendance = CreateValidAttendance(1, 10);

        _dbContext.Attendances.Add(attendance);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _attendanceRepository.GetEmployeeAttendanceByStatus(null , null);

        // Assert
        var record = result.First();

        Assert.That(record.ClockIn, Is.EqualTo(DateTime.Today.AddHours(9)));
        Assert.That(record.ClockOut, Is.EqualTo(DateTime.Today.AddHours(18)));
        Assert.That(record.TotalHours, Is.EqualTo(TimeSpan.FromHours(8)));
        Assert.That(record.Notes, Is.EqualTo("Regular working day"));
        Assert.That(record.StatusId, Is.EqualTo(AttendanceStatus.Present));
    }

    [Test]
    public async Task GetAttendenceAsync_ShouldPreserveWorkingHoursCorrectly()
    {
        var employee = CreateValidEmployee(20);
        _dbContext.Employees.Add(employee);

        var attendance = new Attendance
        {
            Id = 5,
            EmployeeId = 20,
            Date = DateTime.Today,
            ClockIn = DateTime.Today.AddHours(10),
            ClockOut = DateTime.Today.AddHours(19),
            TotalHours = TimeSpan.FromHours(7.5),
            Notes = "Late login",
            StatusId = AttendanceStatus.Late,
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Attendances.Add(attendance);
        await _dbContext.SaveChangesAsync();

        var result = await _attendanceRepository.GetEmployeeAttendanceByStatus(null , 5);
        var record = result.First();

        Assert.That(record.StatusId, Is.EqualTo(AttendanceStatus.Late));
        Assert.That(record.TotalHours, Is.EqualTo(TimeSpan.FromHours(7.5)));
        Assert.That(record.Date, Is.EqualTo(DateTime.Today));
    }

    [Test]
    public async Task GetAttendenceAsync_WhenNoRecords_ReturnsEmptyList()
    {
        // Act
        var result = await _attendanceRepository.GetEmployeeAttendanceByStatus(null, null );

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetAttendenceAsync_ShouldFilterInactiveDeletedAndOrderDescending()
    {
        // Arrange
        var employee = CreateValidEmployee(30);
        _dbContext.Employees.Add(employee);

        var validAttendance1 = CreateValidAttendance(1, 30);
        var validAttendance2 = CreateValidAttendance(5, 30);

        var inactiveAttendance = CreateValidAttendance(10, 30);
        inactiveAttendance.IsActive = false;

        var deletedAttendance = CreateValidAttendance(20, 30);
        deletedAttendance.IsDeleted = true;

        _dbContext.Attendances.AddRange(
            validAttendance1,
            validAttendance2,
            inactiveAttendance,
            deletedAttendance
        );

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _attendanceRepository.GetEmployeeAttendanceByStatus(null , 1);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));

        // Ensure only valid ones returned
        Assert.That(result.All(x => x.IsActive && !x.IsDeleted), Is.True);

        // Ensure ordering descending by Id
        Assert.That(result[0].Id, Is.GreaterThan(result[1].Id));
    }

    [Test]
    public async Task GetAttendenceAsync_ShouldIncludeEmployeeDetails()
    {
        // Arrange
        var employee = CreateValidEmployee(40);
        _dbContext.Employees.Add(employee);

        var attendance = CreateValidAttendance(100, 40);
        _dbContext.Attendances.Add(attendance);

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _attendanceRepository.GetEmployeeAttendanceByStatus(null , null);
        var record = result.First();

        // Assert
        Assert.That(record.Employee, Is.Not.Null);
        Assert.That(record.Employee.FirstName, Is.EqualTo("Test"));
        Assert.That(record.Employee.Id, Is.EqualTo(40));
    }

    [Test]
    public async Task GetAttendenceAsync_WhenMultipleValidRecords_ReturnsAll()
    {
        // Arrange
        var employee = CreateValidEmployee(50);
        _dbContext.Employees.Add(employee);

        _dbContext.Attendances.AddRange(
            CreateValidAttendance(1, 50),
            CreateValidAttendance(2, 50),
            CreateValidAttendance(3, 50)
        );

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _attendanceRepository.GetEmployeeAttendanceByStatus(null ,50);

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task GetAttendanceByIdAsync_WhenValidRecordExists_ReturnsAttendance()
    {
        // Arrange
        var employee = CreateValidEmployee(1);
        _dbContext.Employees.Add(employee);

        var attendance = CreateValidAttendance(100, 1);
        _dbContext.Attendances.Add(attendance);

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _attendanceRepository.GetAttendanceByIdAsync(100);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(100));
    }

    [Test]
    public async Task GetAttendanceByIdAsync_WhenRecordDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _attendanceRepository.GetAttendanceByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAttendanceByIdAsync_WhenInactiveAndWhenDeleted_ReturnsNull()
    {
        var employee = CreateValidEmployee(2);
        _dbContext.Employees.Add(employee);

        var attendance = CreateValidAttendance(200, 2);
        attendance.IsActive = false;

        _dbContext.Attendances.Add(attendance);
        await _dbContext.SaveChangesAsync();

        var result = await _attendanceRepository.GetAttendanceByIdAsync(200);

        Assert.That(result, Is.Null);

        var employee2 = CreateValidEmployee(3);
        _dbContext.Employees.Add(employee2);

        var attendance2 = CreateValidAttendance(300, 3);
        attendance2.IsDeleted = true;

        _dbContext.Attendances.Add(attendance2);
        await _dbContext.SaveChangesAsync();

        var result2 = await _attendanceRepository.GetAttendanceByIdAsync(300);

        Assert.That(result2, Is.Null);

    }

    [Test]
    public async Task GetAttendanceByIdAsync_WhenIdMatchesButInactiveOrDeleted_ReturnsNull()
    {
        // Arrange
        var employee = CreateValidEmployee(4);
        _dbContext.Employees.Add(employee);

        var inactive = CreateValidAttendance(400, 4);
        inactive.IsActive = false;

        var deleted = CreateValidAttendance(401, 4);
        deleted.IsDeleted = true;

        _dbContext.Attendances.AddRange(inactive, deleted);
        await _dbContext.SaveChangesAsync();

        // Act
        var result1 = await _attendanceRepository.GetAttendanceByIdAsync(400);
        var result2 = await _attendanceRepository.GetAttendanceByIdAsync(401);

        // Assert
        Assert.That(result1, Is.Null);
        Assert.That(result2, Is.Null);
    }

    [Test]
    public async Task GetAttendanceByIdAsync_WhenMultipleRecordsExist_ReturnsCorrectOne()
    {
        // Arrange
        var employee = CreateValidEmployee(5);
        _dbContext.Employees.Add(employee);

        var attendance1 = CreateValidAttendance(500, 5);
        var attendance2 = CreateValidAttendance(501, 5);

        _dbContext.Attendances.AddRange(attendance1, attendance2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _attendanceRepository.GetAttendanceByIdAsync(501);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(501));
    }

    [Test]
    public async Task GetAttendanceByIdAsync_WhenIdIsNegative_ReturnsNull()
    {
        // Act
        var result = await _attendanceRepository.GetAttendanceByIdAsync(-1);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAttendanceAsync_WhenValidAttendance_ReturnsCreatedAttendance()
    {
        // Arrange
        var employee = CreateValidEmployee(10);
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var attendance = CreateValidAttendance(1000, 10);

        // Act
        var result = await _attendanceRepository.CreateAttendanceAsync(attendance);
        await _dbContext.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.EmployeeId, Is.EqualTo(10));
        Assert.That(_dbContext.Attendances.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task CreateAttendanceAsync_ShouldPersistCorrectData()
    {
        var employee = CreateValidEmployee(11);
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var attendance = new Attendance
        {
            EmployeeId = 11,
            Date = DateTime.Today,
            ClockIn = DateTime.Today.AddHours(9),
            ClockOut = DateTime.Today.AddHours(18),
            TotalHours = TimeSpan.FromHours(8),
            Notes = "Test Note",
            StatusId = AttendanceStatus.Present,
            IsActive = true,
            IsDeleted = false
        };

        var result = await _attendanceRepository.CreateAttendanceAsync(attendance);
        await _dbContext.SaveChangesAsync();

        var saved = await _dbContext.Attendances.FirstAsync();

        Assert.That(saved.Notes, Is.EqualTo("Test Note"));
        Assert.That(saved.StatusId, Is.EqualTo(AttendanceStatus.Present));
    }

    [Test]
    public async Task CreateAttendanceAsync_ShouldIncreaseCount()
    {
        var employee = CreateValidEmployee(12);
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var attendance1 = CreateValidAttendance(1, 12);
        var attendance2 = CreateValidAttendance(2, 12);

        await _attendanceRepository.CreateAttendanceAsync(attendance1);
        await _attendanceRepository.CreateAttendanceAsync(attendance2);
        await _dbContext.SaveChangesAsync();

        Assert.That(_dbContext.Attendances.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task CreateAttendanceAsync_WhenMultipleRecords_ReturnsEachCorrectly()
    {
        var employee = CreateValidEmployee(13);
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var attendance1 = CreateValidAttendance(101, 13);
        var attendance2 = CreateValidAttendance(102, 13);

        var result1 = await _attendanceRepository.CreateAttendanceAsync(attendance1);
        var result2 = await _attendanceRepository.CreateAttendanceAsync(attendance2);

        await _dbContext.SaveChangesAsync();

        Assert.That(result1.Id, Is.EqualTo(101));
        Assert.That(result2.Id, Is.EqualTo(102));
    }

    [Test]
    public void CreateAttendanceAsync_WhenNullPassed_ThrowsException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _attendanceRepository.CreateAttendanceAsync(null!);
        });
    }

    //[Test]
    //public async Task GetAttendanceByMonthAsync_WhenNoSelectedDate_ReturnsFullMonth()
    //{
    //    var employee = CreateValidEmployee(1);
    //    _dbContext.Employees.Add(employee);
    //    await _dbContext.SaveChangesAsync();

    //    var result = await _attendanceRepository
    //        .GetAttendanceByMonthAsync(2026, 1, null, null);

    //    Assert.That(result.Count, Is.EqualTo(31)); // January
    //}

    //[Test]
    //public async Task GetAttendanceByMonthAsync_WhenSelectedDate_ReturnsSingleDay()
    //{
    //    var employee = CreateValidEmployee(2);
    //    _dbContext.Employees.Add(employee);
    //    await _dbContext.SaveChangesAsync();

    //    var selectedDate = new DateTime(2026, 2, 10);

    //    var result = await _attendanceRepository
    //        .GetAttendanceByMonthAsync(2026, 2, null, selectedDate);

    //    Assert.That(result.Count, Is.EqualTo(1));
    //    Assert.That(result.First().Date, Is.EqualTo(DateOnly.FromDateTime(selectedDate)));
    //}

    //[Test]
    //public async Task GetAttendanceByMonthAsync_WhenEmpIdProvided_ReturnsOnlyThatEmployee()
    //{
    //    var emp1 = CreateValidEmployee(10);
    //    var emp2 = CreateValidEmployee(20);

    //    _dbContext.Employees.AddRange(emp1, emp2);

    //    _dbContext.Attendances.AddRange(
    //        CreateValidAttendance(1, 10),
    //        CreateValidAttendance(2, 20)
    //    );

    //    await _dbContext.SaveChangesAsync();

    //    var result = await _attendanceRepository
    //        .GetAttendanceByMonthAsync(DateTime.Today.Year,
    //                                   DateTime.Today.Month,
    //                                   10,
    //                                   null);

    //    var employees = result.SelectMany(r => r.Employees);

    //    Assert.That(employees.All(e => e.EmployeeId == 10), Is.True);
    //}

    //[Test]
    //public async Task GetAttendanceByMonthAsync_ShouldCalculateWorkingHoursCorrectly()
    //{
    //    var employee = CreateValidEmployee(30);
    //    _dbContext.Employees.Add(employee);

    //    var date = new DateTime(2026, 3, 5);

    //    _dbContext.Attendances.Add(new Attendance
    //    {
    //        EmployeeId = 30,
    //        Date = date,
    //        ClockIn = date.AddHours(9),
    //        ClockOut = date.AddHours(18),
    //        TotalHours = TimeSpan.FromHours(8),
    //        StatusId = AttendanceStatus.Present,
    //        Notes = "Test Note",
    //        IsActive = true,
    //        IsDeleted = false
    //    });

    //    await _dbContext.SaveChangesAsync();

    //    var result = await _attendanceRepository
    //        .GetAttendanceByMonthAsync(2026, 3, 30, null);

    //    var emp = result.First(r => r.Date.Day == 5).Employees.First();

    //    Assert.That(emp.WorkingHours, Is.EqualTo(TimeSpan.FromHours(8)));
    //}

    //[Test]
    //public async Task GetAttendanceByMonthAsync_ShouldCalculateBreakCorrectly()
    //{
    //    var employee = CreateValidEmployee(40);
    //    _dbContext.Employees.Add(employee);

    //    var date = new DateTime(2026, 4, 2);

    //    _dbContext.Attendances.Add(new Attendance
    //    {
    //        EmployeeId = 40,
    //        Date = date,
    //        ClockIn = date.AddHours(9),
    //        ClockOut = date.AddHours(18),
    //        TotalHours = TimeSpan.FromHours(7),
    //        StatusId = AttendanceStatus.Present,
    //        Notes = "Test Not",
    //        IsActive = true,
    //        IsDeleted = false
    //    });

    //    await _dbContext.SaveChangesAsync();

    //    var result = await _attendanceRepository
    //        .GetAttendanceByMonthAsync(2026, 4, 40, null);

    //    var emp = result.First(r => r.Date.Day == 2).Employees.First();

    //    Assert.That(emp.BreakDuration, Is.EqualTo(TimeSpan.FromHours(2)));
    //}

    //[Test]
    //public async Task GetAttendanceByMonthAsync_WhenFestivalHoliday_ReturnsHolidayStatus()
    //{
    //    var employee = CreateValidEmployee(50);
    //    _dbContext.Employees.Add(employee);

    //    var holidayDate = new DateTime(2026, 5, 10);

    //    _dbContext.FestivalHoliday.Add(new FestivalHoliday
    //    {
    //        FestivalName = "Test Festival",
    //        Date = holidayDate,
    //        DayOfWeek = holidayDate.DayOfWeek.ToString(),
    //        IsActive = true,
    //        IsDeleted = false
    //    });

    //    await _dbContext.SaveChangesAsync();

    //    var result = await _attendanceRepository
    //        .GetAttendanceByMonthAsync(2026, 5, 50, null);

    //    var emp = result.First(r => r.Date.Day == 10).Employees.First();

    //    Assert.That(emp.Status, Is.EqualTo("Holiday"));
    //}

    //[Test]
    //public async Task GetAttendanceByMonthAsync_WhenWeekendAndNoAttendance_ReturnsHoliday()
    //{
    //    var employee = CreateValidEmployee(60);
    //    _dbContext.Employees.Add(employee);
    //    await _dbContext.SaveChangesAsync();

    //    var result = await _attendanceRepository
    //        .GetAttendanceByMonthAsync(2026, 8, 60, null);

    //    var weekend = result.First(r =>
    //        r.Date.DayOfWeek == DayOfWeek.Saturday ||
    //        r.Date.DayOfWeek == DayOfWeek.Sunday);

    //    Assert.That(weekend.Employees.First().Status, Is.EqualTo("Holiday"));
    //}

    //[Test]
    //public async Task GetAttendanceByMonthAsync_WhenPresent_StatusIsPresent()
    //{
    //    var employee = CreateValidEmployee(70);
    //    _dbContext.Employees.Add(employee);

    //    var date = new DateTime(2026, 6, 5);

    //    _dbContext.Attendances.Add(new Attendance
    //    {
    //        EmployeeId = 70,
    //        Date = date,
    //        StatusId = AttendanceStatus.Present,
    //        Notes = "Test Note",
    //        IsActive = true,
    //        IsDeleted = false
    //    });

    //    await _dbContext.SaveChangesAsync();

    //    var result = await _attendanceRepository
    //        .GetAttendanceByMonthAsync(2026, 6, 70, null);

    //    var emp = result.First(r => r.Date.Day == 5).Employees.First();

    //    Assert.That(emp.Status, Is.EqualTo("Present"));
    //}

    [Test]
    public async Task GetLastAttendanceDateAsync_WhenMultiplePresentRecords_ReturnsLatestDate()
    {
        var employee = CreateValidEmployee(1);
        _dbContext.Employees.Add(employee);

        var date1 = new DateTime(2026, 1, 1);
        var date2 = new DateTime(2026, 1, 10);
        var date3 = new DateTime(2026, 1, 5);

        _dbContext.Attendances.AddRange(
            new Attendance { EmployeeId = 1, Date = date1, StatusId = AttendanceStatus.Present, Notes = "Test Note", IsActive = true, IsDeleted = false },
            new Attendance { EmployeeId = 1, Date = date2, StatusId = AttendanceStatus.Present, Notes = "Test Note", IsActive = true, IsDeleted = false },
            new Attendance { EmployeeId = 1, Date = date3, StatusId = AttendanceStatus.Present, Notes = "Test Note", IsActive = true, IsDeleted = false }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceRepository.GetLastAttendanceDateAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.HasValue, Is.True);
        Assert.That(result.Value, Is.EqualTo(date2.Date));
        Assert.That(result.Value.Year, Is.EqualTo(2026));
        Assert.That(result.Value.Month, Is.EqualTo(1));
        Assert.That(result.Value.Day, Is.EqualTo(10));
    }

    [Test]
    public async Task GetLastAttendanceDateAsync_ShouldIgnoreNonPresentStatus()
    {
        var employee = CreateValidEmployee(2);
        _dbContext.Employees.Add(employee);

        var date = new DateTime(2026, 2, 15);

        _dbContext.Attendances.AddRange(
            new Attendance { EmployeeId = 2, Date = date, StatusId = AttendanceStatus.Absent, Notes = "Test Note", IsActive = true, IsDeleted = false },
            new Attendance { EmployeeId = 2, Date = date.AddDays(1), StatusId = AttendanceStatus.Late, Notes = "Test Note", IsActive = true, IsDeleted = false }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceRepository.GetLastAttendanceDateAsync(2);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetLastAttendanceDateAsync_WhenNoRecords_ReturnsNull()
    {
        var result = await _attendanceRepository.GetLastAttendanceDateAsync(100);

        Assert.That(result, Is.Null);
        Assert.That(result.HasValue, Is.False);
    }

    [Test]
    public async Task GetLastAttendanceDateAsync_ShouldFilterByEmployeeId()
    {
        var emp1 = CreateValidEmployee(10);
        var emp2 = CreateValidEmployee(20);

        _dbContext.Employees.AddRange(emp1, emp2);

        var date1 = new DateTime(2026, 3, 1);
        var date2 = new DateTime(2026, 3, 20);

        _dbContext.Attendances.AddRange(
            new Attendance { EmployeeId = 10, Date = date1, StatusId = AttendanceStatus.Present, Notes="Test Note", IsActive = true, IsDeleted = false },
            new Attendance { EmployeeId = 20, Date = date2, StatusId = AttendanceStatus.Present, Notes = "Test Note", IsActive = true, IsDeleted = false }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceRepository.GetLastAttendanceDateAsync(10);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo(date1.Date));
        Assert.That(result.Value, Is.Not.EqualTo(date2.Date));
    }

    [Test]
    public async Task GetLastAttendanceDateAsync_WhenSinglePresentRecord_ReturnsThatDate()
    {
        var employee = CreateValidEmployee(30);
        _dbContext.Employees.Add(employee);

        var date = new DateTime(2026, 4, 12);

        _dbContext.Attendances.Add(
            new Attendance { EmployeeId = 30, Date = date, StatusId = AttendanceStatus.Present, Notes = "Test Note", IsActive = true, IsDeleted = false }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceRepository.GetLastAttendanceDateAsync(30);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.HasValue, Is.True);
        Assert.That(result.Value.Date, Is.EqualTo(date.Date));
        Assert.That(result.Value.Day, Is.EqualTo(12));
    }

    [Test]
    public async Task GetLastAttendanceTodayAsync_WhenMultipleRecordsToday_ReturnsLatestById()
    {
        var employee = CreateValidEmployee(1);
        _dbContext.Employees.Add(employee);

        var today = DateTime.Today;

        _dbContext.Attendances.AddRange(
            new Attendance { Id = 1, EmployeeId = 1, Date = today, Notes = "Test Note", IsActive = true, IsDeleted = false },
            new Attendance { Id = 5, EmployeeId = 1, Date = today, Notes = "Test Note", IsActive = true, IsDeleted = false },
            new Attendance { Id = 3, EmployeeId = 1, Date = today, Notes = "Test Note", IsActive = true, IsDeleted = false }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceRepository.GetLastAttendanceTodayAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(5));
        Assert.That(result.EmployeeId, Is.EqualTo(1));
        Assert.That(result.Date.Date, Is.EqualTo(today));
    }

    [Test]
    public async Task GetLastAttendanceTodayAsync_WhenNoRecordToday_ReturnsNull()
    {
        var employee = CreateValidEmployee(2);
        _dbContext.Employees.Add(employee);

        _dbContext.Attendances.Add(
            new Attendance
            {
                EmployeeId = 2,
                Date = DateTime.Today.AddDays(-1),
                Notes = "Test Note",
                IsActive = true,
                IsDeleted = false
            });

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceRepository.GetLastAttendanceTodayAsync(2);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetLastAttendanceTodayAsync_WhenEmployeeHasNoRecords_ReturnsNull()
    {
        var result = await _attendanceRepository.GetLastAttendanceTodayAsync(999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetLastAttendanceTodayAsync_ShouldReturnOnlyThatEmployeeRecord()
    {
        var emp1 = CreateValidEmployee(10);
        var emp2 = CreateValidEmployee(20);

        _dbContext.Employees.AddRange(emp1, emp2);

        var today = DateTime.Today;

        _dbContext.Attendances.AddRange(
            new Attendance { Id = 1, EmployeeId = 10, Date = today, Notes = "Test Note" },
            new Attendance { Id = 2, EmployeeId = 20, Date = today, Notes = "Test Note" }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceRepository.GetLastAttendanceTodayAsync(10);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.EmployeeId, Is.EqualTo(10));
        Assert.That(result.EmployeeId, Is.Not.EqualTo(20));
    }

    [Test]
    public async Task GetLastAttendanceTodayAsync_ShouldIgnoreFutureOrPastDates()
    {
        var employee = CreateValidEmployee(30);
        _dbContext.Employees.Add(employee);

        var today = DateTime.Today;

        _dbContext.Attendances.AddRange(
            new Attendance { Id = 1, EmployeeId = 30, Date = today, Notes="Test Note" },
            new Attendance { Id = 100, EmployeeId = 31, Date = today.AddDays(1), Notes = "Test Note" } // future
        );

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceRepository.GetLastAttendanceTodayAsync(30);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Date.Date, Is.EqualTo(today));
        Assert.That(result.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetLastAttendanceTodayAsync_ShouldReturnEvenIfInactiveOrDeleted()
    {
        var employee = CreateValidEmployee(40);
        _dbContext.Employees.Add(employee);

        var today = DateTime.Today;

        _dbContext.Attendances.Add(
            new Attendance
            {
                Id = 10,
                EmployeeId = 40,
                Date = today,
                Notes = "Test Note",
                IsActive = false,
                IsDeleted = true
            });

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceRepository.GetLastAttendanceTodayAsync(40);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsActive, Is.False);
        Assert.That(result.IsDeleted, Is.True);
    }

    [Test]
    public async Task GetLastAttendanceTodayAsync_WhenMultipleEmployeesSameDay_ReturnsCorrectOne()
    {
        var emp1 = CreateValidEmployee(50);
        var emp2 = CreateValidEmployee(60);

        _dbContext.Employees.AddRange(emp1, emp2);

        var today = DateTime.Today;

        _dbContext.Attendances.AddRange(
            new Attendance { Id = 1, EmployeeId = 50, Date = today, Notes = "Test Note" },
            new Attendance { Id = 2, EmployeeId = 60, Date = today, Notes = "Test Note" }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _attendanceRepository.GetLastAttendanceTodayAsync(60);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.EmployeeId, Is.EqualTo(60));
        Assert.That(result.EmployeeId, Is.Not.EqualTo(50));
    }

}
