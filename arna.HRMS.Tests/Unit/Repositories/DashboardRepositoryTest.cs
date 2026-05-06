using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Repositories;

public class DashboardRepositoryTest
{
    private ApplicationDbContext _dbcontext = null!;
    private DashboardRepository _dashboardRepository;
    private EmployeeRepository _employeeRepository = null!;
    private AttendanceRepository _attendanceRepository = null!;

    [SetUp]
    public void setup()
    {
        var option = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbcontext = new ApplicationDbContext(option);

        var employeeRepository = new BaseRepository<Employee>(_dbcontext);
        var attendanceRepository = new BaseRepository<Attendance>(_dbcontext);

        _dashboardRepository = new DashboardRepository(employeeRepository, attendanceRepository);
    }

    [Test]
    public async Task AdminDashboardAsync_ReturnsOnlyActiveAndNonDeletedEmployees()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "IT", Code = "007", Description = "bond james bond" };
        _dbcontext.Departments.Add(department);

        var employees = GetSampleEmployees();
        _dbcontext.Employees.AddRange(employees);

        await _dbcontext.SaveChangesAsync();

        // Act
        var result = await _dashboardRepository.AdminDashboardAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(x => x.IsActive), Is.True);
        Assert.That(result.All(x => !x.IsDeleted), Is.True);
    }

    [Test]
    public async Task AdminDashboardAsync_ReturnNoValue()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "IT", Code = "007", Description = "bond james bond" };
        _dbcontext.Departments.Add(department);

        await _dbcontext.SaveChangesAsync();

        // Act
        var result = await _dashboardRepository.AdminDashboardAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetTodayPresentEmployeesAsync_ReturnsOnlyEmployeesWithClockInOrOut()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "IT", Code = "007", Description = "bond james bond" };
        _dbcontext.Departments.Add(department);

        var employees = GetSampleEmployees();
        _dbcontext.Employees.AddRange(employees);

        var attendances = GetSampleAttendances();
        _dbcontext.Attendances.AddRange(attendances);

        await _dbcontext.SaveChangesAsync();

        // Act
        var result = await _dashboardRepository.GetTodayPresentEmployeesAsync();

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.Any(x => x.EmployeeId == 3), Is.False);
    }

    [Test]
    public async Task GetTodayPresentEmployeesAsync_ShouldCalculateAllFieldsCorrectly()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "IT", Code = "007", Description = "bond james bond" };
        _dbcontext.Departments.Add(department);

        var employees = GetSampleEmployees();
        _dbcontext.Employees.AddRange(employees);

        var attendances = GetSampleAttendances();
        _dbcontext.Attendances.AddRange(attendances);

        await _dbcontext.SaveChangesAsync();

        // Act
        var result = await _dashboardRepository.GetTodayPresentEmployeesAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));

        var emp1 = result.First(x => x.EmployeeId == 1);

        Assert.That(emp1.WorkingHours, Is.EqualTo(TimeSpan.FromHours(8)));
        Assert.That(emp1.TotalHours, Is.EqualTo(TimeSpan.FromHours(9)));
        Assert.That(emp1.Breaks.Count, Is.EqualTo(1));

        Assert.That(result.Any(x => x.EmployeeId == 3), Is.False);
    }

    [Test]
    public async Task EmployeeDashBoardAsync_ReturnsLeaveEmployees()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "IT", Code = "007", Description = "bond james bond" };
        _dbcontext.Departments.Add(department);

        var employee1 = new Employee
        {
            Id = 2,
            EmployeeNumber = "EMP002",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            OfficeEmail = "john@test.com",
            PhoneNumber = "9999999999",
            DateOfBirth = DateTime.Today.AddYears(-24),
            HireDate = DateTime.Today.AddYears(-1),
            Position = "Developer",
            DepartmentId = 1,
            Salary = 50000,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now,

        };
        _dbcontext.Employees.Add(employee1);

        await _dbcontext.SaveChangesAsync();

        var result = await _dashboardRepository.EmployeeDashboardAsync(Status.Approved, 2);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(2));

    }

    [Test]
    public async Task EmployeeDashboardAsync_ReturnsNull_WhenEmployeeNotFound()
    {
        // Act
        var result = await _dashboardRepository.EmployeeDashboardAsync(null, 999);

        // Assert
        Assert.That(result, Is.Null);
    }
    [Test]
    public async Task EmployeeDashboardAsync_FiltersLeaveRequests_ByStatus()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "IT", Code = "007", Description = "bond james bond" };
        _dbcontext.Departments.Add(department);

        var employee2 = new Employee
        {
            Id = 3,
            EmployeeNumber = "EMP003",
            FirstName = "Bob",
            LastName = "fisher",
            Email = "Bob@test.com",
            OfficeEmail = "Bob@test.com",
            PhoneNumber = "1999999999",
            DateOfBirth = DateTime.Today.AddYears(-24),
            HireDate = DateTime.Today.AddYears(-1),
            Position = "Developer",
            DepartmentId = 1,
            Salary = 50000,
            ManagerId = 1,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now,
            LeaveRequests =new List<LeaveRequest>
            { 
                new LeaveRequest
                {
                    Id = 1,
                    EmployeeId = 3,
                    LeaveTypeId = 2,
                    StartDate = DateTime.Today.AddDays(-2),
                    EndDate = DateTime.Today.AddDays(-1),
                    LeaveDays = 2,
                    Reason = "Fever",
                    StatusId = Status.Approved,
                    ApprovedBy = null,
                    ApprovedDate = null,
                    ApprovalNotes = null,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = DateTime.Now
                },
                new LeaveRequest
                {
                    Id = 2,
                    EmployeeId = 3,
                    LeaveTypeId = 1,
                    StartDate = DateTime.Today.AddDays(-14),
                    EndDate = DateTime.Today.AddDays(-13),
                    LeaveDays = 2,
                    Reason = "Fever",
                    StatusId = Status.Pending,
                    ApprovedBy = null,
                    ApprovedDate = null,
                    ApprovalNotes = null,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = DateTime.Now
                },
            }
        };

        _dbcontext.Employees.Add(employee2);
        _dbcontext.LeaveRequests.AddRange(employee2.LeaveRequests);

        _dbcontext.LeaveTypes.AddRange(
    new LeaveType { Id = 1, LeaveNameId = LeaveName.paidLeave ,MaxPerYear = 10 ,IsPaid =true },
    new LeaveType { Id = 2, LeaveNameId = LeaveName.SickLeave ,MaxPerYear =6 ,IsPaid = true}
);

        await _dbcontext.SaveChangesAsync();

        // Act
        var result = await _dashboardRepository.EmployeeDashboardAsync(Status.Pending, 3);

        // Assert
        Assert.That(result!.LeaveRequests.Count, Is.EqualTo(1));
        Assert.That(result.LeaveRequests.First().StatusId, Is.EqualTo(Status.Pending));
    }

    [Test]
    public async Task GetTodayLeaveEmployeesAsync_ReturnsLeaveEmployees()
    {
        // Arrange
        var today = DateTime.Now.Date;

        var employees = GetSampleEmployees();

        var attendance = GetSampleAttendances();

        _dbcontext.Employees.AddRange(employees);
        _dbcontext.Attendances.AddRange(attendance);

        await _dbcontext.SaveChangesAsync();

        // Act
        var result = await _dashboardRepository.GetTodayLeaveEmployeesAsync();

        // Assert
        Assert.That(result.Any(x => x.EmployeeId == 3), Is.True);
    }

    [Test]
    public async Task GetTodayLeaveEmployeeAsync_ExcludesPresentEmployees()
    {
        //Arrange
        var today = DateTime.Now.Date;

        var employees = GetSampleEmployees();

        var attendances = GetSampleAttendances();

        _dbcontext.Employees.AddRange(employees);
        _dbcontext.Attendances.AddRange(attendances);

        //Act
        var result = await _dashboardRepository.GetTodayLeaveEmployeesAsync();

        //Assert
        Assert.That(result.Any(x => x.EmployeeId == 1), Is.False);
    }

    public List<Employee> GetSampleEmployees()
    {
        var manager = new Employee
        {
            Id = 1,
            EmployeeNumber = "EMP001",
            FirstName = "Brook",
            LastName = "oak",
            Email = "Brook@test.com",
            OfficeEmail = "Brook@test.com",
            PhoneNumber = "2999999999",
            DateOfBirth = DateTime.Today.AddYears(-24),
            HireDate = DateTime.Today.AddYears(-1),
            Position = "Developer",
            DepartmentId = 1,
            Salary = 50000,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now,
        };
        var employee1 = new Employee
        {
            Id = 2,
            EmployeeNumber = "EMP002",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            OfficeEmail = "john@test.com",
            PhoneNumber = "9999999999",
            DateOfBirth = DateTime.Today.AddYears(-24),
            HireDate = DateTime.Today.AddYears(-1),
            Position = "Developer",
            DepartmentId = 1,
            Salary = 50000,
            IsActive = false,
            IsDeleted = false,
            CreatedOn = DateTime.Now,

        };
        var employee2 = new Employee
        {
            Id = 3,
            EmployeeNumber = "EMP003",
            FirstName = "Bob",
            LastName = "fisher",
            Email = "Bob@test.com",
            OfficeEmail = "Bob@test.com",
            PhoneNumber = "1999999999",
            DateOfBirth = DateTime.Today.AddYears(-24),
            HireDate = DateTime.Today.AddYears(-1),
            Position = "Developer",
            DepartmentId = 1,
            Salary = 50000,
            ManagerId = 1,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now,

        };
        var employee3 = new Employee
        {
            Id = 4,
            EmployeeNumber = "EMP004",
            FirstName = "Ash",
            LastName = "Ketcham",
            Email = "Ash@test.com",
            OfficeEmail = "Ash@test.com",
            PhoneNumber = "3999999999",
            DateOfBirth = DateTime.Today.AddYears(-10),
            HireDate = DateTime.Today.AddYears(-1),
            Position = "Developer",
            DepartmentId = 1,
            Salary = 50000,
            ManagerId = 1,
            IsActive = false,
            IsDeleted = true,
            CreatedOn = DateTime.Now,

        };
        return new List<Employee> { manager, employee1, employee2, employee3 };
    }
    public List<Attendance> GetSampleAttendances()
    {
        var today = DateTime.Now.Date;

        return new List<Attendance>
    {
        new Attendance
        {
            EmployeeId = 1,
            Date = today,
            ClockIn = today.AddHours(9),
            ClockOut = today.AddHours(13),
            TotalHours = TimeSpan.FromHours(4),
            StatusId = AttendanceStatus.Present,
            IsActive = true,
            IsDeleted = false,
            Notes = "test"
        },
        new Attendance
        {
            EmployeeId = 1,
            Date = today,
            ClockIn = today.AddHours(14),
            ClockOut = today.AddHours(18),
            TotalHours = TimeSpan.FromHours(4),
            StatusId = AttendanceStatus.Present,
            IsActive = true,
            IsDeleted = false,
            Notes = "test"
        },
        new Attendance
        {
            EmployeeId = 1,
            Date = today.AddDays(-1),
            ClockIn = today.AddDays(-1).AddHours(9),
            ClockOut = today.AddDays(-1).AddHours(17),
            TotalHours = TimeSpan.FromHours(8),
            StatusId = AttendanceStatus.Present,
            IsActive = true,
            IsDeleted = false,
            Notes = "test"
        },
        new Attendance
        {
            EmployeeId = 2,
            Date = today,
            ClockIn = today.AddHours(11),
            ClockOut = today.AddHours(18),
            TotalHours = TimeSpan.FromHours(7),
            StatusId = AttendanceStatus.Present,
            IsActive = true,
            IsDeleted = false,
            Notes = "test"
        },
        new Attendance
        {
            EmployeeId = 2,
            Date = today,
            ClockIn = today.AddHours(9),
            ClockOut = today.AddHours(13),
            TotalHours = TimeSpan.FromHours(4),
            StatusId = AttendanceStatus.Present,
            IsActive = true,
            IsDeleted = false,
            Notes = "test"
        },
        new Attendance
        {
            EmployeeId = 3,
            Date = today,
            ClockIn = null,
            ClockOut = null,
            TotalHours = null,
            StatusId = AttendanceStatus.Absent,
            IsActive = true,
            IsDeleted = false,
            Notes = "test"
        },
        new Attendance
        {
            EmployeeId = 3,
            Date = today,
            ClockIn = null,
            ClockOut = null,
            TotalHours = null,
            StatusId = AttendanceStatus.Leave,
            IsActive = true,
            IsDeleted = false,
            Notes = "Sick Leave"
        }
    };
    }
    public List<LeaveRequest> GetSampleLeaveRequests()
    {
        var today = DateTime.Now.Date;

        return new List<LeaveRequest>
    {
        new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            StartDate = today.AddDays(-2),
            EndDate = today.AddDays(-1),
            LeaveDays = 2,
            Reason = "Fever",
            StatusId = Status.Pending,
            ApprovedBy = null,
            ApprovedDate = null,
            ApprovalNotes = null,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        },

        new LeaveRequest
        {
            Id = 2,
            EmployeeId = 1,
            LeaveTypeId = 1,
            StartDate = today.AddDays(1),
            EndDate = today.AddDays(2),
            LeaveDays = 2,
            Reason = "Family Function",
            StatusId = Status.Approved,
            ApprovedBy = 3,
            ApprovedDate = DateTime.Now,
            ApprovalNotes = "Approved by manager",
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        },

        new LeaveRequest
        {
            Id = 3,
            EmployeeId = 1,
            LeaveTypeId = 2,
            StartDate = today.AddDays(3),
            EndDate = today.AddDays(3),
            LeaveDays = 1,
            Reason = "Personal Work",
            StatusId = Status.Rejected,
            ApprovedBy = 3,
            ApprovedDate = DateTime.Now,
            ApprovalNotes = "Project deadline",
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        },

        new LeaveRequest
        {
            Id = 4,
            EmployeeId = 2,
            LeaveTypeId = 1,
            StartDate = today.AddDays(4),
            EndDate = today.AddDays(5),
            LeaveDays = 2,
            Reason = "Vacation",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        }
    };
    }
}
