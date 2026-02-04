using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Mapping;
using arna.HRMS.Infrastructure.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Services;

[TestFixture]
public class FestivalHolidayServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private FestivalHolidayService _festivalHolidayService = null!;
    private IMapper _mapper = null!;

    // ---------------- SETUP ----------------
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // ---------- Mapper ----------
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<FestivalHolidayProfile>();
        });

        _mapper = mapperConfig.CreateMapper();

        // ---------- Service ----------
        _festivalHolidayService = new FestivalHolidayService(
            new Infrastructure.Repositories.FestivalHolidayRepository(
                new Infrastructure.Repositories.Common.BaseRepository<arna.HRMS.Core.Entities.FestivalHoliday>(_dbContext)
            ),
            _mapper
        );
    }

    [Test]
    public async Task GetAllFestivalHoliday()
    {
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday
            {
                Id = 1,
                FestivalName = "Test1 Holiday",
                Description = "Test Description",
                Date = new DateTime(2024, 12, 25),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 2,
                FestivalName = "Test2 Holiday",
                Description = "Test2 Description",
                Date = new DateTime(2024, 1, 1),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 3,
                FestivalName = "Test3 Holiday",
                Description = "Test3 Description",
                Date = new DateTime(2024, 11, 28),
                IsActive = true,
                IsDeleted = false
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _festivalHolidayService.GetFestivalHolidayAsync();
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(3));
        Assert.That(result.Data![0].FestivalName, Is.EqualTo("Test3 Holiday"));
        Assert.That(result.Data![1].FestivalName, Is.EqualTo("Test2 Holiday"));
        Assert.That(result.Data![2].FestivalName, Is.EqualTo("Test1 Holiday"));
        Assert.That(result.Data![2].Date, Is.EqualTo(new DateTime(2024, 12, 25)));
        Assert.That(result.Data![1].Date, Is.EqualTo(new DateTime(2024, 1, 1)));
        Assert.That(result.Data![0].Date, Is.EqualTo(new DateTime(2024, 11, 28)));
        Assert.That(result.Data![2].Description, Is.EqualTo("Test Description"));
        Assert.That(result.Data![1].Description, Is.EqualTo("Test2 Description"));
        Assert.That(result.Data![0].Description, Is.EqualTo("Test3 Description"));
    }

    [Test]
    public async Task GetFestivalHolidayByMonth_Notfound()
    {
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday
            {
                Id = 1,
                FestivalName = "Test1 Holiday",
                Description = "Test Description",
                Date = new DateTime(2024, 12, 25),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 2,
                FestivalName = "Test2 Holiday",
                Description = "Test2 Description",
                Date = new DateTime(2024, 1, 1),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 3,
                FestivalName = "Test3 Holiday",
                Description = "Test3 Description",
                Date = new DateTime(2024, 11, 28),
                IsActive = true,
                IsDeleted = false
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _festivalHolidayService.GetFestivalHolidayByMonthAsync(2024, 5);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Empty);

    }

    [Test]
    public async Task GetFestivalHolidayByMonth_Found()
    {
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday
            {
                Id = 1,
                FestivalName = "Test1 Holiday",
                Description = "Test Description",
                Date = new DateTime(2024, 12, 25),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 2,
                FestivalName = "Test2 Holiday",
                Description = "Test2 Description",
                Date = new DateTime(2024, 1, 1),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 3,
                FestivalName = "Test3 Holiday",
                Description = "Test3 Description",
                Date = new DateTime(2024, 11, 28),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 4,
                FestivalName = "Test4 Holiday",
                Description = "Test4 Description",
                Date = new DateTime(2024, 12, 31),
                IsActive = true,
                IsDeleted = false
            }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _festivalHolidayService.GetFestivalHolidayByMonthAsync(2024, 12);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(2));
        Assert.That(result.Data![1].FestivalName, Is.EqualTo("Test4 Holiday"));
        Assert.That(result.Data![1].Date, Is.EqualTo(new DateTime(2024, 12, 31)));
        Assert.That(result.Data![1].Description, Is.EqualTo("Test4 Description"));
        Assert.That(result.Data![0].FestivalName, Is.EqualTo("Test1 Holiday"));
        Assert.That(result.Data![0].Date, Is.EqualTo(new DateTime(2024, 12, 25)));
        Assert.That(result.Data![0].Description, Is.EqualTo("Test Description"));
    }

    [Test]
    public async Task CreateFestivalHoliday_Success()
    {
        var newHolidayDto = new FestivalHolidayDto
        {
            FestivalName = "New Year",
            Description = "New Year Holiday",
            Date = new DateTime(2025, 1, 1)
        };
        var result = await _festivalHolidayService.CreateFestivalHolidayAsync(newHolidayDto);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.GreaterThan(0));
        Assert.That(result.Data!.FestivalName, Is.EqualTo("New Year"));
        Assert.That(result.Data!.Description, Is.EqualTo("New Year Holiday"));
        Assert.That(result.Data!.Date, Is.EqualTo(new DateTime(2025, 1, 1)));
    }

    [Test]
    public async Task CreateFestivalHoliday_Fail_NullRequest()
    {
        var result = await _festivalHolidayService.CreateFestivalHolidayAsync(null!);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task UpdateFestivalHoliday_Fail_NullRequest()
    {
        var result = await _festivalHolidayService.UpdateFestivalHolidayAsync(null!);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task UpdateFestivalHoliday_Success()
    {
        var existingHoliday = new FestivalHoliday
        {
            FestivalName = "Old Festival",
            Description = "Old Description",
            Date = new DateTime(2024, 5, 1),
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.FestivalHoliday.Add(existingHoliday);
        await _dbContext.SaveChangesAsync();

        Assert.That(existingHoliday.Id, Is.GreaterThan(0));
        Assert.That(existingHoliday.FestivalName, Is.EqualTo("Old Festival"));
        Assert.That(existingHoliday.Description, Is.EqualTo("Old Description"));
        Assert.That(existingHoliday.Date, Is.EqualTo(new DateTime(2024, 5, 1)));

        _dbContext.Entry(existingHoliday).State = EntityState.Detached;

        var updateDto = new FestivalHolidayDto
        {
            Id = existingHoliday.Id,
            FestivalName = "Updated Festival",
            Description = "Updated Description",
            Date = new DateTime(2024, 6, 1)
        };

        var result = await _festivalHolidayService.UpdateFestivalHolidayAsync(updateDto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(existingHoliday.Id));
        Assert.That(result.Data!.FestivalName, Is.EqualTo("Updated Festival"));
        Assert.That(result.Data!.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Data!.Date, Is.EqualTo(new DateTime(2024, 6, 1)));
    }

    [Test]
    public async Task UpdateFestivalHoliday_Fail_NotFound()
    {
        var updateDto = new FestivalHolidayDto
        {
            Id = -999,
            FestivalName = "Nonexistent Festival",
            Description = "Nonexistent Description",
            Date = new DateTime(2024, 7, 1)
        };

        var result = await _festivalHolidayService.UpdateFestivalHolidayAsync(updateDto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid FestivalHoliday ID"));
    }

    [Test]
    public async Task DeleteFestivalHoliday()
    {
        var holiday = new FestivalHoliday
        {
            FestivalName = "Delete Test Holiday",
            Description = "To be deleted",
            Date = new DateTime(2024, 8, 15),
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.FestivalHoliday.Add(holiday);
        await _dbContext.SaveChangesAsync();

        Assert.That(holiday.Id, Is.GreaterThan(0));
        Assert.That(holiday.IsActive, Is.True);
        Assert.That(holiday.IsDeleted, Is.False);

        var result = await _festivalHolidayService.DeleteFestivalHolidayAsync(holiday.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(holiday.IsDeleted, Is.True);
        Assert.That(holiday.IsActive, Is.False);
    }

    [Test]
    public async Task DeleteFestivalHoliday_Fail_NotFound()
    {
        var result = await _festivalHolidayService.DeleteFestivalHolidayAsync(-999);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid FestivalHoliday ID"));
    }
}
