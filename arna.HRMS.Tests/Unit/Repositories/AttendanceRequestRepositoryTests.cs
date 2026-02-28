using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Repositories;

[TestFixture]
public class AttendanceRequestRepositoryTests
{
    private ApplicationDbContext _dbContext = null!;
    private AttendanceRequestRepository _repository = null!; 

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        var attendanceBaseRepo = new BaseRepository<AttendanceRequest>(_dbContext);

        _repository = new AttendanceRequestRepository(attendanceBaseRepo);
    }

    private Employee CreateValidEmployee(int id)
    {
        return new Employee
        {
            Id = id,
            EmployeeNumber = $"EMP{id}",
            FirstName = "Test",
            LastName = "User",
            Email = $"test{id}@mail.com",
            PhoneNumber = "9999999999",
            DateOfBirth = DateTime.Today.AddYears(-25),
            HireDate = DateTime.Today.AddYears(-2),
            DepartmentId = 1,
            Position = "Developer",
            Salary = 50000,
            IsActive = true,
            IsDeleted = false
        };
    }

    private AttendanceRequest CreateValidRequest(int id, int empId, Status status)
    {
        return new AttendanceRequest
        {
            Id = id,
            EmployeeId = empId,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Office,
            TotalHours = TimeSpan.FromHours(8),
            StatusId = status,
            IsActive = true,
            IsDeleted = false
        };
    }

    [Test]
    public async Task GetAttendanceRequests_WhenNoData_ReturnsEmpty()
    {
        var result = await _repository.GetAttendanceRequests(null, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetAttendanceRequests_ShouldFilterInactiveAndDeleted()
    {
        var emp = CreateValidEmployee(1);
        _dbContext.Employees.Add(emp);

        var active = CreateValidRequest(1, 1, Status.Pending);

        var inactive = CreateValidRequest(2, 1, Status.Pending);
        inactive.IsActive = false;

        var deleted = CreateValidRequest(3, 1, Status.Pending);
        deleted.IsDeleted = true;

        _dbContext.AttendanceRequest.AddRange(active, inactive, deleted);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAttendanceRequests(null, null);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.All(x => x.IsActive && !x.IsDeleted), Is.True);
    }

    [Test]
    public async Task GetAttendanceRequests_WhenEmployeeIdProvided_ReturnsOnlyThatEmployee()
    {
        var emp1 = CreateValidEmployee(1);
        var emp2 = CreateValidEmployee(2);

        _dbContext.Employees.AddRange(emp1, emp2);

        _dbContext.AttendanceRequest.AddRange(
            CreateValidRequest(1, 1, Status.Pending),
            CreateValidRequest(2, 2, Status.Pending)
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAttendanceRequests(1, null);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.All(x => x.EmployeeId == 1), Is.True);
    }

    [Test]
    public async Task GetAttendanceRequests_WhenStatusProvided_ReturnsMatchingStatus()
    {
        var emp = CreateValidEmployee(1);
        _dbContext.Employees.Add(emp);

        _dbContext.AttendanceRequest.AddRange(
            CreateValidRequest(1, 1, Status.Pending),
            CreateValidRequest(2, 1, Status.Approved)
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAttendanceRequests(null, Status.Approved);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.All(x => x.StatusId == Status.Approved), Is.True);
    }

    [Test]
    public async Task GetAttendanceRequests_WhenEmployeeAndStatusProvided_ReturnsFiltered()
    {
        var emp = CreateValidEmployee(1);
        _dbContext.Employees.Add(emp);

        _dbContext.AttendanceRequest.AddRange(
            CreateValidRequest(1, 1, Status.Pending),
            CreateValidRequest(2, 1, Status.Approved)
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAttendanceRequests(1, Status.Pending);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().StatusId, Is.EqualTo(Status.Pending));
    }

    [Test]
    public async Task GetAttendanceRequests_ShouldIncludeEmployee()
    {
        var emp = CreateValidEmployee(10);
        _dbContext.Employees.Add(emp);

        _dbContext.AttendanceRequest.Add(
            CreateValidRequest(1, 10, Status.Pending)
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAttendanceRequests(null, null);

        Assert.That(result.First().Employee, Is.Not.Null);
        Assert.That(result.First().Employee.Id, Is.EqualTo(10));
    }

    [Test]
    public async Task GetAttendanceRequests_ShouldOrderByIdDescending()
    {
        var emp = CreateValidEmployee(1);
        _dbContext.Employees.Add(emp);

        _dbContext.AttendanceRequest.AddRange(
            CreateValidRequest(1, 1, Status.Pending),
            CreateValidRequest(5, 1, Status.Pending),
            CreateValidRequest(3, 1, Status.Pending)
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAttendanceRequests(null, null);

        Assert.That(result[0].Id, Is.EqualTo(5));
        Assert.That(result[1].Id, Is.EqualTo(3));
        Assert.That(result[2].Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetPandingAttendanceRequestes_WhenNoData_ReturnsEmpty()
    {
        var result = await _repository.GetPendingAttendanceRequests();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));

        var result2 = await _repository.GetAttendanceRequestByIdAsync(-1);

        Assert.That(result2, Is.Null);
    }

    [Test]
    public async Task GetPandingAttendanceRequestes_ShouldReturnOnlyPending()
    {
        var emp = CreateValidEmployee(1);
        _dbContext.Employees.Add(emp);

        _dbContext.AttendanceRequest.AddRange(
            CreateValidRequest(1, 1, Status.Pending),
            CreateValidRequest(2, 1, Status.Approved),
            CreateValidRequest(3, 1, Status.Rejected)
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetPendingAttendanceRequests();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.All(x => x.StatusId == Status.Pending), Is.True);
    }

    [Test]
    public async Task GetPandingAttendanceRequestes_ShouldFilterInactiveAndDeleted()
    {
        var emp = CreateValidEmployee(2);
        _dbContext.Employees.Add(emp);

        var active = CreateValidRequest(1, 2, Status.Pending);

        var inactive = CreateValidRequest(2, 2, Status.Pending);
        inactive.IsActive = false;

        var deleted = CreateValidRequest(3, 2, Status.Pending);
        deleted.IsDeleted = true;

        _dbContext.AttendanceRequest.AddRange(active, inactive, deleted);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetPendingAttendanceRequests();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().IsActive, Is.True);
        Assert.That(result.First().IsDeleted, Is.False);
    }

    [Test]
    public async Task GetPandingAttendanceRequestes_ShouldIncludeEmployee()
    {
        var emp = CreateValidEmployee(3);
        _dbContext.Employees.Add(emp);

        _dbContext.AttendanceRequest.Add(
            CreateValidRequest(1, 3, Status.Pending)
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetPendingAttendanceRequests();

        Assert.That(result.First().Employee, Is.Not.Null);
        Assert.That(result.First().Employee.Id, Is.EqualTo(3));
    }

    [Test]
    public async Task GetPandingAttendanceRequestes_ShouldOrderByIdDescending()
    {
        var emp = CreateValidEmployee(4);
        _dbContext.Employees.Add(emp);

        _dbContext.AttendanceRequest.AddRange(
            CreateValidRequest(1, 4, Status.Pending),
            CreateValidRequest(5, 4, Status.Pending),
            CreateValidRequest(3, 4, Status.Pending)
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetPendingAttendanceRequests();

        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result[0].Id, Is.EqualTo(5));
        Assert.That(result[1].Id, Is.EqualTo(3));
        Assert.That(result[2].Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetPandingAttendanceRequestes_ShouldIgnoreNonPendingEvenIfActive()
    {
        var emp = CreateValidEmployee(5);
        _dbContext.Employees.Add(emp);

        _dbContext.AttendanceRequest.AddRange(
            CreateValidRequest(1, 5, Status.Approved),
            CreateValidRequest(2, 5, Status.Rejected)
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetPendingAttendanceRequests();

        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetPandingAttendanceRequestes_ShouldReturnPendingFromMultipleEmployees()
    {
        var emp1 = CreateValidEmployee(6);
        var emp2 = CreateValidEmployee(7);

        _dbContext.Employees.AddRange(emp1, emp2);

        _dbContext.AttendanceRequest.AddRange(
            CreateValidRequest(1, 6, Status.Pending),
            CreateValidRequest(2, 7, Status.Pending)
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetPendingAttendanceRequests();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(x => x.StatusId == Status.Pending), Is.True);
    }

    [Test]
    public async Task GetAttendanceRequestByIdAsync_WhenNoData_ReturnsNull()
    {
        var result = await _repository.GetAttendanceRequestByIdAsync(1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAttendanceRequestByIdAsync_WhenIdNotFound_ReturnsNull()
    {
        var emp = CreateValidEmployee(1);
        _dbContext.Employees.Add(emp);

        _dbContext.AttendanceRequest.Add(
            CreateValidRequest(1, 1, Status.Pending)
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAttendanceRequestByIdAsync(99);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAttendanceRequestByIdAsync_WhenInactive_ReturnsNull()
    {
        var emp = CreateValidEmployee(2);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(2, 2, Status.Pending);
        request.IsActive = false;

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAttendanceRequestByIdAsync(2);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAttendanceRequestByIdAsync_WhenDeleted_ReturnsNull()
    {
        var emp = CreateValidEmployee(3);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(3, 3, Status.Pending);
        request.IsDeleted = true;

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAttendanceRequestByIdAsync(3);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAttendanceRequestByIdAsync_WhenValid_ReturnsRequest()
    {
        var emp = CreateValidEmployee(4);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(10, 4, Status.Pending);

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAttendanceRequestByIdAsync(10);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(10));
        Assert.That(result.EmployeeId, Is.EqualTo(4));
        Assert.That(result.StatusId, Is.EqualTo(Status.Pending));
    }

    [Test]
    public async Task GetAttendanceRequestByIdAsync_ShouldIncludeEmployee()
    {
        var emp = CreateValidEmployee(5);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(20, 5, Status.Pending);

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAttendanceRequestByIdAsync(20);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Employee, Is.Not.Null);
        Assert.That(result.Employee.Id, Is.EqualTo(5));
        Assert.That(result.Employee.FirstName, Is.EqualTo("Test"));
    }

    [Test]
    public async Task GetAttendanceRequestByIdAsync_WhenMultipleRecords_ReturnsCorrectOne()
    {
        var emp = CreateValidEmployee(6);
        _dbContext.Employees.Add(emp);

        _dbContext.AttendanceRequest.AddRange(
            CreateValidRequest(1, 6, Status.Pending),
            CreateValidRequest(2, 6, Status.Pending),
            CreateValidRequest(3, 6, Status.Pending)
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAttendanceRequestByIdAsync(2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(2));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_WhenValid_CreatesSuccessfully()
    {
        var emp = CreateValidEmployee(1);
        _dbContext.Employees.Add(emp);
        await _dbContext.SaveChangesAsync();

        var request = CreateValidRequest(1, 1, Status.Pending);

        var result = await _repository.CreateAttendanceRequestAsync(request);
        await _dbContext.SaveChangesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EmployeeId, Is.EqualTo(1));
        Assert.That(_dbContext.AttendanceRequest.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_ShouldPersistAllFields()
    {
        var emp = CreateValidEmployee(2);
        _dbContext.Employees.Add(emp);
        await _dbContext.SaveChangesAsync();

        var request = new AttendanceRequest
        {
            EmployeeId = 2,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today.AddDays(1),
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Office,
            Description = "WFH",
            TotalHours = TimeSpan.FromHours(8),
            StatusId = Status.Pending,
            IsActive = true,
            IsDeleted = false
        };

        var result = await _repository.CreateAttendanceRequestAsync(request);
        await _dbContext.SaveChangesAsync();

        var saved = await _dbContext.AttendanceRequest.FirstAsync();

        Assert.That(saved.Description, Is.EqualTo("WFH"));
        Assert.That(saved.TotalHours, Is.EqualTo(TimeSpan.FromHours(8)));
        Assert.That(saved.StatusId, Is.EqualTo(Status.Pending));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_ShouldIncreaseCount()
    {
        var emp = CreateValidEmployee(3);
        _dbContext.Employees.Add(emp);
        await _dbContext.SaveChangesAsync();

        await _repository.CreateAttendanceRequestAsync(CreateValidRequest(1, 3, Status.Pending));
        await _repository.CreateAttendanceRequestAsync(CreateValidRequest(2, 3, Status.Pending));

        await _dbContext.SaveChangesAsync();

        Assert.That(_dbContext.AttendanceRequest.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task CreateAttendanceRequestAsync_ShouldMaintainEmployeeRelation()
    {
        var emp = CreateValidEmployee(4);
        _dbContext.Employees.Add(emp);
        await _dbContext.SaveChangesAsync();

        var request = CreateValidRequest(1, 4, Status.Pending);

        await _repository.CreateAttendanceRequestAsync(request);
        await _dbContext.SaveChangesAsync();

        var saved = await _dbContext.AttendanceRequest
            .Include(x => x.Employee)
            .FirstAsync();

        Assert.That(saved.Employee, Is.Not.Null);
        Assert.That(saved.Employee.Id, Is.EqualTo(4));
    }

    [Test]
    public void CreateAttendanceRequestAsync_WhenNullPassed_ThrowsException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _repository.CreateAttendanceRequestAsync(null!);
        });
    }

    [Test]
    public void CreateAttendanceRequestAsync_WhenRequiredFieldsMissing_ShouldThrow()
    {
        var invalid = new AttendanceRequest
        {
            EmployeeId = 1
            // Missing FromDate, ToDate, ReasonTypeId, LocationId, TotalHours
        };

        Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await _repository.CreateAttendanceRequestAsync(invalid);
            await _dbContext.SaveChangesAsync();
        });
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenValid_UpdatesSuccessfully()
    {
        var emp = CreateValidEmployee(1);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(1, 1, Status.Pending);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        request.Description = "Updated";
        request.StatusId = Status.Approved;

        var result = await _repository.UpdateAttendanceRequestAsync(request);
        await _dbContext.SaveChangesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Description, Is.EqualTo("Updated"));
        Assert.That(result.StatusId, Is.EqualTo(Status.Approved));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_ShouldPersistChanges()
    {
        var emp = CreateValidEmployee(2);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(2, 2, Status.Pending);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        request.TotalHours = TimeSpan.FromHours(6);

        await _repository.UpdateAttendanceRequestAsync(request);
        await _dbContext.SaveChangesAsync();

        var saved = await _dbContext.AttendanceRequest.FirstAsync();

        Assert.That(saved.TotalHours, Is.EqualTo(TimeSpan.FromHours(6)));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_ShouldUpdateMultipleFields()
    {
        var emp = CreateValidEmployee(3);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(3, 3, Status.Pending);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        request.Description = "WFH Updated";
        request.StatusId = Status.Rejected;
        request.TotalHours = TimeSpan.FromHours(4);

        await _repository.UpdateAttendanceRequestAsync(request);
        await _dbContext.SaveChangesAsync();

        var updated = await _dbContext.AttendanceRequest.FirstAsync();

        Assert.That(updated.Description, Is.EqualTo("WFH Updated"));
        Assert.That(updated.StatusId, Is.EqualTo(Status.Rejected));
        Assert.That(updated.TotalHours, Is.EqualTo(TimeSpan.FromHours(4)));
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_ShouldMaintainEmployeeRelation()
    {
        var emp = CreateValidEmployee(4);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(4, 4, Status.Pending);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        request.Description = "Changed";

        await _repository.UpdateAttendanceRequestAsync(request);
        await _dbContext.SaveChangesAsync();

        var saved = await _dbContext.AttendanceRequest
            .Include(x => x.Employee)
            .FirstAsync();

        Assert.That(saved.Employee, Is.Not.Null);
        Assert.That(saved.Employee.Id, Is.EqualTo(4));
    }

    [Test]
    public void UpdateAttendanceRequestAsync_WhenNullPassed_ShouldThrow()
    {
        Assert.ThrowsAsync<NullReferenceException>(async () =>
        {
            await _repository.UpdateAttendanceRequestAsync(null!);
        });
    }


    [Test]
    public void UpdateAttendanceRequestAsync_WhenEntityNotTracked_ShouldThrow()
    {
        var request = CreateValidRequest(999, 1, Status.Pending);

        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await _repository.UpdateAttendanceRequestAsync(request);
            await _dbContext.SaveChangesAsync();
        });
    }

    [Test]
    public async Task UpdateAttendanceRequestAsync_WhenRequiredFieldsMissing_ShouldUpdateWithoutException()
    {
        var emp = CreateValidEmployee(5);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(5, 5, Status.Pending);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        request.FromDate = default; // required field

        // Act
        await _repository.UpdateAttendanceRequestAsync(request);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updated = await _dbContext.AttendanceRequest
            .FirstOrDefaultAsync(x => x.Id == request.Id);

        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.FromDate, Is.EqualTo(default(DateTime)));
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenNotFound_ShouldReturnFalse()
    {
        var result = await _repository.DeleteAttendanceRequestAsync(999);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenInactive_ReturnsFalse()
    {
        var emp = CreateValidEmployee(1);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(1, 1, Status.Pending);
        request.IsActive = false;

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.DeleteAttendanceRequestAsync(1);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenDeleted_ReturnsFalse()
    {
        var emp = CreateValidEmployee(2);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(2, 2, Status.Pending);
        request.IsDeleted = true;

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.DeleteAttendanceRequestAsync(2);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenValid_SoftDeletes()
    {
        var emp = CreateValidEmployee(3);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(3, 3, Status.Pending);

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.DeleteAttendanceRequestAsync(3);
        await _dbContext.SaveChangesAsync();

        var deleted = await _dbContext.AttendanceRequest.FirstAsync();

        Assert.That(result, Is.True);
        Assert.That(deleted.IsActive, Is.False);
        Assert.That(deleted.IsDeleted, Is.True);
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_ShouldUpdateUpdatedOn()
    {
        var emp = CreateValidEmployee(4);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(4, 4, Status.Pending);

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var beforeDelete = DateTime.Now;

        var result = await _repository.DeleteAttendanceRequestAsync(4);
        await _dbContext.SaveChangesAsync();

        var deleted = await _dbContext.AttendanceRequest.FirstAsync();

        Assert.That(result, Is.True);
        Assert.That(deleted.UpdatedOn, Is.Not.Null);
        Assert.That(deleted.UpdatedOn, Is.GreaterThanOrEqualTo(beforeDelete));
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_ShouldNotAffectOtherRecords()
    {
        var emp = CreateValidEmployee(5);
        _dbContext.Employees.Add(emp);

        var request1 = CreateValidRequest(1, 5, Status.Pending);
        var request2 = CreateValidRequest(2, 5, Status.Pending);

        _dbContext.AttendanceRequest.AddRange(request1, request2);
        await _dbContext.SaveChangesAsync();

        await _repository.DeleteAttendanceRequestAsync(1);
        await _dbContext.SaveChangesAsync();

        var records = await _dbContext.AttendanceRequest
            .OrderBy(x => x.Id)
            .ToListAsync();

        Assert.That(records.Count, Is.EqualTo(2));
        Assert.That(records[0].IsDeleted, Is.True);
        Assert.That(records[1].IsDeleted, Is.False);
    }

    [Test]
    public async Task DeleteAttendanceRequestAsync_WhenCalledTwice_ReturnsFalseSecondTime()
    {
        var emp = CreateValidEmployee(6);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(6, 6, Status.Pending);

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var first = await _repository.DeleteAttendanceRequestAsync(6);
        await _dbContext.SaveChangesAsync();

        var second = await _repository.DeleteAttendanceRequestAsync(6);

        Assert.That(first, Is.True);
        Assert.That(second, Is.False);
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_WhenNotFound_ReturnsFalse()
    {
        var result = await _repository
            .UpdateAttendanceRequestStatusAsync(999, Status.Approved, 1);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_WhenValid_UpdatesStatus()
    {
        var emp = CreateValidEmployee(1);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(1, 1, Status.Pending);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        var result = await _repository
            .UpdateAttendanceRequestStatusAsync(1, Status.Approved, 99);

        Assert.That(result, Is.True);

        var updated = await _dbContext.AttendanceRequest.FirstAsync();

        Assert.That(updated.StatusId, Is.EqualTo(Status.Approved));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_ShouldUpdateApprovedBy()
    {
        var emp = CreateValidEmployee(2);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(2, 2, Status.Pending);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        await _repository.UpdateAttendanceRequestStatusAsync(2, Status.Approved, 50);

        var updated = await _dbContext.AttendanceRequest.FirstAsync();

        Assert.That(updated.ApprovedBy, Is.EqualTo(50));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_ShouldSetTimestamps()
    {
        var emp = CreateValidEmployee(3);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(3, 3, Status.Pending);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        var before = DateTime.Now;

        await _repository.UpdateAttendanceRequestStatusAsync(3, Status.Approved, 10);

        var updated = await _dbContext.AttendanceRequest.FirstAsync();

        Assert.That(updated.ApprovedOn, Is.Not.Null);
        Assert.That(updated.UpdatedOn, Is.Not.Null);
        Assert.That(updated.ApprovedOn, Is.GreaterThanOrEqualTo(before));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_ShouldPersistChanges()
    {
        var emp = CreateValidEmployee(4);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(4, 4, Status.Pending);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        await _repository.UpdateAttendanceRequestStatusAsync(4, Status.Rejected, 20);

        var saved = await _dbContext.AttendanceRequest.FirstAsync();

        Assert.That(saved.StatusId, Is.EqualTo(Status.Rejected));
        Assert.That(saved.ApprovedBy, Is.EqualTo(20));
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_WhenInactive_ReturnsFalse()
    {
        var emp = CreateValidEmployee(5);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(5, 5, Status.Pending);
        request.IsActive = false;

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _repository
            .UpdateAttendanceRequestStatusAsync(5, Status.Approved, 1);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusAsync_WhenDeleted_ReturnsFalse()
    {
        var emp = CreateValidEmployee(6);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(6, 6, Status.Pending);
        request.IsDeleted = true;

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _repository
            .UpdateAttendanceRequestStatusAsync(6, Status.Approved, 1);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetAttendanceRequestCancelAsync_WhenNotFound_ReturnsFalse()
    {
        var result = await _repository.CancelAttendanceRequestAsync(999, 1);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetAttendanceRequestCancelAsync_WhenWrongEmployee_ReturnsFalse()
    {
        var emp = CreateValidEmployee(1);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(1, 1, Status.Pending);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        var result = await _repository.CancelAttendanceRequestAsync(1, 99);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetAttendanceRequestCancelAsync_WhenInactive_ReturnsFalse()
    {
        var emp = CreateValidEmployee(2);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(2, 2, Status.Pending);
        request.IsActive = false;

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.CancelAttendanceRequestAsync(2, 2);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetAttendanceRequestCancelAsync_WhenDeleted_ReturnsFalse()
    {
        var emp = CreateValidEmployee(3);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(3, 3, Status.Pending);
        request.IsDeleted = true;

        _dbContext.AttendanceRequest.Add(request);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.CancelAttendanceRequestAsync(3, 3);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetAttendanceRequestCancelAsync_WhenStatusNotPending_ReturnsFalse()
    {
        var emp = CreateValidEmployee(4);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(4, 4, Status.Approved);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        var result = await _repository.CancelAttendanceRequestAsync(4, 4);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetAttendanceRequestCancelAsync_WhenValid_CancelsSuccessfully()
    {
        var emp = CreateValidEmployee(5);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(5, 5, Status.Pending);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        var result = await _repository.CancelAttendanceRequestAsync(5, 5);

        Assert.That(result, Is.True);

        var updated = await _dbContext.AttendanceRequest.FirstAsync();

        Assert.That(updated.StatusId, Is.EqualTo(Status.Cancelled));
    }

    [Test]
    public async Task GetAttendanceRequestCancelAsync_ShouldSetUpdatedOn()
    {
        var emp = CreateValidEmployee(6);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(6, 6, Status.Pending);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        var before = DateTime.Now;

        await _repository.CancelAttendanceRequestAsync(6, 6);

        var updated = await _dbContext.AttendanceRequest.FirstAsync();

        Assert.That(updated.UpdatedOn, Is.Not.Null);
        Assert.That(updated.UpdatedOn, Is.GreaterThanOrEqualTo(before));
    }

    [Test]
    public async Task GetAttendanceRequestCancelAsync_ShouldPersistChanges()
    {
        var emp = CreateValidEmployee(7);
        _dbContext.Employees.Add(emp);

        var request = CreateValidRequest(7, 7, Status.Pending);
        _dbContext.AttendanceRequest.Add(request);

        await _dbContext.SaveChangesAsync();

        await _repository.CancelAttendanceRequestAsync(7, 7);

        var saved = await _dbContext.AttendanceRequest.FirstAsync();

        Assert.That(saved.StatusId, Is.EqualTo(Status.Cancelled));
    }

}
