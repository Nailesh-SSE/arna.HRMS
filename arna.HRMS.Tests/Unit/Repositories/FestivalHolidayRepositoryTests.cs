using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Repositories;

[TestFixture]
public class FestivalHolidayRepositoryTests
{
    private ApplicationDbContext _dbContext = null!;
    private FestivalHolidayRepository _festivalHolidayRepository = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var baseRepository = new BaseRepository<FestivalHoliday>(_dbContext);
        _festivalHolidayRepository = new FestivalHolidayRepository(baseRepository);
    }

    [Test]
    public async Task GetFestivalHolidayAsync_WhenSuccess()
    {
        // Arrange
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday { Id = 1, FestivalName = "Holiday 1", Date = DateTime.Now, DayOfWeek = DateTime.Now.DayOfWeek.ToString(), IsActive = true, IsDeleted = false },
            new FestivalHoliday { Id = 2, FestivalName = "Holiday 2", Date = DateTime.Now, DayOfWeek = DateTime.Now.DayOfWeek.ToString(), IsActive = false, IsDeleted = false },
            new FestivalHoliday { Id = 3, FestivalName = "Holiday 3", Date = DateTime.Now, DayOfWeek = DateTime.Now.DayOfWeek.ToString(), IsActive = true, IsDeleted = true }
        );
        _dbContext.SaveChanges();
        // Act
        var result = await _festivalHolidayRepository.GetFestivalHolidayAsync();
        // Assert

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].FestivalName, Is.EqualTo("Holiday 1"));
        Assert.That(result[0].IsActive, Is.True);
        Assert.That(result[0].IsDeleted, Is.False);
    }

    [Test]
    public async Task GetFestivalHolidayAsync_WhenEmpty()
    {
        // Act
        var result = await _festivalHolidayRepository.GetFestivalHolidayAsync();
        // Assert
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetFestivalHolidayByIdAsync_WhenSuccess()
    {
        // Arrange
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday { Id = 1, FestivalName = "Holiday 1", Date = DateTime.Now, DayOfWeek = DateTime.Now.DayOfWeek.ToString(), IsActive = true, IsDeleted = false },
            new FestivalHoliday { Id = 2, FestivalName = "Holiday 2", Date = DateTime.Now, DayOfWeek = DateTime.Now.DayOfWeek.ToString(), IsActive = false, IsDeleted = false }
        );
        _dbContext.SaveChanges();
        // Act
        var result = await _festivalHolidayRepository.GetFestivalHolidayByIdAsync(1);
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.FestivalName, Is.EqualTo("Holiday 1"));
        Assert.That(result.IsActive, Is.True);
        Assert.That(result.IsDeleted, Is.False);
        Assert.That(result.Date.Date, Is.EqualTo(DateTime.Now.Date));
    }

    [Test]
    public async Task GetFestivalHolidayByIdAsync_WhenNotFound()
    {
        // Arrange
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday { Id = 1, FestivalName = "Holiday 1", Date = DateTime.Now, DayOfWeek = DateTime.Now.DayOfWeek.ToString(), IsActive = true, IsDeleted = false }
        );
        _dbContext.SaveChanges();
        // Act
        var result = await _festivalHolidayRepository.GetFestivalHolidayByIdAsync(99);
        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateFestivalHolidayAsync_WhenSuccess()
    {
        // Arrange
        var newHoliday = new FestivalHoliday
        {
            FestivalName = "New Holiday",
            Date = DateTime.Now,
            DayOfWeek = DateTime.Now.DayOfWeek.ToString(),
            IsActive = true,
            IsDeleted = false
        };
        // Act
        var createdHoliday = await _festivalHolidayRepository.CreateFestivalHolidayAsync(newHoliday);
        // Assert
        Assert.That(createdHoliday, Is.Not.Null);
        Assert.That(createdHoliday.Id, Is.GreaterThan(0));
        Assert.That(createdHoliday.FestivalName, Is.EqualTo("New Holiday"));
        Assert.That(createdHoliday.IsActive, Is.True);
        Assert.That(createdHoliday.IsDeleted, Is.False);
        Assert.That((await _dbContext.FestivalHoliday.FindAsync(createdHoliday.Id)) != null, Is.True);
        Assert.That((await _dbContext.FestivalHoliday.FindAsync(createdHoliday.Id))!.FestivalName, Is.EqualTo("New Holiday"));
    }

    [Test]
    public async Task CreateFestivalHolidayAsync_WhenNullInput()
    {
        // Arrange
        FestivalHoliday? newHoliday = null;
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _festivalHolidayRepository.CreateFestivalHolidayAsync(newHoliday!);
        });
    }

    [Test]
    public async Task UpdateFestivalHolidayAsync_WhenNotFound()
    {
        // Arrange
        var nonExistentHoliday = new FestivalHoliday
        {
            Id = 999,
            FestivalName = "Non Existent Holiday",
            Date = DateTime.Now,
            IsActive = true,
            IsDeleted = false
        };
        // Act & Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await _festivalHolidayRepository.UpdateFestivalHolidayAsync(nonExistentHoliday);
        });
    }

    [Test]
    public async Task UpdateFestivalHolidayAsync_WhenSuccess()
    {
        // Arrange
        var existingHoliday = new FestivalHoliday
        {
            FestivalName = "Existing Holiday",
            Date = DateTime.Now,
            DayOfWeek = DateTime.Now.DayOfWeek.ToString(),
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.FestivalHoliday.Add(existingHoliday);
        await _dbContext.SaveChangesAsync();

        //Assert pre-condition
        Assert.That(existingHoliday.Id, Is.GreaterThan(0));
        Assert.That(existingHoliday.FestivalName, Is.EqualTo("Existing Holiday"));
        Assert.That(existingHoliday.IsActive, Is.True);
        Assert.That(existingHoliday.IsDeleted, Is.False);
        Assert.That((await _dbContext.FestivalHoliday.FindAsync(existingHoliday.Id)) != null, Is.True);

        // Act
        existingHoliday.FestivalName = "Updated Holiday";
        var updatedHoliday = await _festivalHolidayRepository.UpdateFestivalHolidayAsync(existingHoliday);

        // Assert
        Assert.That(updatedHoliday, Is.Not.Null);
        Assert.That(updatedHoliday.Id, Is.EqualTo(existingHoliday.Id));
        Assert.That(updatedHoliday.FestivalName, Is.EqualTo("Updated Holiday"));
        Assert.That(updatedHoliday.IsActive, Is.True);
        Assert.That(updatedHoliday.IsDeleted, Is.False);

        var holidayInDb = await _dbContext.FestivalHoliday.FindAsync(existingHoliday.Id);
        Assert.That(holidayInDb, Is.Not.Null);
        Assert.That(holidayInDb!.FestivalName, Is.EqualTo("Updated Holiday"));
        Assert.That(holidayInDb.IsActive, Is.True);
        Assert.That(holidayInDb.IsDeleted, Is.False);
    }

    [Test]
    public async Task DeleteFestivalHolidayAsync_WhenSuccess()
    {
        // Arrange
        var existingHoliday = new FestivalHoliday
        {
            FestivalName = "Holiday to Delete",
            Date = DateTime.Now,
            DayOfWeek = DateTime.Now.DayOfWeek.ToString(),
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.FestivalHoliday.Add(existingHoliday);
        await _dbContext.SaveChangesAsync();

        //Assert pre-condition
        Assert.That(existingHoliday.Id, Is.GreaterThan(0));
        Assert.That(existingHoliday.IsActive, Is.True);
        Assert.That(existingHoliday.IsDeleted, Is.False);
        Assert.That((await _dbContext.FestivalHoliday.FindAsync(existingHoliday.Id)) != null, Is.True);

        // Act
        var deleteResult = await _festivalHolidayRepository.DeleteFestivalHolidayAsync(existingHoliday.Id);

        // Assert
        Assert.That(deleteResult, Is.True);
        var holidayInDb = await _dbContext.FestivalHoliday.FindAsync(existingHoliday.Id);
        Assert.That(holidayInDb, Is.Not.Null);
        Assert.That(holidayInDb!.IsActive, Is.False);
        Assert.That(holidayInDb.IsDeleted, Is.True);
    }

    [Test]
    public async Task DeleteFestivalHolidayAsync_WhenNotFound()
    {
        // Arrange
        var nonExistentId = 999;
        // Act
        var deleteResult = await _festivalHolidayRepository.DeleteFestivalHolidayAsync(nonExistentId);
        // Assert
        Assert.That(deleteResult, Is.False);
    }

    [Test]
    public async Task GetByNameAsync_WhenSuccess()
    {
        // Arrange
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday { Id = 1, FestivalName = "Holiday 1", Date = DateTime.Now, DayOfWeek = DateTime.Now.DayOfWeek.ToString(), IsActive = true, IsDeleted = false },
            new FestivalHoliday { Id = 2, FestivalName = "Holiday 2", Date = DateTime.Now, DayOfWeek = DateTime.Now.DayOfWeek.ToString(), IsActive = false, IsDeleted = false }
        );
        _dbContext.SaveChanges();
        // Act
        var result = await _festivalHolidayRepository.GetByNameAsync("Holiday 1");
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result[0]!.FestivalName, Is.EqualTo("Holiday 1"));
        Assert.That(result[0]!.IsActive, Is.True);
        Assert.That(result[0]!.IsDeleted, Is.False);
    }

    [Test]
    public async Task GetByNameAsync_WhenInactiveOrDeleted()
    {
        // Arrange
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday
            {
                Id = 1,
                FestivalName = "Holiday 1",
                Date = DateTime.Now,
                DayOfWeek = DateTime.Now.DayOfWeek.ToString(),
                IsActive = false,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 2,
                FestivalName = "Holiday 2",
                Date = DateTime.Now,
                DayOfWeek = DateTime.Now.DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = true
            }
        );
        _dbContext.SaveChanges();
        // Act
        var result1 = await _festivalHolidayRepository.GetByNameAsync("Holiday 1");
        var result2 = await _festivalHolidayRepository.GetByNameAsync("Holiday 2");
        // Assert
        Assert.That(result1, Is.Empty);
        Assert.That(result2, Is.Empty);
    }

    [Test]
    public async Task GetByNameAsync_WhenEmpty()
    {
        var resultEmpty = await _festivalHolidayRepository.GetByNameAsync(string.Empty);
        Assert.That(resultEmpty, Is.Empty);

        var resultWhitespace = await _festivalHolidayRepository.GetByNameAsync("   ");
        Assert.That(resultWhitespace, Is.Empty);

    }

    [Test]
    public async Task GetByNameAsync_WhenTrimed()
    {
        var holiday = new FestivalHoliday
        {
            FestivalName = "Trimmed Holiday",
            Date = DateTime.Now,
            DayOfWeek = DateTime.Now.DayOfWeek.ToString(),
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.FestivalHoliday.Add(holiday);
        await _dbContext.SaveChangesAsync();

        var result = await _festivalHolidayRepository.GetByNameAsync("  Trimmed Holiday  ");
        Assert.That(result, Is.Not.Null);
        Assert.That(result[0]!.FestivalName, Is.EqualTo("Trimmed Holiday"));

    }

    [Test]
    public async Task GetByNameAsync_WhenCaseInsensitive()
    {
        var holiday = new FestivalHoliday
        {
            FestivalName = "Case Insensitive Holiday",
            Date = DateTime.Now,
            DayOfWeek = DateTime.Now.DayOfWeek.ToString(),
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.FestivalHoliday.Add(holiday);
        await _dbContext.SaveChangesAsync();
        var result = await _festivalHolidayRepository.GetByNameAsync("case insensitive holiday");
        Assert.That(result, Is.Not.Null);
        Assert.That(result[0]!.FestivalName, Is.EqualTo("Case Insensitive Holiday"));
    }

    [Test]
    public async Task GetByNameAsync_WhenMultipleEntries()
    {
        // Arrange
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday { Id = 1, FestivalName = "Holiday 1", Date = DateTime.Now, DayOfWeek = DateTime.Now.DayOfWeek.ToString(), IsActive = true, IsDeleted = false },
            new FestivalHoliday { Id = 2, FestivalName = "Holiday 1", Date = DateTime.Now, DayOfWeek = DateTime.Now.DayOfWeek.ToString(), IsActive = true, IsDeleted = false }
        );
        _dbContext.SaveChanges();
        // Act
        var result = await _festivalHolidayRepository.GetByNameAsync("Holiday 1");
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result[0]!.FestivalName, Is.EqualTo("Holiday 1"));
        Assert.That(result[1]!.IsActive, Is.True);
        Assert.That(result[1]!.IsDeleted, Is.False);
    }

    [Test]
    public async Task GetByNameAsync_WhenNotFound()
    {
        // Arrange
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday { Id = 1, FestivalName = "Holiday 1", Date = DateTime.Now, DayOfWeek = DateTime.Now.DayOfWeek.ToString(), IsActive = true, IsDeleted = false }
        );
        _dbContext.SaveChanges();
        // Act
        var result = await _festivalHolidayRepository.GetByNameAsync("Non Existent Holiday");
        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetByMonthAsync_WhenAllInactiveOrDeleted()
    {
        // Arrange
        var targetYear = 2024;
        var targetMonth = 6;
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday
            {
                Id = 1,
                FestivalName = "Holiday June Inactive",
                Date = new DateTime(targetYear, targetMonth, 15),
                DayOfWeek = new DateTime(targetYear, targetMonth, 15).DayOfWeek.ToString(),
                IsActive = false,
                IsDeleted = false
            },
            new FestivalHoliday { Id = 2, FestivalName = "Holiday June Deleted", Date = new DateTime(targetYear, targetMonth, 20), DayOfWeek = new DateTime(targetYear, targetMonth, 15).DayOfWeek.ToString(), IsActive = true, IsDeleted = true }
        );
        _dbContext.SaveChanges();
        // Act
        var result = await _festivalHolidayRepository.GetByMonthAsync(targetYear, targetMonth);
        // Assert
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetByMonthAsync_WhenSuccess()
    {
        // Arrange
        var targetYear = 2024;
        var targetMonth = 6;
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday { Id = 1, FestivalName = "Holiday June", Date = new DateTime(targetYear, targetMonth, 15), DayOfWeek = new DateTime(targetYear, targetMonth, 15).DayOfWeek.ToString(), IsActive = true, IsDeleted = false },
            new FestivalHoliday { Id = 2, FestivalName = "Holiday May", Date = new DateTime(targetYear, 5, 10), DayOfWeek = new DateTime(targetYear, 5, 15).DayOfWeek.ToString(), IsActive = true, IsDeleted = false },
            new FestivalHoliday { Id = 3, FestivalName = "Holiday June Inactive", Date = new DateTime(targetYear, targetMonth, 20), DayOfWeek = new DateTime(targetYear, targetMonth, 15).DayOfWeek.ToString(), IsActive = false, IsDeleted = false }
        );
        _dbContext.SaveChanges();
        // Act
        var result = await _festivalHolidayRepository.GetByMonthAsync(targetYear, targetMonth);
        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].FestivalName, Is.EqualTo("Holiday June"));
        Assert.That(result[0].Date.Year, Is.EqualTo(targetYear));
        Assert.That(result[0].Date.Month, Is.EqualTo(targetMonth));
        Assert.That(result[0].IsActive, Is.True);
        Assert.That(result[0].IsDeleted, Is.False);
    }

    [Test]
    public async Task GetByMonthAsync_WhenNoHolidays()
    {
        // Arrange
        var targetYear = 2024;
        var targetMonth = 7;
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday
            {
                Id = 1,
                FestivalName = "Holiday June",
                Date = new DateTime(2024, 6, 15),
                DayOfWeek = new DateTime(2024, 6, 15).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            }
        );
        _dbContext.SaveChanges();
        // Act
        var result = await _festivalHolidayRepository.GetByMonthAsync(targetYear, targetMonth);
        // Assert
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetByMonthAsync_whenEmpty()
    {
        // Act
        var result = await _festivalHolidayRepository.GetByMonthAsync(2024, 6);
        // Assert
        Assert.That(result.Count, Is.EqualTo(0));
    }


}
