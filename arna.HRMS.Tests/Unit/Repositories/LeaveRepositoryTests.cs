using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Repositories;

[TestFixture]
public class LeaveRepositoryTests
{
    private ApplicationDbContext _dbContext = null!;
    private LeaveRepository _leaveRepository = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
           .UseInMemoryDatabase(Guid.NewGuid().ToString())
           .Options;

        _dbContext = new ApplicationDbContext(options);

        var baseRepositoryLM = new BaseRepository<LeaveType>(_dbContext);
        var baseRepositoryLR = new BaseRepository<LeaveRequest>(_dbContext);

        _leaveRepository = new LeaveRepository(baseRepositoryLM, baseRepositoryLR);
    }

    //leave Type tests

    [Test]
    public async Task GetAllLeaveTypesAsync_ShouldReturnAllActiveLeaveTypes()
    {
        // Arrange
        var leaveType1 = new LeaveType
        {
            LeaveNameId = LeaveName.CasualLeave,
            Description = "Casual leave",
            MaxPerYear = 7
        };
        var leaveType2 = new LeaveType
        {
            LeaveNameId = LeaveName.MaternityLeave,
            Description = "Bereavement leave",
            MaxPerYear = 5
        };
        await _leaveRepository.CreateLeaveTypeAsync(leaveType1);
        await _leaveRepository.CreateLeaveTypeAsync(leaveType2);
        // Act
        var leaveTypes = await _leaveRepository.GetLeaveTypesAsync();
        // Assert
        Assert.That(leaveTypes.Count, Is.EqualTo(2));
        Assert.That(leaveTypes.Any(lm => lm.LeaveNameId == LeaveName.CasualLeave), Is.True);
        Assert.That(leaveTypes.Any(lm => lm.LeaveNameId == LeaveName.MaternityLeave), Is.True);
        Assert.That(leaveTypes.All(lm => lm.IsActive && !lm.IsDeleted), Is.True);
        Assert.That(leaveTypes.OrderByDescending(lm => lm.Id).SequenceEqual(leaveTypes), Is.True);
        Assert.That(leaveTypes[0].Id, Is.GreaterThan(leaveTypes[1].Id));
    }

    [Test]
    public async Task GetAllLeaveTypesAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Act
        var leaveTypes = await _leaveRepository.GetLeaveTypesAsync();
        // Assert
        Assert.That(leaveTypes, Is.Not.Null);
        Assert.That(leaveTypes, Is.Empty);
    }

    [Test]
    public async Task CreateLeaveTypeAsync_WhenNullInput()
    {
        LeaveType? newHoliday = null;
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _leaveRepository.CreateLeaveTypeAsync(newHoliday!);
        });
    }

    [Test]
    public async Task CreateLeaveTypeAsync_ShouldAddLeaveType()
    {
        // Arrange
        var leaveType = new LeaveType
        {
            LeaveNameId = LeaveName.AnnualLeave,
            Description = "Annual paid leave",
            MaxPerYear = 15
        };
        // Act
        var createdLeaveType = await _leaveRepository.CreateLeaveTypeAsync(leaveType);
        var fetchedLeaveType = await _leaveRepository.GetLeaveTypeByIdAsync(createdLeaveType.Id);
        // Assert
        Assert.That(fetchedLeaveType, Is.Not.Null);
        Assert.That(fetchedLeaveType!.LeaveNameId, Is.EqualTo(LeaveName.AnnualLeave));
        Assert.That(fetchedLeaveType!.Description, Is.EqualTo("Annual paid leave"));
        Assert.That(fetchedLeaveType.MaxPerYear, Is.EqualTo(15));
        Assert.That(fetchedLeaveType.IsActive, Is.True);
    }

    [Test]
    public async Task DeleteLeaveTypeAsync_ShouldMarkLeaveTypeAsDeleted()
    {
        // Arrange
        var leaveType = new LeaveType
        {
            LeaveNameId = LeaveName.AnnualLeave,
            Description = "Sick leave",
            MaxPerYear = 10
        };
        var createdLeaveType = await _leaveRepository.CreateLeaveTypeAsync(leaveType);
        // Act
        var deleteResult = await _leaveRepository.DeleteLeaveTypeAsync(createdLeaveType.Id);

        //Assert
        Assert.That(deleteResult, Is.True);
        Assert.That((await _leaveRepository.GetLeaveTypeByIdAsync(createdLeaveType.Id)), Is.Null);
    }

    [Test]
    public async Task DeleteLeaveTypeAsync_WhenNotFound()
    {
        var deleteResult = await _leaveRepository.DeleteLeaveTypeAsync(999);
        Assert.That(deleteResult, Is.False);
    }

    [Test]
    public async Task DeleteLeaveTypeAsync_WhenIdIsNullOrZero()
    {
        var deleteResult = await _leaveRepository.DeleteLeaveTypeAsync(0);

        Assert.That(deleteResult, Is.False);
        Assert.That(await _leaveRepository.GetLeaveTypesAsync(), Is.Empty);

        var deleteResultNegative = await _leaveRepository.DeleteLeaveTypeAsync(-5);

        Assert.That(deleteResultNegative, Is.False);
        Assert.That(await _leaveRepository.GetLeaveTypesAsync(), Is.Empty);

    }

    [Test]
    public async Task GetLeaveTypeByIdAsync_ShouldReturnLeaveTypeIfExists()
    {
        // Arrange
        var leaveType = new LeaveType
        {
            LeaveNameId = LeaveName.PaternityLeave,
            Description = "Paternity leave",
            MaxPerYear = 14
        };
        var createdLeaveType = await _leaveRepository.CreateLeaveTypeAsync(leaveType);
        // Act
        var fetchedLeaveType = await _leaveRepository.GetLeaveTypeByIdAsync(createdLeaveType.Id);
        // Assert
        Assert.That(fetchedLeaveType, Is.Not.Null);
        Assert.That(fetchedLeaveType!.LeaveNameId, Is.EqualTo(LeaveName.PaternityLeave));
        Assert.That(fetchedLeaveType!.Description, Is.EqualTo("Paternity leave"));
        Assert.That(fetchedLeaveType.MaxPerYear, Is.EqualTo(14));
    }

    [Test]
    public async Task GetLeaveTypeByIdAsync_ShouldReturnNullIfNotExistsOrNegativeOrZero()
    {
        // Act
        var fetchedLeaveType = await _leaveRepository.GetLeaveTypeByIdAsync(999);
        // Assert
        Assert.That(fetchedLeaveType, Is.Null);

        var fetchedLeaveTypeZero = await _leaveRepository.GetLeaveTypeByIdAsync(0);
        Assert.That(fetchedLeaveTypeZero, Is.Null);

        var fetchedLeaveTypeNegative = await _leaveRepository.GetLeaveTypeByIdAsync(-10);
        Assert.That(fetchedLeaveTypeNegative, Is.Null);
    }

    [Test]
    public async Task LeaveExistsAsync_ShouldReturnTrueIfLeaveExists()
    {
        // Arrange
        var leaveType = new LeaveType
        {
            Id = 1,
            LeaveNameId = LeaveName.PaternityLeave,
            Description = "Maternity leave",
            MaxPerYear = 90
        };
        await _leaveRepository.CreateLeaveTypeAsync(leaveType);
        // Act
        var exists = await _leaveRepository.LeaveTypeExistsAsync(LeaveName.PaternityLeave,null);
        // Assert
        Assert.That(exists, Is.EqualTo(true));
    }

    [Test]
    public async Task LeaveExistsAsync_ShouldReturnFalseIfLeaveDoesNotExist()
    {
        // Act
        var exists = await _leaveRepository.LeaveTypeExistsAsync(LeaveName.MaternityLeave, 1);
        // Assert
        Assert.That(exists, Is.EqualTo(false));
    }

    [Test]
    public async Task UpdateLeaveTypeAsync_ShouldUpdateLeaveTypeDetails()
    {
        // Arrange
        var leaveType = new LeaveType
        {
            LeaveNameId = LeaveName.SickLeave,
            Description = "Sick leave",
            MaxPerYear = 30
        };
        var createdLeaveType = await _leaveRepository.CreateLeaveTypeAsync(leaveType);
        // Act
        createdLeaveType.Description = "Updated study leave description";
        createdLeaveType.MaxPerYear = 45;
        var updatedLeaveType = await _leaveRepository.UpdateLeaveTypeAsync(createdLeaveType);
        var fetchedLeaveType = await _leaveRepository.GetLeaveTypeByIdAsync(updatedLeaveType.Id);
        // Assert
        Assert.That(fetchedLeaveType, Is.Not.Null);
        Assert.That(fetchedLeaveType!.Description, Is.EqualTo("Updated study leave description"));
        Assert.That(fetchedLeaveType.MaxPerYear, Is.EqualTo(45));
    }

    [Test]
    public async Task UpdateLeaveTypeAsync_WhenLeaveTypeDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var leaveType = new LeaveType
        {
            Id = 999, // Non-existent ID
            LeaveNameId = LeaveName.SickLeave,
            Description = "This leave does not exist",
            MaxPerYear = 10
        };
        // Act & Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await _leaveRepository.UpdateLeaveTypeAsync(leaveType);
        });
    }


    // Leave Request

    [Test]
    public async Task CreateLeaveRequest_whenFound()
    {
        var leaveRequest = new LeaveRequest
        {
            EmployeeId = 1,
            LeaveTypeId = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(5),
            LeaveDays = 5,
            Reason = "Vacation",
            StatusId = Status.Pending
        };

        var createdLeaveRequest = await _leaveRepository.CreateLeaveRequestAsync(leaveRequest);

        Assert.That(createdLeaveRequest, Is.Not.Null);
        Assert.That(createdLeaveRequest.Id, Is.GreaterThan(0));
        Assert.That(createdLeaveRequest.EmployeeId, Is.EqualTo(1));
        Assert.That(createdLeaveRequest.LeaveTypeId, Is.EqualTo(1));
        Assert.That(createdLeaveRequest.StartDate, Is.EqualTo(leaveRequest.StartDate));
        Assert.That(createdLeaveRequest.EndDate, Is.EqualTo(leaveRequest.EndDate));
        Assert.That(createdLeaveRequest.LeaveDays, Is.EqualTo(5));
        Assert.That(createdLeaveRequest.Reason, Is.EqualTo("Vacation"));
        Assert.That(createdLeaveRequest.StatusId, Is.EqualTo(Status.Pending));
        Assert.That(createdLeaveRequest.IsActive, Is.True);
        Assert.That(createdLeaveRequest.IsDeleted, Is.False);
        Assert.That(createdLeaveRequest.CreatedOn, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public async Task CreateLeaveRequest_whenNullInput()
    {
        LeaveRequest? newLeaveRequest = null;
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _leaveRepository.CreateLeaveRequestAsync(newLeaveRequest!);
        });
    }

    [Test]
    public async Task GetLeaveRequestByIdAsync_WhenFound()
    {
        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@gmail.com",
            EmployeeNumber = "EMP001",
            DepartmentId = 1,
            Position = "Software Engineer",
            IsActive = true,
            IsDeleted = false,
            PhoneNumber = "1234567892"
        });
        await _dbContext.SaveChangesAsync();

        _dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = 1,
            LeaveNameId = LeaveName.CasualLeave,
            Description = "Casual leave",
            MaxPerYear = 7,
            IsActive = true,
            IsDeleted = false,
        }); 
        await _dbContext.SaveChangesAsync();

        _dbContext.LeaveRequests.Add(new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(5),
            LeaveDays = 5,
            Reason = "Vacation",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        });
        await _dbContext.SaveChangesAsync();
        var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(1);
        Assert.That(leaveRequest, Is.Not.Null); 
        Assert.That(leaveRequest!.Id, Is.EqualTo(1));
        Assert.That(leaveRequest.EmployeeId, Is.EqualTo(1));
        Assert.That(leaveRequest.LeaveTypeId, Is.EqualTo(1));
        Assert.That(leaveRequest.StartDate, Is.EqualTo(_dbContext.LeaveRequests.First().StartDate));
    }

    [Test]
    public async Task GetLeaveRequestByIdAsync_WhenNotFound()
    {
        var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(999);
        Assert.That(leaveRequest, Is.Null);
    }

    [Test]
    public async Task GetLeaveRequestByIdAsync_WhenIdIsNegativeOrZero()
    {
        var leaveRequestZero = await _leaveRepository.GetLeaveRequestByIdAsync(0);
        Assert.That(leaveRequestZero, Is.Null);
        var leaveRequestNegative = await _leaveRepository.GetLeaveRequestByIdAsync(-10);
        Assert.That(leaveRequestNegative, Is.Null);
    }

    [Test]
    public async Task GetLeaveRequestsByEmployeeIdAsync_WhenFound()
    {
        _dbContext.Employees.AddRange(
            new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@gmail.com",
                EmployeeNumber = "EMP001",
                DepartmentId = 1,
                Position = "Software Engineer",
                IsActive = true,
                IsDeleted = false,
                PhoneNumber = "1234567892"
            },
            new Employee
            {
                Id = 2,
                EmployeeNumber= "EMP002",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@gmail.com",
                PhoneNumber = "9876543210",
                DepartmentId = 2,
                Position = "Project Manager",
                IsActive = true,
                IsDeleted = false,
            }
        );
        await _dbContext.SaveChangesAsync();

        _dbContext.LeaveTypes.AddRange(
            new LeaveType
            {
                Id = 1,
                LeaveNameId = LeaveName.CasualLeave,
                Description = "Casual leave",
                MaxPerYear = 7,
                IsActive = true,
                IsDeleted = false,
            },
            new LeaveType
            {
                Id = 2,
                LeaveNameId = LeaveName.SickLeave,
                Description = "Sick leave",
                MaxPerYear = 10,
                IsActive = true,
                IsDeleted = false,
            }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.LeaveRequests.AddRange(
            new LeaveRequest
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5),
                LeaveDays = 5,
                Reason = "Vacation",
                StatusId = Status.Pending,
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.Now
            },
            new LeaveRequest
            {
                Id = 2,
                EmployeeId = 1,
                LeaveTypeId = 2,
                StartDate = DateTime.Now.AddDays(10),
                EndDate = DateTime.Now.AddDays(15),
                LeaveDays = 5,
                Reason = "Medical",
                StatusId = Status.Approved,
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.Now
            },
            new LeaveRequest
            {
                Id = 3,
                EmployeeId = 2,
                LeaveTypeId = 1,
                StartDate = DateTime.Now.AddDays(20),
                EndDate = DateTime.Now.AddDays(25),
                LeaveDays = 5,
                Reason = "Family event",
                StatusId = Status.Pending,
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.Now
            }
        );
        _dbContext.SaveChanges();

        var leaveRequests = await _leaveRepository.GetLeaveRequestByEmployeeIdAsync(1);
        Assert.That(leaveRequests, Is.Not.Null);
        Assert.That(leaveRequests.Count, Is.EqualTo(2));
        Assert.That(leaveRequests.All(lr => lr.EmployeeId == 1), Is.True);
        Assert.That(leaveRequests.All(lr => lr.IsActive && !lr.IsDeleted), Is.True);
        Assert.That(leaveRequests.OrderByDescending(lr => lr.Id).SequenceEqual(leaveRequests), Is.True);
        Assert.That(leaveRequests[0].Id, Is.GreaterThan(leaveRequests[1].Id));
        Assert.That(leaveRequests[0].Employee, Is.Not.Null);
        Assert.That(leaveRequests[0].Employee!.FirstName, Is.EqualTo("John"));
        Assert.That(leaveRequests[0].Employee!.LastName, Is.EqualTo("Doe"));
        Assert.That(leaveRequests[0].LeaveType, Is.Not.Null);
        Assert.That(leaveRequests[0].LeaveType!.LeaveNameId, Is.EqualTo(LeaveName.SickLeave));
        Assert.That(leaveRequests[0].LeaveType!.Description, Is.EqualTo("Sick leave"));
        Assert.That(leaveRequests[0].LeaveType!.MaxPerYear, Is.EqualTo(10));
        Assert.That(leaveRequests[1].Employee, Is.Not.Null);
        Assert.That(leaveRequests[1].Employee!.FirstName, Is.EqualTo("John"));
        Assert.That(leaveRequests[1].Employee!.LastName, Is.EqualTo("Doe"));
        Assert.That(leaveRequests[1].LeaveType, Is.Not.Null);
        Assert.That(leaveRequests[1].LeaveType!.LeaveNameId, Is.EqualTo(LeaveName.CasualLeave));
        Assert.That(leaveRequests[1].LeaveType!.Description, Is.EqualTo("Casual leave"));
        Assert.That(leaveRequests[1].LeaveType!.MaxPerYear, Is.EqualTo(7));
    }

    [Test]
    public async Task GetLeaveRequestsByEmployeeIdAsync_WhenNotFound()
    {
        var leaveRequests = await _leaveRepository.GetLeaveRequestByEmployeeIdAsync(999);
        Assert.That(leaveRequests, Is.Not.Null);
        Assert.That(leaveRequests, Is.Empty);
    }

    [Test]
    public async Task GetLeaveRequestsByEmployeeIdAsync_WhenEmployeeIdIsNegativeOrZero()
    {
        var leaveRequestsZero = await _leaveRepository.GetLeaveRequestByEmployeeIdAsync(0);
        Assert.That(leaveRequestsZero, Is.Not.Null);
        Assert.That(leaveRequestsZero, Is.Empty);
        var leaveRequestsNegative = await _leaveRepository.GetLeaveRequestByEmployeeIdAsync(-10);
        Assert.That(leaveRequestsNegative, Is.Not.Null);
        Assert.That(leaveRequestsNegative, Is.Empty);
    }

    [Test]
    public async Task GetLeaveRequestAsync_ShouldReturnAllActiveLeaveRequests()
    {
        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@gmail.com",
            EmployeeNumber = "EMP001",
            DepartmentId = 1,
            Position = "Software Engineer",
            IsActive = true,
            IsDeleted = false,
            PhoneNumber = "1234567892"
        });
        await _dbContext.SaveChangesAsync();

        _dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = 1,
            LeaveNameId = LeaveName.CasualLeave,
            Description = "Casual leave",
            MaxPerYear = 7,
            IsActive = true,
            IsDeleted = false,
        });

        await _dbContext.SaveChangesAsync();

        _dbContext.LeaveRequests.AddRange(
            new LeaveRequest
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5),
                LeaveDays = 5,
                Reason = "Vacation",
                StatusId = Status.Pending,
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.Now
            },
            new LeaveRequest
            {
                Id = 2,
                EmployeeId = 1,
                LeaveTypeId = 1,
                StartDate = DateTime.Now.AddDays(10),
                EndDate = DateTime.Now.AddDays(15),
                LeaveDays = 5,
                Reason = "Medical",
                StatusId = Status.Approved,
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.Now
            }
        );
        await _dbContext.SaveChangesAsync();
        var leaveRequests = await _leaveRepository.GetLeaveRequestsAsync();
        Assert.That(leaveRequests, Is.Not.Null);
        Assert.That(leaveRequests.Count, Is.EqualTo(2));
        Assert.That(leaveRequests.All(lr => lr.IsActive && !lr.IsDeleted), Is.True);
        Assert.That(leaveRequests.OrderByDescending(lr => lr.Id).SequenceEqual(leaveRequests), Is.True);
        Assert.That(leaveRequests[0].Id, Is.GreaterThan(leaveRequests[1].Id));
        Assert.That(leaveRequests[0].Employee, Is.Not.Null);
        Assert.That(leaveRequests[0].Employee!.FirstName, Is.EqualTo("John"));
        Assert.That(leaveRequests[0].Employee!.LastName, Is.EqualTo("Doe"));
        Assert.That(leaveRequests[0].LeaveType, Is.Not.Null);
        Assert.That(leaveRequests[0].LeaveType!.LeaveNameId, Is.EqualTo(LeaveName.CasualLeave));
        Assert.That(leaveRequests[0].LeaveType!.Description, Is.EqualTo("Casual leave"));
        Assert.That(leaveRequests[0].LeaveType!.MaxPerYear, Is.EqualTo(7));
        Assert.That(leaveRequests[1].Employee, Is.Not.Null);
        Assert.That(leaveRequests[1].Employee!.FirstName, Is.EqualTo("John"));
        Assert.That(leaveRequests[1].Employee!.LastName, Is.EqualTo("Doe"));
        Assert.That(leaveRequests[1].LeaveType, Is.Not.Null);
        Assert.That(leaveRequests[1].LeaveType!.LeaveNameId, Is.EqualTo(LeaveName.CasualLeave));
        Assert.That(leaveRequests[1].LeaveType!.Description, Is.EqualTo("Casual leave"));
        Assert.That(leaveRequests[1].LeaveType!.MaxPerYear, Is.EqualTo(7));
        Assert.That(leaveRequests.All(lr => lr.Employee != null), Is.True);
        Assert.That(leaveRequests.All(lr => lr.LeaveType != null), Is.True);
        Assert.That(leaveRequests.All(lr => lr.Employee!.FirstName == "John" && lr.Employee!.LastName == "Doe"), Is.True);
        Assert.That(leaveRequests.All(lr => lr.LeaveType!.LeaveNameId == LeaveName.CasualLeave && lr.LeaveType!.Description == "Casual leave" && lr.LeaveType!.MaxPerYear == 7), Is.True);
        Assert.That(leaveRequests.All(lr => lr.StartDate != default(DateTime) && lr.EndDate != default(DateTime)), Is.True);
        Assert.That(leaveRequests.All(lr => lr.LeaveDays == 5), Is.True);
        Assert.That(leaveRequests.All(lr => lr.Reason == "Vacation" || lr.Reason == "Medical"), Is.True);
        Assert.That(leaveRequests.All(lr => lr.StatusId == Status.Pending || lr.StatusId == Status.Approved), Is.True);
        Assert.That(leaveRequests.All(lr => lr.CreatedOn != default(DateTime)), Is.True);
    }

    [Test]
    public async Task GetLeaveRequestAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        var leaveRequests = await _leaveRepository.GetLeaveRequestsAsync();
        Assert.That(leaveRequests, Is.Not.Null);
        Assert.That(leaveRequests, Is.Empty);
    }

    [Test]
    public async Task UpdateLeaveRequestAsync_ShouldUpdateLeaveRequestDetails()
    {
        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@gmail.com",
            PhoneNumber= "1234564567",
            EmployeeNumber = "EMP001",
            DepartmentId = 1,
            Position = "Software Engineer",
            IsActive = true,
            IsDeleted = false,
        });
        await _dbContext.SaveChangesAsync();
        _dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = 1,
            LeaveNameId = LeaveName.CasualLeave,
            Description = "Casual leave",
            MaxPerYear = 7,
            IsActive = true,
            IsDeleted = false,
        });

        await _dbContext.SaveChangesAsync();

        _dbContext.LeaveRequests.Add(new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(5),
            LeaveDays = 5,
            Reason = "Vacation",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        });
        await _dbContext.SaveChangesAsync();
        
        Assert.That((await _leaveRepository.GetLeaveRequestByIdAsync(1))!.Reason, Is.EqualTo("Vacation"));
        Assert.That((await _leaveRepository.GetLeaveRequestByIdAsync(1))!.StatusId, Is.EqualTo(Status.Pending));
        Assert.That((await _leaveRepository.GetLeaveRequestByIdAsync(1))!.StartDate.Date, Is.EqualTo(_dbContext.LeaveRequests.First().StartDate.Date));
        Assert.That((await _leaveRepository.GetLeaveRequestByIdAsync(1))!.EndDate.Date, Is.EqualTo(_dbContext.LeaveRequests.First().EndDate.Date));
        Assert.That((await _leaveRepository.GetLeaveRequestByIdAsync(1))!.LeaveDays, Is.EqualTo(5));
        Assert.That((await _leaveRepository.GetLeaveRequestByIdAsync(1))!.EmployeeId, Is.EqualTo(1));
        Assert.That((await _leaveRepository.GetLeaveRequestByIdAsync(1))!.LeaveTypeId, Is.EqualTo(1));

        _dbContext.ChangeTracker.Clear();

        var dto = new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(6),
            LeaveDays = 5,
            Reason = "Updated Vacation",
            StatusId = Status.Approved,
            IsActive = true,
            IsDeleted = false,
        };
        var leaveRequestToUpdate = await _leaveRepository.UpdateLeaveRequestAsync(dto);

        Assert.That(leaveRequestToUpdate, Is.Not.Null);
        Assert.That(leaveRequestToUpdate!.Id, Is.EqualTo(dto.Id));
        Assert.That(leaveRequestToUpdate.EmployeeId, Is.EqualTo(dto.EmployeeId));
        Assert.That(leaveRequestToUpdate.LeaveTypeId, Is.EqualTo(dto.LeaveTypeId));
        Assert.That(leaveRequestToUpdate.StartDate.Date, Is.EqualTo(dto.StartDate.Date));
        Assert.That(leaveRequestToUpdate.EndDate.Date, Is.EqualTo(dto.EndDate.Date));
        Assert.That(leaveRequestToUpdate.LeaveDays, Is.EqualTo(dto.LeaveDays));
        Assert.That(leaveRequestToUpdate.Reason, Is.EqualTo(dto.Reason));
        Assert.That(leaveRequestToUpdate.StatusId, Is.EqualTo(dto.StatusId));
    }

    [Test]
    public async Task UpdateLeaveRequestAsync_WhenLeaveRequestDoesNotExist_ShouldThrowException()
    {
        var dto = new LeaveRequest
        {
            Id = 999, // Non-existent ID
            EmployeeId = 1,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(6),
            LeaveDays = 5,
            Reason = "Updated Vacation",
            StatusId = Status.Approved,
            IsActive = true,
            IsDeleted = false,
        };
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await _leaveRepository.UpdateLeaveRequestAsync(dto);
        });
    }

    [Test]
    public async Task UpdateLeaveRequestAsync_WhenIdIsNullOrZero_ShouldThrowException()
    {
        var dtoZero = new LeaveRequest
        {
            Id = 0, // Invalid ID
            EmployeeId = 1,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(6),
            LeaveDays = 5,
            Reason = "Updated Vacation",
            StatusId = Status.Approved,
            IsActive = true,
            IsDeleted = false,
        };
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await _leaveRepository.UpdateLeaveRequestAsync(dtoZero);
        });
        var dtoNegative = new LeaveRequest
        {
            Id = -10, // Invalid ID
            EmployeeId = 1,
            LeaveTypeId = 1,
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(6),
            LeaveDays = 5,
            Reason = "Updated Vacation",
            StatusId = Status.Approved,
            IsActive = true,
            IsDeleted = false,
        };
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await _leaveRepository.UpdateLeaveRequestAsync(dtoNegative);
        });
    }

    [Test]
    public async Task DeleteLeaveRequestAsync_ShouldMarkLeaveRequestAsDeleted()
    {
        // ---------- Arrange ----------
        var baseDate = new DateTime(2026, 02, 09);

        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@gmail.com",
            EmployeeNumber = "EMP001",
            PhoneNumber = "1234567894",
            DepartmentId = 1,
            Position = "Software Engineer",
            IsActive = true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        _dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = 1,
            LeaveNameId = LeaveName.CasualLeave,
            Description = "Casual leave",
            MaxPerYear = 7,
            IsActive = true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        var leaveRequest = new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            StartDate = baseDate,
            EndDate = baseDate.AddDays(5),
            LeaveDays = 5,
            Reason = "Vacation",
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = baseDate
        };

        _dbContext.LeaveRequests.Add(leaveRequest); 
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // ---------- Act ----------
        var result = await _leaveRepository.DeleteLeaveRequestAsync(1);

        // ---------- Assert ----------
        Assert.That(result, Is.True);

        var deletedLeaveRequest =
            await _leaveRepository.GetLeaveRequestByIdAsync(1);

        Assert.That(deletedLeaveRequest, Is.Null); 
    }

    [Test]
    public async Task DeleteLeaveRequestAsync_WhenNotFound()
    {
        var result = await _leaveRepository.DeleteLeaveRequestAsync(999);
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteLeaveRequestAsync_WhenIdIsZeroOrNegative()
    {
        var resultZero = await _leaveRepository.DeleteLeaveRequestAsync(0);
        Assert.That(resultZero, Is.False);

        var resultNegative = await _leaveRepository.DeleteLeaveRequestAsync(-10);
        Assert.That(resultNegative, Is.False);
    }
}
