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
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Threading.Channels;


namespace arna.HRMS.Tests.Unit.Services;

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
        var leaveTypeBaseRepo = new BaseRepository<Core.Entities.LeaveType>(_dbContext);
        var attendanceBaseRepo = new BaseRepository<Attendance>(_dbContext);
        var festivalBaseRepo = new BaseRepository<FestivalHoliday>(_dbContext);
        var employeeBaseRepo = new BaseRepository<Employee>(_dbContext);

        var leaveRepository = new LeaveRepository(
            leaveTypeBaseRepo,
            leaveRequestBaseRepo
        );

        var festivalHolidayRepository = new FestivalHolidayRepository(festivalBaseRepo);
        var employeeRepository = new EmployeeRepository(employeeBaseRepo);

        _attendanceRepository = new AttendanceRepository(attendanceBaseRepo, festivalHolidayRepository, employeeRepository);

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
    public async Task GetLeaveTypeAsync_WhenDbEmpty()
    {
        var result = await _leaveService.GetLeaveTypeAsync();
        Assert.That(result.Data, Is.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Not Found"));
    }

    [Test]
    public async Task GetLeaveTypeAsync_whenFounc()
    {
        _dbContext.AddRange(
            new LeaveType { LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true },
            new LeaveType { LeaveNameId = LeaveName.AnnualLeave, Description = "Earned Leave Description", MaxPerYear = 15, IsPaid = true },
            new LeaveType { LeaveNameId = LeaveName.UnpaidLeave, Description = "Unpaid Leave Description", MaxPerYear = 5, IsPaid = false },
            new LeaveType { LeaveNameId = LeaveName.MaternityLeave , Description = "Maternity Leave Description", MaxPerYear = 90, IsPaid = true },
            new LeaveType { LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = false }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.GetLeaveTypeAsync();

        Assert.That(result.Data!.Count, Is.EqualTo(6));
        Assert.That(result.Data[5].LeaveNameId, Is.EqualTo(LeaveName.SickLeave));
        Assert.That(result.Data[0].IsPaid, Is.False);
        Assert.That(result.Data[3].MaxPerYear, Is.EqualTo(15));
        Assert.That(result.Data[4].Description, Is.EqualTo("Casual Leave Description"));
        Assert.That(result.Data[2].Id, Is.EqualTo(4));
        Assert.That(result.Data[1].IsPaid, Is.True);
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task GetLeaveTypeById_whenFound()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _leaveService.GetLeaveTypeByIdAsync(2);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.LeaveNameId, Is.EqualTo(LeaveName.CasualLeave));
        Assert.That(result.Data.MaxPerYear, Is.EqualTo(8));
        Assert.That(result.Data.IsPaid, Is.True);
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task GetLeaveTypeById_whenNotFound()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _leaveService.GetLeaveTypeByIdAsync(5);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("leave not found"));
    }

    [Test]
    public async Task GetLeaveTypeById_whenDeleted()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        await _leaveService.DeleteLeaveTypeAsync(2);

        var result = await _leaveService.GetLeaveTypeByIdAsync(2);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("leave not found"));
    }

    [Test]
    public async Task GetLeaveTypeById_WnenNullorNegative()
    {
        var result = await _leaveService.GetLeaveTypeByIdAsync(-1);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid ID"));

        var result2 = await _leaveService.GetLeaveTypeByIdAsync(0);
        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Is.EqualTo("Invalid ID"));
    }

    [Test]
    public async Task CreateLeaveType_WhenLeaveNameAlreadyExists()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        var newLeaveType = new LeaveTypeDto
        {
            LeaveNameId = LeaveName.SickLeave,
            Description = "Duplicate Sick Leave Description",
            MaxPerYear = 12,
            IsPaid = true
        };

        var result = await _leaveService.CreateLeaveTypeAsync(newLeaveType);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo($"Leave '{newLeaveType.LeaveNameId}' already exists"));
    }

    [Test]
    public async Task CreateLeaveType_WhenNameIsMissing()
    {
        var newLeaveType = new LeaveTypeDto
        {
            Description = "Duplicate Sick Leave Description",
            MaxPerYear = 12,
            IsPaid = true
        };

        var result = await _leaveService.CreateLeaveTypeAsync(newLeaveType);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Leave name is required"));
    }

    [Test]
    public async Task CreateLeaveType_WhenMessingMaxPerYear()
    {
        var newLeaveType = new LeaveTypeDto
        {
            LeaveNameId = LeaveName.SickLeave,
            Description = "Sick Leave Description",
            IsPaid = true
        };

        var result = await _leaveService.CreateLeaveTypeAsync(newLeaveType);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("number of days is required"));
    }

    [Test]
    public async Task CreateLeaveType_WhenNullInput()
    {
        var result = await _leaveService.CreateLeaveTypeAsync(null!);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Data not Found"));
    }

    [Test]
    public async Task CreateLeaveType_WhenSuccess()
    {
        var newLeaveType = new LeaveTypeDto
        {
            LeaveNameId = LeaveName.PaternityLeave,
            Description = "Paternity Leave Description",
            MaxPerYear = 15
        };
        var result = await _leaveService.CreateLeaveTypeAsync(newLeaveType);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.GreaterThan(0));
        Assert.That(result.Data.LeaveNameId, Is.EqualTo(LeaveName.PaternityLeave));
        Assert.That(result.Data.MaxPerYear, Is.EqualTo(15));
        Assert.That(result.Data.IsPaid, Is.True);
        Assert.That(result.Data!.Description, Is.EqualTo("Paternity Leave Description"));
        Assert.That(result.IsSuccess, Is.True);
    }

    
    [Test]
    public async Task DeleteLeaveType_WhenFound()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        Assert.That((await _dbContext.LeaveTypes.FindAsync(1))!.IsActive, Is.True);
        Assert.That((await _dbContext.LeaveTypes.FindAsync(1))!.IsDeleted, Is.False);
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.IsActive, Is.True);
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.IsDeleted, Is.False);

        var result = await _leaveService.DeleteLeaveTypeAsync(1);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.IsActive, Is.True);
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.IsDeleted, Is.False);

        var deletedLeaveType = await _dbContext.LeaveTypes.FindAsync(1);
        Assert.That(deletedLeaveType!.IsActive, Is.False);
        Assert.That(deletedLeaveType.IsDeleted, Is.True);
    }

    [Test]
    public async Task DeleteLeaveType_WhenNotFound()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.DeleteLeaveTypeAsync(5);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
    }

    [Test]
    public async Task DeleteLeaveType_WhenAlreadyDeleted()
    {
        _dbContext.Add(
           new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true , IsActive= false, IsDeleted=true}
        );

        await _dbContext.SaveChangesAsync();
        var result = await _leaveService.DeleteLeaveTypeAsync(1);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Leave Type not found"));
    }

    [Test]
    public async Task DeleteLeaveType_WhenNullOrWhiteSpace()
    {
        var result = await _leaveService.DeleteLeaveTypeAsync(5);
    }

    [Test]
    public async Task UpdateLeaveType_whenFound()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.LeaveNameId, Is.EqualTo(LeaveName.CasualLeave));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.Description, Is.EqualTo("Casual Leave Description"));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.MaxPerYear, Is.EqualTo(8));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.IsPaid, Is.True);

        _dbContext.ChangeTracker.Clear();

        var updatedLeaveType = new LeaveTypeDto
        {
            Id = 2,
            LeaveNameId = LeaveName.PaternityLeave,
            Description = "Updated Description",
            MaxPerYear = 12,
            IsPaid = false
        };
        var result = await _leaveService.UpdateLeaveTypeAsync(updatedLeaveType);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(2));
        Assert.That(result.Data.LeaveNameId, Is.EqualTo(LeaveName.PaternityLeave));
        Assert.That(result.Data.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Data.MaxPerYear, Is.EqualTo(12));
        Assert.That(result.Data.IsPaid, Is.False);
    }

    [Test]
    public async Task UpdateLeaveType_whenAlreadyDeleted()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true ,  IsActive = true, IsDeleted = false },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true , IsActive=false, IsDeleted=true}
        );
        await _dbContext.SaveChangesAsync();

        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.LeaveNameId, Is.EqualTo(LeaveName.CasualLeave));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.Description, Is.EqualTo("Casual Leave Description"));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.MaxPerYear, Is.EqualTo(8));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.IsPaid, Is.True);

        _dbContext.ChangeTracker.Clear();

        var updatedLeaveType = new LeaveTypeDto
        {
            Id = 2,
            LeaveNameId = LeaveName.PaternityLeave,
            Description = "Updated Description",
            MaxPerYear = 12,
            IsPaid = false
        };
        var result = await _leaveService.UpdateLeaveTypeAsync(updatedLeaveType);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("No Such Data Found"));
    }

    [Test]
    public async Task UpdateLeaveType_WhereIdIsZero()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true, IsActive = true, IsDeleted = false },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();

        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.LeaveNameId, Is.EqualTo(LeaveName.CasualLeave));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.Description, Is.EqualTo("Casual Leave Description"));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.MaxPerYear, Is.EqualTo(8));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.IsPaid, Is.True);

        _dbContext.ChangeTracker.Clear();

        var updatedLeaveType = new LeaveTypeDto
        {
            Id = 0,
            LeaveNameId = LeaveName.SickLeave,
            Description = "Updated Description",
            MaxPerYear = 12,
            IsPaid = false
        };
        var result = await _leaveService.UpdateLeaveTypeAsync(updatedLeaveType);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to update Leave Type"));
    }

    public async Task UpdateLeaveType_WhereIdIsNegative()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true, IsActive = true, IsDeleted = false },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();

        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.LeaveNameId, Is.EqualTo(LeaveName.CasualLeave));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.Description, Is.EqualTo("Casual Leave Description"));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.MaxPerYear, Is.EqualTo(8));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.IsPaid, Is.True);

        _dbContext.ChangeTracker.Clear();

        var updatedLeaveType = new LeaveTypeDto
        {
            Id = -1,
            LeaveNameId = LeaveName.SickLeave,
            Description = "Updated Description",
            MaxPerYear = 12,
            IsPaid = false
        };
        var result = await _leaveService.UpdateLeaveTypeAsync(updatedLeaveType);
        Assert.That(result, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to update Leave Type"));
    }

    [Test]
    public async Task UpdateLeaveType_WhereAlreadyHaveName()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true, IsActive = true, IsDeleted = false },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();

        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.LeaveNameId, Is.EqualTo(LeaveName.CasualLeave));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.Description, Is.EqualTo("Casual Leave Description"));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.MaxPerYear, Is.EqualTo(8));
        Assert.That((await _dbContext.LeaveTypes.FindAsync(2))!.IsPaid, Is.True);

        _dbContext.ChangeTracker.Clear();

        var updatedLeaveType = new LeaveTypeDto
        {
            Id = 2,
            LeaveNameId = LeaveName.SickLeave,
            Description = "Updated Description",
            MaxPerYear = 12,
            IsPaid = false
        };
        var result = await _leaveService.UpdateLeaveTypeAsync(updatedLeaveType);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo($"Leave '{updatedLeaveType.LeaveNameId}' already exists"));
    }

    [Test]
    public async Task UpdateLeaveType_whenNotFound()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var updatedLeaveType = new LeaveTypeDto
        {
            Id = 5,
            LeaveNameId = LeaveName.PaternityLeave,
            Description = "Updated Description",
            MaxPerYear = 12,
            IsPaid = false
        };

        var result = await _leaveService.UpdateLeaveTypeAsync(updatedLeaveType);

        Assert.That(result.IsSuccess, Is.False);

        Assert.That(result.Message, Is.EqualTo("No Such Data Found"));
    }

    [Test]
    public async Task UpdateLeaveType_WhenNameIsNullOrWhitespace()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var updatedLeaveType = new LeaveTypeDto
        {
            Id = 2,
            Description = "Updated Description",
            MaxPerYear = 12,
            IsPaid = false
        };
        var result = await _leaveService.UpdateLeaveTypeAsync(updatedLeaveType);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to update Leave Type"));

    }

    [Test]
    public async Task UpdateLeaveType_WhenMaxPerYearIsMissing()
    {
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var updatedLeaveType = new LeaveTypeDto
        {
            Id = 2,
            LeaveNameId = LeaveName.PaternityLeave,
            Description = "Updated Description",
            IsPaid = false
        };
        var result = await _leaveService.UpdateLeaveTypeAsync(updatedLeaveType);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to update Leave Type"));
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
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.AddRange(
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7), Reason = "Vacation", StatusId = Status.Approved },
            new LeaveRequest { EmployeeId = 103, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(10), EndDate = DateTime.Now.AddDays(12), Reason = "Flu", StatusId = Status.Rejected },
            new LeaveRequest { EmployeeId = 104, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(15), EndDate = DateTime.Now.AddDays(18), Reason = "Family Function", StatusId = Status.Pending }
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
        Assert.That(result.Data![0].StatusId, Is.EqualTo(Status.Pending));
        Assert.That(result.Data![1].StatusId, Is.EqualTo(Status.Rejected));
        Assert.That(result.Data![2].StatusId, Is.EqualTo(Status.Approved));
        Assert.That(result.Data![3].StatusId, Is.EqualTo(Status.Pending));
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
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.AddRange(
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7), Reason = "Vacation", StatusId = Status.Approved },
            new LeaveRequest { EmployeeId = 103, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(10), EndDate = DateTime.Now.AddDays(12), Reason = "Flu", StatusId = Status.Rejected },
            new LeaveRequest { EmployeeId = 104, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(15), EndDate = DateTime.Now.AddDays(18), Reason = "Family Function", StatusId = Status.Pending }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.GetByFilterAsync(Status.Pending,null);

        Assert.That(result.Data!.Count, Is.EqualTo(2));
        Assert.That(result.Data[0].EmployeeId, Is.EqualTo(104));
        Assert.That(result.Data[1].EmployeeId, Is.EqualTo(101));
        Assert.That(result.Data[0].StatusId, Is.EqualTo(Status.Pending));
        Assert.That(result.Data[1].StatusId, Is.EqualTo(Status.Pending));
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
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.AddRange(
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(7), EndDate = DateTime.Now.AddDays(5), Reason = "Vacation", StatusId = Status.Approved },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(15), EndDate = DateTime.Now.AddDays(12), Reason = "Flu", StatusId = Status.Rejected }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.GetLeaveRequestByIdAsync(2);

        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.EmployeeId, Is.EqualTo(102));
        Assert.That(result.Data.LeaveTypeId, Is.EqualTo(2));
        Assert.That(result.Data.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(7).Date));
        Assert.That(result.Data.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(5).Date));
        Assert.That(result.Data.Reason, Is.EqualTo("Vacation"));
        Assert.That(result.Data.StatusId, Is.EqualTo(Status.Approved));
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task GetLeaveRequestById_whenDeleted()
    {
        _dbContext.Employees.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@gmail.com",
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
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.AddRange(
            new LeaveRequest
            {
                Id = 3,
                EmployeeId = 101,
                LeaveTypeId = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Reason = "Medical", 
                StatusId = Status.Pending,
                IsActive = false,
                IsDeleted = true
            }
        );
        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.GetLeaveRequestByIdAsync(3);

        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("leave request not found"));
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
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );

        await _dbContext.SaveChangesAsync();

        _dbContext.AddRange(
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7), Reason = "Vacation", StatusId = Status.Approved },
            new LeaveRequest { EmployeeId = 103, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(10), EndDate = DateTime.Now.AddDays(12), Reason = "Flu", StatusId = Status.Rejected },
            new LeaveRequest { EmployeeId = 104, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(15), EndDate = DateTime.Now.AddDays(18), Reason = "Family Function", StatusId = Status.Pending }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.GetLeaveRequestByIdAsync(5);

        Assert.That(result.Message, Is.EqualTo("leave request not found"));
        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public async Task GetLeaveRequestById_WhenNullOrNegative()
    {
        var result = await _leaveService.GetLeaveRequestByIdAsync(-1);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid ID"));
        var result2 = await _leaveService.GetLeaveRequestByIdAsync(0);
        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Is.EqualTo("Invalid ID"));
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

        _dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = 1,
            LeaveNameId = LeaveName.SickLeave,
            Description = "Sick Leave Description",
            MaxPerYear = 10,
            IsPaid = true
        });
        await _dbContext.SaveChangesAsync();

        var dto = new LeaveRequestDto
        {
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(3),
            EndDate = DateTime.Now.AddDays(7),
            Reason = "Medical",
            StatusId = Status.Pending
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
        Assert.That(result.Data.StatusId, Is.EqualTo(Status.Pending));
        Assert.That(result.Data.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(3).Date));
        Assert.That(result.Data.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(7).Date));

    }

    [Test] 
    public async Task CreateLeaveRequest_WhenLeaveTypeNotFound()
    {
        _festivalHolidayServiceMock
        .Setup(f => f.GetFestivalHolidayAsync())
        .ReturnsAsync(
            ServiceResult<List<FestivalHolidayDto>>
                .Success(new List<FestivalHolidayDto>())
        );
        var dto = new LeaveRequestDto
        {
            EmployeeId = 101,
            StartDate = DateTime.Now.AddDays(3),
            EndDate = DateTime.Now.AddDays(7),
            Reason = "Medical",
            StatusId = Status.Pending
        };
        // -------------------------------
        // Act
        // -------------------------------
        var result = await _leaveService.CreateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Leave Type Id"));
    }

    [Test]
    public async Task CreateLeaveRequest_WhenStartDateAfterEndDate()
    {
        _festivalHolidayServiceMock
        .Setup(f => f.GetFestivalHolidayAsync())
        .ReturnsAsync(
            ServiceResult<List<FestivalHolidayDto>>
                .Success(new List<FestivalHolidayDto>())
        );
        var dto = new LeaveRequestDto
        {
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(7),
            EndDate = DateTime.Now.AddDays(3),
            Reason = "Medical",
            StatusId = Status.Pending
        };
        // -------------------------------
        // Act
        // -------------------------------
        var result = await _leaveService.CreateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Date you select "));
    }

    [Test]
    public async Task CreateLeaveRequest_WhenStartDateInPast()
    {
        _festivalHolidayServiceMock
        .Setup(f => f.GetFestivalHolidayAsync())
        .ReturnsAsync(
            ServiceResult<List<FestivalHolidayDto>>
                .Success(new List<FestivalHolidayDto>())
        );
        var dto = new LeaveRequestDto
        {
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(-3),
            EndDate = DateTime.Now.AddDays(3),
            Reason = "Medical",
            StatusId = Status.Pending
        };
        // -------------------------------
        // Act
        // -------------------------------
        var result = await _leaveService.CreateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Date you select "));
    }

    [Test]
    public async Task CreateLeaveRequest_WhenEndDateEqualCurrentDate()
    {
        _festivalHolidayServiceMock
        .Setup(f => f.GetFestivalHolidayAsync())
        .ReturnsAsync(
            ServiceResult<List<FestivalHolidayDto>>
                .Success(new List<FestivalHolidayDto>())
        );
        var dto = new LeaveRequestDto
        {
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now,
            Reason = "Medical",
            StatusId = Status.Pending
        };
        // -------------------------------
        // Act
        // -------------------------------
        var result = await _leaveService.CreateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
    }

    [Test]
    public async Task CreateLeaveRequest_WhenStartDateIsMissing()
    {
        _festivalHolidayServiceMock
        .Setup(f => f.GetFestivalHolidayAsync())
        .ReturnsAsync(
            ServiceResult<List<FestivalHolidayDto>>
                .Success(new List<FestivalHolidayDto>())
        );
        var dto = new LeaveRequestDto
        {
            EmployeeId = 101,
            LeaveTypeId = 1,
            EndDate = DateTime.Now.AddDays(4),
            Reason = "Medical",
            StatusId = Status.Pending
        };
        // -------------------------------
        // Act
        // -------------------------------
        var result = await _leaveService.CreateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Date you select "));
    }

    [Test]
    public async Task CreateLeaveRequest_WhenEndDateIsMissing()
    {
        _festivalHolidayServiceMock
        .Setup(f => f.GetFestivalHolidayAsync())
        .ReturnsAsync(
            ServiceResult<List<FestivalHolidayDto>>
                .Success(new List<FestivalHolidayDto>())
        );
        var dto = new LeaveRequestDto
        {
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(4),
            Reason = "Medical",
            StatusId = Status.Pending
        };
        // -------------------------------
        // Act
        // -------------------------------
        var result = await _leaveService.CreateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Date you select "));
    }

    [Test]
    public async Task CreateLeaveRequest_WhenResoneIsMissing()
    {
        _festivalHolidayServiceMock
        .Setup(f => f.GetFestivalHolidayAsync())
        .ReturnsAsync(
            ServiceResult<List<FestivalHolidayDto>>
                .Success(new List<FestivalHolidayDto>())
        );
        var dto = new LeaveRequestDto
        {
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(4),
            EndDate = DateTime.Now.AddDays(4),
            StatusId = Status.Pending
        };
        // -------------------------------
        // Act
        // -------------------------------
        var result = await _leaveService.CreateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Reason is required"));
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
            StatusId = Status.Pending
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
           new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
           new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
       );

        await _dbContext.SaveChangesAsync();

        _dbContext.AddRange(
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending },
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7), Reason = "Vacation", StatusId = Status.Approved },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(10), EndDate = DateTime.Now.AddDays(12), Reason = "Flu", StatusId = Status.Rejected },
            new LeaveRequest { EmployeeId = 101, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(15), EndDate = DateTime.Now.AddDays(18), Reason = "Family Function", StatusId = Status.Pending },
            new LeaveRequest { EmployeeId = 102, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(20), EndDate = DateTime.Now.AddDays(22), Reason = "Conference", StatusId = Status.Approved }
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
        Assert.That(result.Data![0].StatusId, Is.EqualTo(Status.Pending));
        Assert.That(result.Data![1].StatusId, Is.EqualTo(Status.Approved));
        Assert.That(result.Data![2].StatusId, Is.EqualTo(Status.Pending));

    }

    [Test]
    public async Task GetLeaveRequestByEmployeeId_whenNotfoumnd()
    {
        var result = await _leaveService.GetLeaveRequestByEmployeeIdAsync(999);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("leave requests not found for this employee"));
    }

    [Test]
    public async Task GetLeaveRequestByEmployeeId_whenIsZeroOrNegative()
    {
        var result = await _leaveService.GetLeaveRequestByEmployeeIdAsync(0);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Employee Id"));
        var result2 = await _leaveService.GetLeaveRequestByEmployeeIdAsync(-99);
        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Is.EqualTo("Invalid Employee Id"));
    }

    [Test]
    public async Task GetLeaveRequestByEmployeeId_WhenAlreadyDeleted()
    {
        _dbContext.Employees.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@gmail.com",
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
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true },
            new LeaveType { Id = 2, LeaveNameId = LeaveName.CasualLeave, Description = "Casual Leave Description", MaxPerYear = 8, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.Add(
            new LeaveRequest
            {
                Id = 3,
                EmployeeId = 101,
                LeaveTypeId = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Reason = "Medical",
                StatusId = Status.Pending,
                IsActive = false,
                IsDeleted = true
            }
        );
        await _dbContext.SaveChangesAsync();

        var result = await _leaveService.GetLeaveRequestByEmployeeIdAsync(101);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("leave requests not found for this employee"));
    }
    [Test]
    public async Task DeleteLeaveRequestbyID_whenFound()
    {
        // -------------------------
        // Arrange
        // -------------------------

        _dbContext.Employees.Add(new Employee
        {
            Id = 103,
            EmployeeNumber = "EMP103",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@a.com",
            PhoneNumber = "9999999999",
            Position = "Dev",
            Salary = 50000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        });

        _dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = 1,
            LeaveNameId = LeaveName.SickLeave,
            MaxPerYear = 10,
            IsPaid = true,
            IsActive = true,
            IsDeleted = false
        });

        _dbContext.LeaveRequests.Add(new LeaveRequest
        {
            Id = 3,
            EmployeeId = 103,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(10),
            EndDate = DateTime.Now.AddDays(12),
            Reason = "Flu",
            StatusId = Status.Pending, // ✅ must be Approved
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // -------------------------
        // Act
        // -------------------------
        var result = await _leaveService.DeleteLeaveRequestAsync(3);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);

        var deleted = await _dbContext.LeaveRequests.FindAsync(3);
        Assert.That(deleted, Is.Not.Null);
        Assert.That(deleted!.IsDeleted, Is.True);
        Assert.That(deleted.IsActive, Is.False);
    }

    [Test]
    public async Task DeleteLeaveRequestbyID_whenAlreadyDeleted()
    {
        _dbContext.AddRange(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = false, IsDeleted = true },
            new LeaveRequest { Id = 2, EmployeeId = 105, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7), Reason = "Vacation", StatusId = Status.Approved, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _leaveService.DeleteLeaveRequestAsync(1);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("not found"));
    }


    [Test]
    public async Task DeleteLeaveRequestbyID_whenNotFound()
    {
        _dbContext.AddRange(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = true, IsDeleted = false },
            new LeaveRequest { Id = 2, EmployeeId = 105, LeaveTypeId = 2, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7), Reason = "Vacation", StatusId = Status.Approved, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _leaveService.DeleteLeaveRequestAsync(5);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.False);
        Assert.That(result.Message, Is.EqualTo("not found"));
    }
    [Test]
    public async Task DeleteLeaveRequestbyID_whenIsZeroOrNegative()
    {
        var result = await _leaveService.DeleteLeaveRequestAsync(0);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid ID"));

        var result2 = await _leaveService.DeleteLeaveRequestAsync(-99);
        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Is.EqualTo("Invalid ID"));

    }

    [Test]
    public async Task UpdateLeaveRequest_WhenAlreadyDeleted()
    {
        _festivalHolidayServiceMock
        .Setup(f => f.GetFestivalHolidayAsync())
        .ReturnsAsync(
            ServiceResult<List<FestivalHolidayDto>>
                .Success(new List<FestivalHolidayDto>())
        );

        _dbContext.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@gmail.com",
                PhoneNumber = "1111111111",
                Position = "Developer",
                DateOfBirth = DateTime.Now.AddYears(-25),
                DepartmentId = 1,
                HireDate = DateTime.Now,
                Salary = 50000
            }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );

        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = false, IsDeleted = true }
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
            StatusId = Status.Pending
        };

        var result = await _leaveService.UpdateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("No Such Data Found")); 

    }

    [Test]
    public async Task UpdateLeaveRequest_WhenStartDateIsMissing()
    {
        _dbContext.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "johngmail.com",
                PhoneNumber = "1111111111",
                DateOfBirth = DateTime.Now.AddYears(-22),
                HireDate = DateTime.Now,
                Salary = 50000,
                Position = "Developer",
                DepartmentId = 1
            }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

         _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = true, IsDeleted = false }
         );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var dto = new LeaveRequestDto
        {
            Id = 1,
            EmployeeId = 101,
            LeaveTypeId = 1,
            EndDate = DateTime.Now.AddDays(4),
            Reason = "Medical Update",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        var result = await _leaveService.UpdateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Date you select "));

    }

    [Test]
    public async Task UpdateLeaveRequest_WhenEndDateIsMissing()
    {
        _dbContext.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "johngmail.com",
                PhoneNumber = "1111111111",
                DateOfBirth = DateTime.Now.AddYears(-22),
                HireDate = DateTime.Now,
                Salary = 50000,
                Position = "Developer",
                DepartmentId = 1
            }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
           new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var dto = new LeaveRequestDto
        {
            Id = 1,
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(4),
            Reason = "Medical Update",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        var result = await _leaveService.UpdateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Date you select "));

    }

    [Test]
    public async Task UpdateLeaveRequest_WhenStartDateInPast()
    {
        _dbContext.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "johngmail.com",
                PhoneNumber = "1111111111",
                DateOfBirth = DateTime.Now.AddYears(-22),
                HireDate = DateTime.Now,
                Salary = 50000,
                Position = "Developer",
                DepartmentId = 1
            }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
           new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var dto = new LeaveRequestDto
        {
            Id = 1,
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(-20),
            EndDate = DateTime.Now.AddDays(20),
            Reason = "Medical Update",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        var result = await _leaveService.UpdateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Date you select "));

    }

    [Test]
    public async Task UpdateLeaveRequest_WhenEndDateInPast()
    {
        _dbContext.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "johngmail.com",
                PhoneNumber = "1111111111",
                DateOfBirth = DateTime.Now.AddYears(-22),
                HireDate = DateTime.Now,
                Salary = 50000,
                Position = "Developer",
                DepartmentId = 1
            }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
           new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var dto = new LeaveRequestDto
        {
            Id = 1,
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate= DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(-14),
            Reason = "Medical Update",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        var result = await _leaveService.UpdateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Date you select "));

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
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@gmail.com",
                PhoneNumber = "1111111111",
                Position = "Developer",
                DateOfBirth = DateTime.Now.AddYears(-25),
                DepartmentId = 1,
                HireDate = DateTime.Now,
                Salary = 50000
            }
        );

        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = true, IsDeleted = false }
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
            StatusId = Status.Pending,
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
    public async Task UpdateLeaveRequest_WhenStartDateIsCurrentDate()
    {
        _dbContext.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "johngmail.com",
                PhoneNumber = "1111111111",
                DateOfBirth = DateTime.Now.AddYears(-22),
                HireDate = DateTime.Now,
                Salary = 50000,
                Position = "Developer",
                DepartmentId = 1
            }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
           new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var dto = new LeaveRequestDto
        {
            Id = 1,
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(4),
            Reason = "Medical Update",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        var result = await _leaveService.UpdateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Date you select "));

    }

    [Test]
    public async Task UpdateLeaveRequest_WhenEndDateIsCurrentDate()
    {
        _dbContext.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "johngmail.com",
                PhoneNumber = "1111111111",
                DateOfBirth = DateTime.Now.AddYears(-22),
                HireDate = DateTime.Now,
                Salary = 50000,
                Position = "Developer",
                DepartmentId = 1
            }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
           new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var dto = new LeaveRequestDto
        {
            Id = 1,
            EmployeeId = 101,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now,
            Reason = "Medical Update",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        var result = await _leaveService.UpdateLeaveRequestAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Date you select "));

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
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        var result = await _leaveService.UpdateLeaveRequestAsync(dto);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("No Data Found"));
    }

    

    [Test]
    public async Task UpdateLeaveRequest_WhenLeaveTypeNotFound()
    {
        _dbContext.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@gmail.com",
                PhoneNumber = "1111111111",
                Position = "Developer",
                DateOfBirth = DateTime.Now.AddYears(-25),
                DepartmentId = 1,
                HireDate = DateTime.Now,
                Salary = 50000
            }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var dto = new LeaveRequestDto
        {
            Id = 1,
            EmployeeId = 101,
            LeaveTypeId = 0, // Non-existing LeaveTypeId
            StartDate = DateTime.Now.AddDays(2),
            EndDate = DateTime.Now.AddDays(4),
            Reason = "Medical Update",
            StatusId = Status.Pending,
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
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StatusId, Is.EqualTo(Status.Pending));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Reason, Is.EqualTo("Medical"));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(1).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(3).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.LeaveTypeId, Is.EqualTo(1));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EmployeeId, Is.EqualTo(101));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Id, Is.EqualTo(1));

        var result = await _leaveService.UpdateStatusLeaveAsync(1, Status.Approved, 99);
        Assert.That(result.IsSuccess, Is.True);

        var leaveRequest = await _dbContext.LeaveRequests.FindAsync(1);
        Assert.That(leaveRequest, Is.Not.Null);
        Assert.That(leaveRequest!.StatusId, Is.EqualTo(Status.Approved));
        Assert.That(leaveRequest.Reason, Is.EqualTo("Medical"));
        Assert.That(leaveRequest.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(1).Date));
        Assert.That(leaveRequest.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(3).Date));
        Assert.That(leaveRequest.LeaveTypeId, Is.EqualTo(1));
        Assert.That(leaveRequest.EmployeeId, Is.EqualTo(101));
        Assert.That(leaveRequest.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateLeaveRequestStatus_WhenAlreadyApproved()
    {
        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Approved, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var result = await _leaveService.UpdateStatusLeaveAsync(1, Status.Approved, 99);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to update leave status or status is not approved."));
    }

    [Test]
    public async Task UpdateLeaveRequestStatus_WhenAlreadyRejected()
    {
        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Rejected, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var result = await _leaveService.UpdateStatusLeaveAsync(1, Status.Approved, 99);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to update leave status or status is not approved."));
    }

    [Test]
    public async Task UpdateLeaveRequestStatus_WhenAlreadyCancled()
    {
        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Cancelled, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var result = await _leaveService.UpdateStatusLeaveAsync(1, Status.Approved, 99);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to update leave status or status is not approved."));
    }

    [Test]
    public async Task UpdateLeaveRequestStatus_WhenAlreadyDelete() 
    {
        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Rejected, IsActive = false, IsDeleted = true }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var result = await _leaveService.UpdateStatusLeaveAsync(1, Status.Approved, 99);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to update leave status or status is not approved."));
    }

    [Test]
    public async Task UpdateLeaveRequestStatus_WhenReject()
    {
        _dbContext.Employees.Add(new Employee
        {
            Id = 101,
            EmployeeNumber = "EMP101",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@a.com",
            PhoneNumber = "9999999999",
            Position = "Dev",
            Salary = 50000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        });

        await _dbContext.SaveChangesAsync();
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();


        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StatusId, Is.EqualTo(Status.Pending));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Reason, Is.EqualTo("Medical"));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(1).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(3).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.LeaveTypeId, Is.EqualTo(1));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EmployeeId, Is.EqualTo(101));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Id, Is.EqualTo(1));

        _dbContext.ChangeTracker.Clear();

        var result = await _leaveService.UpdateStatusLeaveAsync(1, Status.Rejected, 99);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StatusId, Is.EqualTo(Status.Rejected));
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
        _dbContext.Employees.Add(new Employee
        {
            Id = 101,
            EmployeeNumber = "EMP101",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@a.com",
            PhoneNumber = "9999999999",
            Position = "Dev",
            Salary = 50000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        });

        await _dbContext.SaveChangesAsync();
        _dbContext.AddRange(
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StatusId, Is.EqualTo(Status.Pending));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Reason, Is.EqualTo("Medical"));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StartDate.Date, Is.EqualTo(DateTime.Now.AddDays(1).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EndDate.Date, Is.EqualTo(DateTime.Now.AddDays(3).Date));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.LeaveTypeId, Is.EqualTo(1));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.EmployeeId, Is.EqualTo(101));
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.Id, Is.EqualTo(1));

        var result = await _leaveService.UpdateLeaveRequestStatusCancelAsync(1, 101);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That((await _dbContext.LeaveRequests.FindAsync(1))!.StatusId, Is.EqualTo(Status.Cancelled));
    }

    [Test]
    public async Task UpdateLeaveRequestStatusCancel_WhenIsZeroOrNegative()
    {
        var result = await _leaveService.UpdateLeaveRequestStatusCancelAsync(0, 101);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Id"));

        var result2 = await _leaveService.UpdateLeaveRequestStatusCancelAsync(-2, 101);
        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Is.EqualTo("Invalid Id"));
    }

    [Test]
    public async Task UpdateLeaveRequestStatusCancel_WhenEmployeeIdMismatch()
    {
        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var result = await _leaveService.UpdateLeaveRequestStatusCancelAsync(1, 0);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Id"));
    }

    [Test]
    public async Task UpdateLeaveRequestStatusCancel_WhenAlreadyCancelled()
    {
        _dbContext.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@gmail.com",
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
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Cancelled, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var result = await _leaveService.UpdateLeaveRequestStatusCancelAsync(1, 101);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Leave request is already cancelled."));
    }

    [Test]
    public async Task UpdateLeaveRequestStatusCancel_WhenAlreadyApproved()
    {
        _dbContext.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@gmail.com",
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
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Approved, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var result = await _leaveService.UpdateLeaveRequestStatusCancelAsync(1, 101);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Cannot cancel an approved leave request"));
    }

    [Test]
    public async Task UpdateLeaveRequestStatusCancel_WhenAlreadyRejected()
    {
        _dbContext.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@gmail.com",
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
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Rejected, IsActive = true, IsDeleted = false }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var result = await _leaveService.UpdateLeaveRequestStatusCancelAsync(1, 101);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Cannot cancel an Rejected leave request"));
    }

    [Test]
    public async Task UpdateLeaveRequestStatusCancel_WhenAlreadyDeleted()
    {
        _dbContext.Add(
            new Employee
            {
                Id = 101,
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@gmail.com",
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
            new LeaveType { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Sick Leave Description", MaxPerYear = 10, IsPaid = true }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.Add(
            new LeaveRequest { Id = 1, EmployeeId = 101, LeaveTypeId = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3), Reason = "Medical", StatusId = Status.Pending, IsActive = false, IsDeleted = true }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var result = await _leaveService.UpdateLeaveRequestStatusCancelAsync(1, 101);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Leave Request not found"));
    }
}