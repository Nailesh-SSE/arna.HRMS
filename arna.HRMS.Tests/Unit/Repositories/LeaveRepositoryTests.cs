using arna.HRMS.Core.Entities;
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

        var baseRepositoryLM = new BaseRepository<LeaveMaster>(_dbContext);
        var baseRepositoryLR = new BaseRepository<LeaveRequest>(_dbContext);
        var baseRepositoryELB = new BaseRepository<EmployeeLeaveBalance>(_dbContext);

        _leaveRepository = new LeaveRepository(baseRepositoryLM, baseRepositoryLR, baseRepositoryELB);
    }

    //leave master tests

    [Test]
    public async Task GetAllLeaveMastersAsync_ShouldReturnAllActiveLeaveMasters()
    {
        // Arrange
        var leaveMaster1 = new LeaveMaster
        {
            LeaveName = "Casual Leave",
            Description = "Casual leave",
            MaxPerYear = 7
        };
        var leaveMaster2 = new LeaveMaster
        {
            LeaveName = "Bereavement Leave",
            Description = "Bereavement leave",
            MaxPerYear = 5
        };
        await _leaveRepository.CreateLeaveMasterAsync(leaveMaster1);
        await _leaveRepository.CreateLeaveMasterAsync(leaveMaster2);
        // Act
        var leaveMasters = await _leaveRepository.GetLeaveMasterAsync();
        // Assert
        Assert.That(leaveMasters.Count, Is.EqualTo(2));
        Assert.That(leaveMasters.Any(lm => lm.LeaveName == "Casual Leave"), Is.True);
        Assert.That(leaveMasters.Any(lm => lm.LeaveName == "Bereavement Leave"), Is.True);
        Assert.That(leaveMasters.All(lm => lm.IsActive && !lm.IsDeleted), Is.True);
        Assert.That(leaveMasters.OrderByDescending(lm => lm.Id).SequenceEqual(leaveMasters), Is.True);
        Assert.That(leaveMasters[0].Id, Is.GreaterThan(leaveMasters[1].Id));
    }

    [Test]
    public async Task GetAllLeaveMastersAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Act
        var leaveMasters = await _leaveRepository.GetLeaveMasterAsync();
        // Assert
        Assert.That(leaveMasters, Is.Not.Null);
        Assert.That(leaveMasters, Is.Empty);
    }

    [Test]
    public async Task CreateLeaveMasterAsync_WhenNullInput()
    {
        LeaveMaster? newHoliday = null;
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _leaveRepository.CreateLeaveMasterAsync(newHoliday!);
        });
    }

    [Test]
    public async Task CreateLeaveMasterAsync_ShouldAddLeaveMaster()
    {
        // Arrange
        var leaveMaster = new LeaveMaster
        {
            LeaveName = "Annual Leave",
            Description = "Annual paid leave",
            MaxPerYear = 15
        };
        // Act
        var createdLeaveMaster = await _leaveRepository.CreateLeaveMasterAsync(leaveMaster);
        var fetchedLeaveMaster = await _leaveRepository.GetLeaveMasterByIdAsync(createdLeaveMaster.Id);
        // Assert
        Assert.That(fetchedLeaveMaster, Is.Not.Null);
        Assert.That(fetchedLeaveMaster!.LeaveName, Is.EqualTo("Annual Leave"));
        Assert.That(fetchedLeaveMaster!.Description, Is.EqualTo("Annual paid leave"));
        Assert.That(fetchedLeaveMaster.MaxPerYear, Is.EqualTo(15));
        Assert.That(fetchedLeaveMaster.IsActive, Is.True);
    }

    [Test]
    public async Task DeleteLeaveMasterAsync_ShouldMarkLeaveMasterAsDeleted()
    {
        // Arrange
        var leaveMaster = new LeaveMaster
        {
            LeaveName = "Sick Leave",
            Description = "Sick leave",
            MaxPerYear = 10
        };
        var createdLeaveMaster = await _leaveRepository.CreateLeaveMasterAsync(leaveMaster);
        // Act
        var deleteResult = await _leaveRepository.DeleteLeaveMasterAsync(createdLeaveMaster.Id);

        //Assert
        Assert.That(deleteResult, Is.True);
        Assert.That((await _leaveRepository.GetLeaveMasterByIdAsync(createdLeaveMaster.Id)), Is.Null);
    }

    [Test]
    public async Task DeleteLeaveMasterAsync_WhenNotFound()
    {
        var deleteResult = await _leaveRepository.DeleteLeaveMasterAsync(999);
        Assert.That(deleteResult, Is.False);
    }

    [Test]
    public async Task DeleteLeaveMasterAsync_WhenIdIsNullOrZero()
    {
        var deleteResult = await _leaveRepository.DeleteLeaveMasterAsync(0);

        Assert.That(deleteResult, Is.False);
        Assert.That(await _leaveRepository.GetLeaveMasterAsync(), Is.Empty);

        var deleteResultNegative = await _leaveRepository.DeleteLeaveMasterAsync(-5);

        Assert.That(deleteResultNegative, Is.False);
        Assert.That(await _leaveRepository.GetLeaveMasterAsync(), Is.Empty);

    }

    [Test]
    public async Task GetLeaveMasterByIdAsync_ShouldReturnLeaveMasterIfExists()
    {
        // Arrange
        var leaveMaster = new LeaveMaster
        {
            LeaveName = "Paternity Leave",
            Description = "Paternity leave",
            MaxPerYear = 14
        };
        var createdLeaveMaster = await _leaveRepository.CreateLeaveMasterAsync(leaveMaster);
        // Act
        var fetchedLeaveMaster = await _leaveRepository.GetLeaveMasterByIdAsync(createdLeaveMaster.Id);
        // Assert
        Assert.That(fetchedLeaveMaster, Is.Not.Null);
        Assert.That(fetchedLeaveMaster!.LeaveName, Is.EqualTo("Paternity Leave"));
        Assert.That(fetchedLeaveMaster!.Description, Is.EqualTo("Paternity leave"));
        Assert.That(fetchedLeaveMaster.MaxPerYear, Is.EqualTo(14));
    }

    [Test]
    public async Task GetLeaveMasterByIdAsync_ShouldReturnNullIfNotExistsOrNegativeOrZero()
    {
        // Act
        var fetchedLeaveMaster = await _leaveRepository.GetLeaveMasterByIdAsync(999);
        // Assert
        Assert.That(fetchedLeaveMaster, Is.Null);

        var fetchedLeaveMasterZero = await _leaveRepository.GetLeaveMasterByIdAsync(0);
        Assert.That(fetchedLeaveMasterZero, Is.Null);

        var fetchedLeaveMasterNegative = await _leaveRepository.GetLeaveMasterByIdAsync(-10);
        Assert.That(fetchedLeaveMasterNegative, Is.Null);
    }

    [Test]
    public async Task LeaveExistsAsync_ShouldReturnTrueIfLeaveExists()
    {
        // Arrange
        var leaveMaster = new LeaveMaster
        {
            LeaveName = "Maternity Leave",
            Description = "Maternity leave",
            MaxPerYear = 90
        };
        await _leaveRepository.CreateLeaveMasterAsync(leaveMaster);
        // Act
        var exists = await _leaveRepository.LeaveExistsAsync("Maternity Leave");
        // Assert
        Assert.That(exists[0].LeaveName, Is.EqualTo("Maternity Leave"));
    }

    [Test]
    public async Task LeaveExistsAsync_ShouldReturnFalseIfLeaveDoesNotExistOrInvalidName()
    {
        // Act
        var exists = await _leaveRepository.LeaveExistsAsync("NonExistent Leave");
        // Assert
        Assert.That(exists, Is.Empty);

        var existsEmpty = await _leaveRepository.LeaveExistsAsync(string.Empty);
        Assert.That(existsEmpty, Is.Empty);

        var existsWhitespace = await _leaveRepository.LeaveExistsAsync("   ");
        Assert.That(existsWhitespace, Is.Empty);

        var existsNull = await _leaveRepository.LeaveExistsAsync(null!);
        Assert.That(existsNull, Is.Empty);
    }

    [Test]
    public async Task LeaveExistsAsync_ShouldBeCaseInsensitiveAndTrimmed()
    {
        // Arrange
        var leaveMaster = new LeaveMaster
        {
            LeaveName = "Sabbatical Leave",
            Description = "Sabbatical leave",
            MaxPerYear = 180
        };
        await _leaveRepository.CreateLeaveMasterAsync(leaveMaster);
        // Act
        var existsDifferentCase = await _leaveRepository.LeaveExistsAsync("sabbatical leave");
        var existsWithWhitespace = await _leaveRepository.LeaveExistsAsync("  Sabbatical Leave  ");
        // Assert
        Assert.That(existsDifferentCase[0].LeaveName, Is.EqualTo("Sabbatical Leave"));
        Assert.That(existsWithWhitespace[0].LeaveName, Is.EqualTo("Sabbatical Leave"));
    }

    [Test]
    public async Task LeaveExistsAsync_WhenLeaveIsDeleted_ShouldReturnFalse()
    {
        // Arrange
        var leaveMaster = new LeaveMaster
        {
            LeaveName = "Temporary Leave",
            Description = "Temporary leave",
            MaxPerYear = 20
        };
        var createdLeaveMaster = await _leaveRepository.CreateLeaveMasterAsync(leaveMaster);
        await _leaveRepository.DeleteLeaveMasterAsync(createdLeaveMaster.Id);
        // Act
        var exists = await _leaveRepository.LeaveExistsAsync("Temporary Leave");
        // Assert
        Assert.That(exists, Is.Empty);
    }

    [Test]
    public async Task LeaveExistsAsync_WhenCaseInsencitiveCheck()
    {
        // Arrange
        var leaveMaster = new LeaveMaster
        {
            LeaveName = "Compassionate Leave",
            Description = "Compassionate leave",
            MaxPerYear = 10
        };
        await _leaveRepository.CreateLeaveMasterAsync(leaveMaster);
        // Act
        var exists = await _leaveRepository.LeaveExistsAsync("compassionate LEAVE");
        // Assert
        Assert.That(exists[0].LeaveName, Is.EqualTo("Compassionate Leave"));
    }

    [Test]
    public async Task UpdateLeaveMasterAsync_ShouldUpdateLeaveMasterDetails()
    {
        // Arrange
        var leaveMaster = new LeaveMaster
        {
            LeaveName = "Study Leave",
            Description = "Study leave",
            MaxPerYear = 30
        };
        var createdLeaveMaster = await _leaveRepository.CreateLeaveMasterAsync(leaveMaster);
        // Act
        createdLeaveMaster.Description = "Updated study leave description";
        createdLeaveMaster.MaxPerYear = 45;
        var updatedLeaveMaster = await _leaveRepository.UpdateLeaveMasterAsync(createdLeaveMaster);
        var fetchedLeaveMaster = await _leaveRepository.GetLeaveMasterByIdAsync(updatedLeaveMaster.Id);
        // Assert
        Assert.That(fetchedLeaveMaster, Is.Not.Null);
        Assert.That(fetchedLeaveMaster!.Description, Is.EqualTo("Updated study leave description"));
        Assert.That(fetchedLeaveMaster.MaxPerYear, Is.EqualTo(45));
    }

    [Test]
    public async Task UpdateLeaveMasterAsync_WhenLeaveMasterDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var leaveMaster = new LeaveMaster
        {
            Id = 999, // Non-existent ID
            LeaveName = "NonExistent Leave",
            Description = "This leave does not exist",
            MaxPerYear = 10
        };
        // Act & Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await _leaveRepository.UpdateLeaveMasterAsync(leaveMaster);
        });
    }


}
