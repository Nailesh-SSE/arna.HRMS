using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
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
        var leaveTypes = await _leaveRepository.GetLeaveTypeAsync();
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
        var leaveTypes = await _leaveRepository.GetLeaveTypeAsync();
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
        Assert.That(await _leaveRepository.GetLeaveTypeAsync(), Is.Empty);

        var deleteResultNegative = await _leaveRepository.DeleteLeaveTypeAsync(-5);

        Assert.That(deleteResultNegative, Is.False);
        Assert.That(await _leaveRepository.GetLeaveTypeAsync(), Is.Empty);

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
            LeaveNameId = LeaveName.PaternityLeave,
            Description = "Maternity leave",
            MaxPerYear = 90
        };
        await _leaveRepository.CreateLeaveTypeAsync(leaveType);
        // Act
        var exists = await _leaveRepository.LeaveExistsAsync(LeaveName.PaternityLeave);
        // Assert
        Assert.That(exists, Is.EqualTo(true));
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


}
