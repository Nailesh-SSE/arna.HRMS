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
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Services;

[TestFixture]
public class LeaveServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private LeaveService _leaveService = null!;
    private AttendanceRepository _attendanceRepository = null!;
    private IMapper _mapper = null!;

    private Mock<IFestivalHolidayService> _festivalHolidayServiceMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var leaveRequestBaseRepo = new BaseRepository<LeaveRequest>(_dbContext);
        var leaveMasterBaseRepo = new BaseRepository<LeaveMaster>(_dbContext);
        var employeeLeaveBalanceBaseRepo = new BaseRepository<EmployeeLeaveBalance>(_dbContext);
        var attendanceBaseRepo = new BaseRepository<Attendance>(_dbContext);
        var festivalBaseRepo = new BaseRepository<FestivalHoliday>(_dbContext);

        var leaveRepository = new LeaveRepository(
            leaveMasterBaseRepo,
            leaveRequestBaseRepo,
            employeeLeaveBalanceBaseRepo);

        var festivalHolidayRepository = new FestivalHolidayRepository(festivalBaseRepo);

        _attendanceRepository = new AttendanceRepository(attendanceBaseRepo, festivalHolidayRepository);

        _festivalHolidayServiceMock = new Mock<IFestivalHolidayService>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AttendanceProfile>();
            cfg.AddProfile<LeaveProfile>();
        });

        _mapper = mapperConfig.CreateMapper();

        _leaveService = new LeaveService(
            leaveRepository,
            _mapper,
            _festivalHolidayServiceMock.Object,
            _attendanceRepository
        );
    }

    [Test]
    public async Task GetAllLeaveMaster()
    {
        _dbContext.AddRange(
            new LeaveMaster { LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true },
            new LeaveMaster { LeaveName = "Earned Leave", Description = "Earned Leave Description", MaxPerYear = 15, IsPaid = true },
            new LeaveMaster { LeaveName = "Unpaid Leave", Description = "Unpaid Leave Description", MaxPerYear = 5, IsPaid = false },
            new LeaveMaster { LeaveName = "Maternity Leave", Description = "Maternity Leave Description", MaxPerYear = 90, IsPaid = true },
            new LeaveMaster { LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = false }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.GetLeaveMasterAsync();

        Assert.That(result.Data!.Count, Is.EqualTo(6));
        Assert.That(result.Data[5].LeaveName, Is.EqualTo("Sick Leave"));
        Assert.That(result.Data[0].IsPaid, Is.False);
        Assert.That(result.Data[3].MaxPerYear, Is.EqualTo(15));
        Assert.That(result.Data[4].Description, Is.EqualTo("Casual Leave Description"));
        Assert.That(result.Data[2].Id, Is.EqualTo(4));
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task GetLeaveMasterById_whenFound()
    {
        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true },
            new LeaveMaster { Id = 3, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = false }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _leaveService.GetLeaveMasterByIdAsync(2);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.LeaveName, Is.EqualTo("Casual Leave"));
        Assert.That(result.Data.MaxPerYear, Is.EqualTo(8));
        Assert.That(result.Data.IsPaid, Is.True);
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task GetLeaveMasterById_whenNotFound()
    {
        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _leaveService.GetLeaveMasterByIdAsync(5);
        Assert.That(result.Data!.LeaveName, Is.Null);
        Assert.That(result.Data!.Description, Is.Null);
        Assert.That(result.Data!.MaxPerYear, Is.EqualTo(0));
        Assert.That(result.Data!.IsPaid, Is.EqualTo(true));
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task CreateLeaveMaster_WhenSuccess()
    {
        var newLeaveMaster = new LeaveMasterDto
        {
            LeaveName = "Paternity Leave",
            Description = "Paternity Leave Description",
            MaxPerYear = 15
        };
        var result = await _leaveService.CreateLeaveMasterAsync(newLeaveMaster);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.GreaterThan(0));
        Assert.That(result.Data.LeaveName, Is.EqualTo("Paternity Leave"));
        Assert.That(result.Data.MaxPerYear, Is.EqualTo(15));
        Assert.That(result.Data.IsPaid, Is.True);
        Assert.That(result.Data!.Description, Is.EqualTo("Paternity Leave Description"));
        Assert.That(result.IsSuccess, Is.True);
    }



    [Test]
    public async Task CreateLeaveMaster_WhenNameIsNullOrWhitespace()
    {
        var newLeaveMaster = new LeaveMasterDto
        {
            Description = "No Name Leave Description",
            MaxPerYear = 5
        };

        Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await _leaveService.CreateLeaveMasterAsync(newLeaveMaster);
        });

    }

    [Test]
    public async Task LeaveExistsAsync_WhenExists()
    {
        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _leaveService.LeaveExistsAsync("sick leave");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);
    }

    [Test]
    public async Task LeaveExistsAsync_WhenNotExists()
    {
        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _leaveService.LeaveExistsAsync("Maternity Leave");
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.False);
    }

    [Test]
    public async Task LeaveExistsAsync_WhenNameIsNullOrWhitespace()
    {
        var result = await _leaveService.LeaveExistsAsync("   ");
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.False);
    }

    [Test]
    public async Task DeleteLeaveMaster_WhenFound()
    {
        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        Assert.That((await _dbContext.LeaveMasters.FindAsync(1))!.IsActive, Is.True);
        Assert.That((await _dbContext.LeaveMasters.FindAsync(1))!.IsDeleted, Is.False);
        Assert.That((await _dbContext.LeaveMasters.FindAsync(2))!.IsActive, Is.True);
        Assert.That((await _dbContext.LeaveMasters.FindAsync(2))!.IsDeleted, Is.False);

        var result = await _leaveService.DeleteLeaveMasterAsync(1);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That((await _dbContext.LeaveMasters.FindAsync(2))!.IsActive, Is.True);
        Assert.That((await _dbContext.LeaveMasters.FindAsync(2))!.IsDeleted, Is.False);

        var deletedLeaveMaster = await _dbContext.LeaveMasters.FindAsync(1);
        Assert.That(deletedLeaveMaster!.IsActive, Is.False);
        Assert.That(deletedLeaveMaster.IsDeleted, Is.True);
    }

    [Test]
    public async Task DeleteLeaveMaster_WhenNotFound()
    {
        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.DeleteLeaveMasterAsync(5);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.False);
    }

    [Test]
    public async Task UpdateLeaveMaster_whenFound()
    {
        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        Assert.That((await _dbContext.LeaveMasters.FindAsync(2))!.LeaveName, Is.EqualTo("Casual Leave"));
        Assert.That((await _dbContext.LeaveMasters.FindAsync(2))!.Description, Is.EqualTo("Casual Leave Description"));
        Assert.That((await _dbContext.LeaveMasters.FindAsync(2))!.MaxPerYear, Is.EqualTo(8));
        Assert.That((await _dbContext.LeaveMasters.FindAsync(2))!.IsPaid, Is.True);

        _dbContext.ChangeTracker.Clear();

        var updatedLeaveMaster = new LeaveMasterDto
        {
            Id = 2,
            LeaveName = "Updated Casual Leave",
            Description = "Updated Description",
            MaxPerYear = 12,
            IsPaid = false
        };
        var result = await _leaveService.UpdateLeaveMasterAsync(updatedLeaveMaster);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(2));
        Assert.That(result.Data.LeaveName, Is.EqualTo("Updated Casual Leave"));
        Assert.That(result.Data.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Data.MaxPerYear, Is.EqualTo(12));
        Assert.That(result.Data.IsPaid, Is.False);
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task UpdateLeaveMaster_whenNotFound()
    {
        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var updatedLeaveMaster = new LeaveMasterDto
        {
            Id = 5,
            LeaveName = "Updated Casual Leave",
            Description = "Updated Description",
            MaxPerYear = 12,
            IsPaid = false
        };

        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await _leaveService.UpdateLeaveMasterAsync(updatedLeaveMaster);
        });
    }
    [Test]
    public async Task UpdateLeaveMaster_WhenNameIsNullOrWhitespace()
    {
        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var updatedLeaveMaster = new LeaveMasterDto
        {
            Id = 2,
            Description = "Updated Description",
            MaxPerYear = 12,
            IsPaid = false
        };
        var result = await _leaveService.UpdateLeaveMasterAsync(updatedLeaveMaster);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to update Leave Master"));

    }

    [Test]
    public async Task GetAllLeaveRequest()
    {
        _dbContext.Employees.AddRange(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@a.com",
                PhoneNumber = "1111111111",
                Position = "Developer",
                Salary = 50000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-25)
            },
            new Employee
            {
                Id = 102,
                EmployeeNumber = "Emp002",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@b.com",
                PhoneNumber = "2222002222",
                Position = "Tester",
                Salary = 4000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-24)
            },
            new Employee
            {
                Id = 103,
                EmployeeNumber = "Emp003",
                FirstName = "Pratham",
                LastName = "Smith",
                Email = "pratham@b.com",
                PhoneNumber = "2222222222",
                Position = "Tester",
                Salary = 45000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-24)
            },
             new Employee
             {
                 Id = 104,
                 EmployeeNumber = "Emp004",
                 FirstName = "Ajay",
                 LastName = "Smith",
                 Email = "ajay@b.com",
                 PhoneNumber = "2444222222",
                 Position = "Developer",
                 Salary = 40000,
                 DepartmentId = 1,
                 HireDate = DateTime.Now,
                 DateOfBirth = DateTime.Now.AddYears(-24)
             }
        );

        await _dbContext.SaveChangesAsync();

        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.AddRange(
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", Status = Status.Pending },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7), Reason = "Vacation", Status = Status.Approved },
            new LeaveRequest { EmployeeId = 103, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(10), EndDate = DateTime.Now.AddDays(12), Reason = "Flu", Status = Status.Rejected },
            new LeaveRequest { EmployeeId = 104, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(15), EndDate = DateTime.Now.AddDays(18), Reason = "Family Function", Status = Status.Pending }
        );
        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.GetLeaveRequestAsync();
        Assert.That(result.Data!.Count, Is.EqualTo(4));
        Assert.That(result.Data[0].EmployeeId, Is.EqualTo(104));
        Assert.That(result.Data[2].Reason, Is.EqualTo("Vacation"));
        Assert.That(result.Data[3].LeaveTypeId, Is.EqualTo(1));
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data![0].Id, Is.EqualTo(4));
        Assert.That(result.Data![3].Id, Is.EqualTo(1));
        Assert.That(result.Data![2].Id, Is.EqualTo(2));
        Assert.That(result.Data![1].Id, Is.EqualTo(3));
        Assert.That(result.Data![0].Reason, Is.Not.Null);
        Assert.That(result.Data![1].Reason, Is.Not.Null);
        Assert.That(result.Data![2].Reason, Is.Not.Null);
        Assert.That(result.Data![3].Reason, Is.Not.Null);
        Assert.That(result.Data![0].Status, Is.EqualTo(Status.Pending));
        Assert.That(result.Data![1].Status, Is.EqualTo(Status.Rejected));
        Assert.That(result.Data![2].Status, Is.EqualTo(Status.Approved));
        Assert.That(result.Data![3].Status, Is.EqualTo(Status.Pending));
        Assert.That(result.Data![0].LeaveTypeId, Is.GreaterThan(0));
        Assert.That(result.Data![1].LeaveTypeId, Is.GreaterThan(0));
        Assert.That(result.Data![2].LeaveTypeId, Is.GreaterThan(0));
        Assert.That(result.Data![3].LeaveTypeId, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetLeaveRequestBystatuses()
    {
        _dbContext.Employees.AddRange(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@a.com",
                PhoneNumber = "1111111111",
                Position = "Developer",
                Salary = 50000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-25)
            },
            new Employee
            {
                Id = 102,
                EmployeeNumber = "Emp002",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@b.com",
                PhoneNumber = "2222002222",
                Position = "Tester",
                Salary = 4000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-24)
            },
            new Employee
            {
                Id = 103,
                EmployeeNumber = "Emp003",
                FirstName = "Pratham",
                LastName = "Smith",
                Email = "pratham@b.com",
                PhoneNumber = "2222222222",
                Position = "Tester",
                Salary = 45000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-24)
            },
             new Employee
             {
                 Id = 104,
                 EmployeeNumber = "Emp004",
                 FirstName = "Ajay",
                 LastName = "Smith",
                 Email = "ajay@b.com",
                 PhoneNumber = "2444222222",
                 Position = "Developer",
                 Salary = 40000,
                 DepartmentId = 1,
                 HireDate = DateTime.Now,
                 DateOfBirth = DateTime.Now.AddYears(-24)
             }
        );

        await _dbContext.SaveChangesAsync();

        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.AddRange(
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", Status = Status.Pending },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7), Reason = "Vacation", Status = Status.Approved },
            new LeaveRequest { EmployeeId = 103, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(10), EndDate = DateTime.Now.AddDays(12), Reason = "Flu", Status = Status.Rejected },
            new LeaveRequest { EmployeeId = 104, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(15), EndDate = DateTime.Now.AddDays(18), Reason = "Family Function", Status = Status.Pending }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.GetByStatusAsync(Status.Pending);

        Assert.That(result.Data!.Count, Is.EqualTo(2));
        Assert.That(result.Data[0].EmployeeId, Is.EqualTo(104));
        Assert.That(result.Data[1].EmployeeId, Is.EqualTo(101));
        Assert.That(result.Data[0].Status, Is.EqualTo(Status.Pending));
        Assert.That(result.Data[1].Status, Is.EqualTo(Status.Pending));
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data![0].StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(15).Date));
        Assert.That(result.Data![1].EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(3).Date));
        Assert.That(result.Data![0].Reason, Is.EqualTo("Family Function")); 
        Assert.That(result.Data![1].Reason, Is.EqualTo("Medical"));
        Assert.That(result.Data![0].LeaveTypeId, Is.EqualTo(2));
        Assert.That(result.Data![1].LeaveTypeId, Is.EqualTo(1));
    }

    [Test]
    public async Task GetLeaveRequestById_whenFound()
    {
        _dbContext.Employees.AddRange(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@a.com",
                PhoneNumber = "1111111111",
                Position = "Developer",
                Salary = 50000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-25)
            },
            new Employee
            {
                Id = 102,
                EmployeeNumber = "Emp002",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@b.com",
                PhoneNumber = "2222002222",
                Position = "Tester",
                Salary = 4000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-24)
            }
        );

        await _dbContext.SaveChangesAsync();

        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.AddRange(
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", Status = Status.Pending },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(7), EndDate = DateTime.Now.AddDays(5), Reason = "Vacation", Status = Status.Approved },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(15), EndDate = DateTime.Now.AddDays(12), Reason = "Flu", Status = Status.Rejected }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.GetLeaveRequestByIdAsync(2);

        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.EmployeeId, Is.EqualTo(102));
        Assert.That(result.Data.LeaveTypeId, Is.EqualTo(2));
        Assert.That(result.Data.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(7).Date));
        Assert.That(result.Data.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(5).Date));
        Assert.That(result.Data.Reason, Is.EqualTo("Vacation"));
        Assert.That(result.Data.Status, Is.EqualTo(Status.Approved));
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task GetLeaveRequestById_whenNotFound()
    {
        _dbContext.Employees.AddRange(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@a.com",
                PhoneNumber = "1111111111",
                Position = "Developer",
                Salary = 50000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-25)
            },
            new Employee
            {
                Id = 102,
                EmployeeNumber = "Emp002",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@b.com",
                PhoneNumber = "2222002222",
                Position = "Tester",
                Salary = 4000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-24)
            }

        );

        await _dbContext.SaveChangesAsync();

        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );

        await _dbContext.SaveChangesAsync();

        _dbContext.AddRange(
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", Status = Status.Pending },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7), Reason = "Vacation", Status = Status.Approved },
            new LeaveRequest { EmployeeId = 103, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(10), EndDate = DateTime.Now.AddDays(12), Reason = "Flu", Status = Status.Rejected },
            new LeaveRequest { EmployeeId = 104, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(15), EndDate = DateTime.Now.AddDays(18), Reason = "Family Function", Status = Status.Pending }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.GetLeaveRequestByIdAsync(5);
        Assert.That(result.Data!.EmployeeId, Is.EqualTo(0));
        Assert.That(result.Data!.LeaveTypeId, Is.EqualTo(0));
        Assert.That(result.Data!.StartDate, Is.EqualTo(DateTime.MinValue));
        Assert.That(result.Data!.EndDate, Is.EqualTo(DateTime.MinValue));
        Assert.That(result.Data!.Reason, Is.Null);
        Assert.That(result.Data!.Status, Is.EqualTo(Status.Pending));
        Assert.That(result.IsSuccess, Is.True);

    }

    [Test]
    public async Task CreateLeaveRequest_WhenSuccess()
    {
        // -------------------------------
        // Arrange
        // -------------------------------

        _festivalHolidayServiceMock
            .Setup(x => x.GetFestivalHolidayAsync())
            .ReturnsAsync(
                ServiceResult<List<FestivalHolidayDto>>.Success(new List<FestivalHolidayDto>())
            );

        _dbContext.Employees.Add(new Employee
        {
            Id = 101,
            EmployeeNumber = "Emp001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@a.com",
            PhoneNumber = "1111111111",
            Position = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        });

        _dbContext.LeaveMasters.Add(new LeaveMaster
        {
            Id = 1,
            LeaveName = "Sick Leave",
            Description = "Sick Leave Description",
            MaxPerYear = 10,
            IsPaid = true
        });
        await _dbContext.SaveChangesAsync();

        _dbContext.EmployeeLeaveBalances.Add(new EmployeeLeaveBalance
        {
            EmployeeId = 101,
            LeaveMasterId = 1,
            TotalLeaves = 10,
            UsedLeaves = 0,
            RemainingLeaves = 10,
            Year = DateTime.Now.Year
        });

        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var dto = new LeaveRequestDto
        {
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(3),
            EndDate = DateTime.Now.AddDays(7),
            Reason = "Medical",
            Status = Status.Pending
        };

        // -------------------------------
        // Act
        // -------------------------------
        var result = await _leaveService.CreateLeaveRequestAsync(dto);

        // -------------------------------
        // Assert
        // -------------------------------
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.EmployeeId, Is.EqualTo(101));
        Assert.That(result.Data.LeaveTypeId, Is.EqualTo(1));
        Assert.That(result.Data.Reason, Is.EqualTo("Medical"));
        Assert.That(result.Data.Status, Is.EqualTo(Status.Pending));
        Assert.That(result.Data.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(3).Date));
        Assert.That(result.Data.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(7).Date));

    }

    [Test]
    public async Task CreateLeaveRequest_WhenInsufficientLeaveBalance()
    {
        // ---------------- ARRANGE ----------------

        // 1️⃣ Festival holidays (service dependency)
        _festivalHolidayServiceMock
            .Setup(f => f.GetFestivalHolidayAsync())
            .ReturnsAsync(
                ServiceResult<List<FestivalHolidayDto>>
                    .Success(new List<FestivalHolidayDto>())
            );

        // 2️⃣ Employee
        _dbContext.Employees.Add(new Employee
        {
            Id = 101,
            EmployeeNumber = "Emp001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@a.com",
            PhoneNumber = "1111111111",
            Position = "Developer",
            HireDate = DateTime.Now,
            IsActive = true,
            IsDeleted = false
        });

        // 3️⃣ Leave master (REQUIRED)
        _dbContext.LeaveMasters.Add(new LeaveMaster
        {
            Id = 1,
            LeaveName = "Sick Leave",
            MaxPerYear = 10,
            IsPaid = true,
            IsActive = true,
            IsDeleted = false
        });

        // 4️⃣ Employee leave balance (KEY DATA)
        _dbContext.EmployeeLeaveBalances.Add(new EmployeeLeaveBalance
        {
            EmployeeId = 101,
            LeaveMasterId = 1,
            TotalLeaves = 10,
            UsedLeaves = 9,
            RemainingLeaves = 1 // 🔥 insufficient
        });

        await _dbContext.SaveChangesAsync();

        // 5️⃣ Leave request DTO (needs > 1 day)
        var dto = new LeaveRequestDto
        {
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(3),
            EndDate = DateTime.Now.AddDays(7), // 5 days
            Reason = "Medical",
            Status = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        // ---------------- ACT ----------------
        var result = await _leaveService.CreateLeaveRequestAsync(dto);

        // ---------------- ASSERT ----------------
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Insufficient leave balance"));
    }


    [Test]
    public async Task CreateLeaveRequest_WhenEmployeeNotFound()
    {
        _festivalHolidayServiceMock
        .Setup(f => f.GetFestivalHolidayAsync())
        .ReturnsAsync(
            ServiceResult<List<FestivalHolidayDto>>
                .Success(new List<FestivalHolidayDto>())
        );
        var dto = new LeaveRequestDto
        {
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(3),
            EndDate = DateTime.Now.AddDays(7),
            Reason = "Medical",
            Status = Status.Pending
        };
        // -------------------------------
        // Act
        // -------------------------------
        var result = await _leaveService.CreateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Employee Id"));
    }

    [Test]
    public async Task GetLeaveRequestByEmployeeId_whenfoumnd()
    {
        _festivalHolidayServiceMock
        .Setup(f => f.GetFestivalHolidayAsync())
        .ReturnsAsync(
            ServiceResult<List<FestivalHolidayDto>>
                .Success(new List<FestivalHolidayDto>())
        );

        _dbContext.Employees.AddRange(
             new Employee
             {
                 Id = 101,
                 EmployeeNumber = "Emp001",
                 FirstName = "John",
                 LastName = "Doe",
                 Email = "john@a.com",
                 PhoneNumber = "1111111111",
                 Position = "Developer",
                 Salary = 50000,
                 DepartmentId = 1,
                 HireDate = DateTime.Now,
                 DateOfBirth = DateTime.Now.AddYears(-25)
             },
             new Employee
             {
                 Id = 102,
                 EmployeeNumber = "Emp002",
                 FirstName = "Jane",
                 LastName = "Smith",
                 Email = "jane@b.com",
                 PhoneNumber = "2222002222",
                 Position = "Tester",
                 Salary = 4000,
                 DepartmentId = 1,
                 HireDate = DateTime.Now,
                 DateOfBirth = DateTime.Now.AddYears(-24)
             }

         );

        await _dbContext.SaveChangesAsync();

        _dbContext.AddRange(
           new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
           new LeaveMaster { Id = 2, LeaveName = "Casual Leave", Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
       );

        await _dbContext.SaveChangesAsync();

        _dbContext.AddRange(
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", Status = Status.Pending },
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7), Reason = "Vacation", Status = Status.Approved },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(10), EndDate = DateTime.Now.AddDays(12), Reason = "Flu", Status = Status.Rejected },
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(15), EndDate = DateTime.Now.AddDays(18), Reason = "Family Function", Status = Status.Pending },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(20), EndDate = DateTime.Now.AddDays(22), Reason = "Conference", Status = Status.Approved }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.GetLeaveRequestByEmployeeIdAsync(101);

        Assert.That(result.Data!.Count, Is.EqualTo(3));
        Assert.That(result.Data[0].EmployeeId, Is.EqualTo(101));
        Assert.That(result.Data[1].EmployeeId, Is.EqualTo(101));
        Assert.That(result.Data[2].EmployeeId, Is.EqualTo(101));
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data![0].Reason, Is.Not.Null);
        Assert.That(result.Data![1].Reason, Is.Not.Null);
        Assert.That(result.Data![2].Reason, Is.Not.Null);
        Assert.That(result.Data![0].LeaveTypeId, Is.GreaterThan(0));
        Assert.That(result.Data![1].LeaveTypeId, Is.GreaterThan(0));
        Assert.That(result.Data![2].LeaveTypeId, Is.GreaterThan(0));
        Assert.That(result.Data![0].Id, Is.EqualTo(4));
        Assert.That(result.Data![1].Id, Is.EqualTo(2));
        Assert.That(result.Data![2].Id, Is.EqualTo(1));
        Assert.That(result.Data![0].Status, Is.EqualTo(Status.Pending));
        Assert.That(result.Data![1].Status, Is.EqualTo(Status.Approved));
        Assert.That(result.Data![2].Status, Is.EqualTo(Status.Pending));

    }

    [Test]
    public async Task GetLeaveRequestByEmployeeId_whenNotfoumnd()
    {
        var result = await _leaveService.GetLeaveRequestByEmployeeIdAsync(999);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.EqualTo(0));
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task DeleteLeaveRequestbyID_whenFound()
    {
        _dbContext.AddRange(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", Status = Status.Pending, IsActive = true, IsDeleted = false },
            new LeaveRequest { Id = 2, EmployeeId = 105, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7), Reason = "Vacation", Status = Status.Approved, IsActive = true, IsDeleted = false },
            new LeaveRequest { Id = 3, EmployeeId = 103, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(10), EndDate = DateTime.Now.AddDays(12), Reason = "Flu", Status = Status.Rejected, IsActive = true, IsDeleted = false },
            new LeaveRequest { Id = 4, EmployeeId = 104, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(15), EndDate = DateTime.Now.AddDays(18), Reason = "Family Function", Status = Status.Pending, IsActive = true, IsDeleted = false },
            new LeaveRequest { Id = 5, EmployeeId = 102, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(20), EndDate = DateTime.Now.AddDays(22), Reason = "Conference", Status = Status.Approved, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();

        Assert.That((await _dbContext.LeaveRequests.FindAsync(3))!.IsActive, Is.True);
        Assert.That((await _dbContext.LeaveRequests.FindAsync(3))!.IsDeleted, Is.False);

        var result = await _leaveService.DeleteLeaveRequestAsync(3);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That((await _dbContext.LeaveRequests.FindAsync(3))!.IsDeleted, Is.True);
        Assert.That((await _dbContext.LeaveRequests.FindAsync(3))!.IsActive, Is.False);
    }

    [Test]
    public async Task DeleteLeaveRequestbyID_whenNotFound()
    {
        _dbContext.AddRange(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", Status = Status.Pending, IsActive = true, IsDeleted = false },
            new LeaveRequest { Id = 2, EmployeeId = 105, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7), Reason = "Vacation", Status = Status.Approved, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _leaveService.DeleteLeaveRequestAsync(5);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.False);
    }

    [Test]
    public async Task UpdateLeaveRequest_WhenFounc()
    {
        _festivalHolidayServiceMock
        .Setup(f => f.GetFestivalHolidayAsync())
        .ReturnsAsync(
            ServiceResult<List<FestivalHolidayDto>>
                .Success(new List<FestivalHolidayDto>())
        );
        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", Status = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();

        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.LeaveTypeId, Is.EqualTo(1));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(1).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(3).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Reason, Is.EqualTo("Medical"));

        _dbContext.ChangeTracker.Clear();

        var dto = new LeaveRequestDto
        {
            Id = 1,
            EmployeeId = 101,
            LeaveTypeId = 2,
            StartDate = DateTime.Now.AddDays(2),
            EndDate = DateTime.Now.AddDays(4),
            Reason = "Medical Update",
            Status = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        var result = await _leaveService.UpdateLeaveRequestAsync(dto);

        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.LeaveTypeId, Is.EqualTo(2));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(2).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(4).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Reason, Is.EqualTo("Medical Update"));
    }

    [Test]
    public async Task UpdateLeaveRequest_WhenNotFound()
    {
        var dto = new LeaveRequestDto
        {
            EmployeeId = 101,
            LeaveTypeId = 2,
            StartDate = DateTime.Now.AddDays(2),
            EndDate = DateTime.Now.AddDays(4),
            Reason = "Medical Update",
            Status = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        var result = await _leaveService.UpdateLeaveRequestAsync(dto);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("No Data Found"));
    }

    [Test]
    public async Task UpdateLeaveRequest_WhenInsufficientLeaveBalance()
    {
        // -------- Festival Holidays --------
        _festivalHolidayServiceMock
            .Setup(f => f.GetFestivalHolidayAsync())
            .ReturnsAsync(
                ServiceResult<List<FestivalHolidayDto>>
                    .Success(new List<FestivalHolidayDto>())
            );

        // -------- Existing Leave Request --------
        _dbContext.LeaveRequests.Add(new LeaveRequest
        {
            Id = 1,
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(3),
            Reason = "Medical",
            Status = Status.Pending,
            IsActive = true,
            IsDeleted = false
        });

        // -------- REQUIRED Leave Master (ID = 2) --------
        _dbContext.LeaveMasters.Add(new LeaveMaster
        {
            Id = 2,
            LeaveName = "Sick Leave",
            MaxPerYear = 10,
            IsPaid = true,
            IsActive = true,
            IsDeleted = false
        });

        // -------- REQUIRED Leave Balance --------
        _dbContext.EmployeeLeaveBalances.Add(new EmployeeLeaveBalance
        {
            EmployeeId = 101,
            LeaveMasterId = 2,
            TotalLeaves = 10,
            UsedLeaves = 9,
            RemainingLeaves = 1 // 🔥 insufficient
        });

        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // -------- Update DTO (needs > 1 day) --------
        var dto = new LeaveRequestDto
        {
            Id = 1,
            EmployeeId = 101,
            LeaveTypeId = 2,
            StartDate = DateTime.Now.AddDays(2),
            EndDate = DateTime.Now.AddDays(6), // ~5 days
            Reason = "Medical Update",
            Status = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        // -------- ACT --------
        var result = await _leaveService.UpdateLeaveRequestAsync(dto);

        // -------- ASSERT --------
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Insufficient leave balance"));
    }


    [Test]
    public async Task UpdateLeaveRequest_WhenLeaveMasterNotFound()
    {
        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", Status = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var dto = new LeaveRequestDto
        {
            Id = 1,
            EmployeeId = 101,
            LeaveTypeId = 0, // Non-existing LeaveMasterId
            StartDate = DateTime.Now.AddDays(2),
            EndDate = DateTime.Now.AddDays(4),
            Reason = "Medical Update",
            Status = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };
        var result = await _leaveService.UpdateLeaveRequestAsync(dto);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Leave Type Id"));
    }

    [Test]
    public async Task UpdateLeaveRequestStatus_WhenApproved()
    {
        _festivalHolidayServiceMock
        .Setup(f => f.GetFestivalHolidayAsync())
        .ReturnsAsync(
            ServiceResult<List<FestivalHolidayDto>>
                .Success(new List<FestivalHolidayDto>())
        );
        _dbContext.Employees.AddRange(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@a.com",
                PhoneNumber = "1111111111",
                Position = "Developer",
                Salary = 50000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-25)
            }
        );

        await _dbContext.SaveChangesAsync();

        _dbContext.AddRange(
            new LeaveMaster { Id = 1, LeaveName = "Sick Leave", Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", Status = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Status, Is.EqualTo(Status.Pending));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Reason, Is.EqualTo("Medical"));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(1).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(3).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.LeaveTypeId, Is.EqualTo(1));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EmployeeId, Is.EqualTo(101));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Id, Is.EqualTo(1));

        var result = await _leaveService.UpdateStatusLeaveAsync(1, Status.Approved, 99);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Status, Is.EqualTo(Status.Approved));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Reason, Is.EqualTo("Medical"));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(1).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(3).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.LeaveTypeId, Is.EqualTo(1));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EmployeeId, Is.EqualTo(101));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Id, Is.EqualTo(1));

    }

    [Test]
    public async Task UpdateLeaveRequestStatus_WhenReject()
    {
        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", Status = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Status, Is.EqualTo(Status.Pending));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Reason, Is.EqualTo("Medical"));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(1).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(3).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.LeaveTypeId, Is.EqualTo(1));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EmployeeId, Is.EqualTo(101));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Id, Is.EqualTo(1));

        var result = await _leaveService.UpdateStatusLeaveAsync(1, Status.Rejected, 99);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Status, Is.EqualTo(Status.Rejected));
    }

    [Test]
    public async Task UpdateLeaveRequestStatus_WhenNotFound()
    {
        var result = await _leaveService.UpdateStatusLeaveAsync(0, Status.Approved, 99);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Leave Request Id"));
    }

    [Test]
    public async Task UpdateLeaveRequestStatusCancel_WhenFound()
    {
        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", Status = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Status, Is.EqualTo(Status.Pending));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Reason, Is.EqualTo("Medical"));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(1).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(3).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.LeaveTypeId, Is.EqualTo(1));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EmployeeId, Is.EqualTo(101));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Id, Is.EqualTo(1));

        var result = await _leaveService.UpdateLeaveRequestStatusCancelAsync(1, 101);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Status, Is.EqualTo(Status.Cancelled));
    }

    [Test]
    public async Task UpdateLeaveRequestStatusCancel_WhenNotFound()
    {
        var result = await _leaveService.UpdateLeaveRequestStatusCancelAsync(0, 101);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Id"));
    }

    [Test]
    public async Task UpdateLeaveRequestStatusCancel_WhenEmployeeIdMismatch()
    {
        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", Status = Status.Pending    , IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var result = await _leaveService.UpdateLeaveRequestStatusCancelAsync(1, 0);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Id"));
    }

    [Test]
    public async Task GetALLLeaveBalance()
    {
        await UpdateLeaveRequestStatus_WhenApproved();
        var result = await _leaveService.GetLeaveBalanceAsync();
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetLeaveBalanceByEmployeeId_WhenFound()
    {
        await UpdateLeaveRequestStatus_WhenApproved();
        var result = await _leaveService.GetLeaveBalanceByEmployeeIdAsync(101);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data[0].EmployeeId, Is.EqualTo(101));
        Assert.That(result.Data[0].LeaveMasterId, Is.EqualTo(1));
        Assert.That(result.Data[0].TotalLeaves, Is.EqualTo(10));
        Assert.That(result.Data[0].UsedLeaves, Is.EqualTo(2));
        Assert.That(result.Data[0].RemainingLeaves, Is.EqualTo(8));
    }

    [Test]
    public async Task GetLeaveBalanceByEmployeeId_WhenNotFound()
    {
        var result = await _leaveService.GetLeaveBalanceByEmployeeIdAsync(0);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Employee Id"));
    }

    [Test]
    public async Task DeleteLeaveBalance_WhenFound()
    {
        await UpdateLeaveRequestStatus_WhenApproved();

        var leaveRequest = await _dbContext.LeaveRequests.FindAsync(1);
        Assert.That(leaveRequest!.IsActive, Is.EqualTo(true));
        Assert.That(leaveRequest.IsDeleted, Is.EqualTo(false));
        Assert.That(leaveRequest.Id, Is.EqualTo(1));
        Assert.That(leaveRequest.EmployeeId, Is.EqualTo(101));
        Assert.That(leaveRequest.LeaveTypeId, Is.EqualTo(1));

        var balance = await _dbContext.EmployeeLeaveBalances
            .FirstOrDefaultAsync(b => b.EmployeeId == 101 && b.LeaveMasterId == 1);
        Assert.That(balance, Is.Not.Null, "Expected an EmployeeLeaveBalance to exist after approval.");

        var result = await _leaveService.DeleteLeaveBalanceAsync(balance!.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);

        var deletedBalance = await _dbContext.EmployeeLeaveBalances.FindAsync(balance.Id);
       
        Assert.That(deletedBalance.IsActive, Is.EqualTo(false));
        Assert.That(deletedBalance.IsDeleted, Is.EqualTo(true));
    }

    [Test]
    public async Task DeleteLeaveBalance_WhenNotFound()
    {
        var result = await _leaveService.DeleteLeaveBalanceAsync(0);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Leave Balance Id"));
    }

    [Test]
    public async Task DeleteLeaveBalance_WhenAlreadyDeleted()
    {
        await UpdateLeaveRequestStatus_WhenApproved();
        var balance = await _dbContext.EmployeeLeaveBalances
            .FirstOrDefaultAsync(b => b.EmployeeId == 101 && b.LeaveMasterId == 1);
        Assert.That(balance, Is.Not.Null, "Expected an EmployeeLeaveBalance to exist after approval.");
        // First deletion
        var firstDeleteResult = await _leaveService.DeleteLeaveBalanceAsync(balance!.Id);
        Assert.That(firstDeleteResult.IsSuccess, Is.True);
        Assert.That(firstDeleteResult.Data, Is.True);
        // Attempt to delete again
        var secondDeleteResult = await _leaveService.DeleteLeaveBalanceAsync(balance.Id);
        Assert.That(secondDeleteResult.IsSuccess, Is.False);
        Assert.That(secondDeleteResult.Message, Is.EqualTo("Leave Balance not found"));
    }

    [Test]
    public async Task UpdateLeaveBalance_whenFound()
    {
        _dbContext.Add(
            new EmployeeLeaveBalance
            {
                Id = 1,
                EmployeeId = 101,
                LeaveMasterId = 1,
                TotalLeaves = 10,
                RemainingLeaves = 3,
                UsedLeaves = 7
            }
        );
        await _dbContext.SaveChangesAsync();


        var leaveRequest = await _dbContext.EmployeeLeaveBalances.FindAsync(1);
       
        Assert.That(leaveRequest.Id, Is.EqualTo(1));
        Assert.That(leaveRequest.EmployeeId, Is.EqualTo(101));
        Assert.That(leaveRequest.TotalLeaves, Is.EqualTo(10));
        Assert.That(leaveRequest.RemainingLeaves, Is.EqualTo(3));
        Assert.That(leaveRequest.UsedLeaves, Is.EqualTo(7));

        _dbContext.ChangeTracker.Clear();

        var dto = new EmployeeLeaveBalanceDto
        {
            Id = 1,
            EmployeeId = 101,
            LeaveMasterId = 2,
            TotalLeaves = 10,
            RemainingLeaves = 4,
            UsedLeaves = 6
        };

        var result = await _leaveService.UpdateLeaveBalanceAsync(dto);
        Assert.That(result.IsSuccess, Is.EqualTo(true));
        Assert.That(result.Data!.UsedLeaves, Is.EqualTo(6));
        Assert.That(result.Data!.Id, Is.EqualTo(1));
        Assert.That(result.Data!.RemainingLeaves, Is.EqualTo(4));
        Assert.That(result.Data!.EmployeeId, Is.EqualTo(101));
    }

    [Test]
    public async Task UpdateLeaveBalance_whenNullFound()
    {
        var result = await _leaveService.UpdateLeaveBalanceAsync(null);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Leave Balance Data"));
    }

    [Test]
    public async Task UpdateLeaveBalance_whenNotFound()
    {
        var dto = new EmployeeLeaveBalanceDto
        {
            Id = 0,
            EmployeeId = 101,
            LeaveMasterId = 2,
            TotalLeaves = 10,
            RemainingLeaves = 4,
            UsedLeaves = 6
        };
        var result = await _leaveService.UpdateLeaveBalanceAsync(dto);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Leave Balance Data"));
    }
}