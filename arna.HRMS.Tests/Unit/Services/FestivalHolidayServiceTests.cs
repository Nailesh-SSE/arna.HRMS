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
                DayOfWeek = new DateTime(2024, 12, 25).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 2,
                FestivalName = "Test2 Holiday",
                Description = "Test2 Description",
                Date = new DateTime(2024, 1, 1),
                DayOfWeek = new DateTime(2024, 1, 1).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 3,
                FestivalName = "Test3 Holiday",
                Description = "Test3 Description",
                Date = new DateTime(2024, 11, 28),
                DayOfWeek = new DateTime(2024, 11, 28).DayOfWeek.ToString(),
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
    public async Task GetAllFestivalHoliday_Empty()
    {
        var result = await _festivalHolidayService.GetFestivalHolidayAsync();
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task GetFestivalHolidayById_Found()
    {
        _dbContext.FestivalHoliday.Add(
            new FestivalHoliday
            {
                Id = 1,
                FestivalName = "Test1 Holiday",
                Description = "Test Description",
                Date = new DateTime(2024, 12, 25),
                DayOfWeek = new DateTime(2024, 12, 25).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _festivalHolidayService.GetFestivalHolidayByIdAsync(1);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.FestivalName, Is.EqualTo("Test1 Holiday"));
        Assert.That(result.Data!.Date, Is.EqualTo(new DateTime(2024, 12, 25)));
        Assert.That(result.Data!.Description, Is.EqualTo("Test Description"));
    }

    [Test]
    public async Task GetFestivalHolidayById_WhenIsNegativeOrZero()
    {
        var result = await _festivalHolidayService.GetFestivalHolidayByIdAsync(-999);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid FestivalHoliday ID"));
        Assert.That(result.Data, Is.Null);

        var resultZero = await _festivalHolidayService.GetFestivalHolidayByIdAsync(0);
        Assert.That(resultZero.IsSuccess, Is.False);
        Assert.That(resultZero.Message, Is.EqualTo("Invalid FestivalHoliday ID"));
    }

    [Test]
    public async Task GetFestivalHolidayById_WhenDBEmptyOrNotFound()
    {
        var result = await _festivalHolidayService.GetFestivalHolidayByIdAsync(1);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Festival holiday not found"));
    }

    [Test]
    public async Task GetFestivalHolidayById_WhenAlreadyDeleted()
    {
        _dbContext.FestivalHoliday.Add(
            new FestivalHoliday
            {
                Id = 1,
                FestivalName = "Test1 Holiday",
                Description = "Test Description",
                Date = new DateTime(2024, 12, 25),
                DayOfWeek = new DateTime(2024, 12, 25).DayOfWeek.ToString(),
                IsActive = false,
                IsDeleted = true
            }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _festivalHolidayService.GetFestivalHolidayByIdAsync(1);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Festival holiday not found"));
    }

    [Test]
    public async Task GetFestivalHolidayByMonth_EmptyDB()
    {
        var result = await _festivalHolidayService.GetFestivalHolidayByMonthAsync(2024, 5);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
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
                DayOfWeek = new DateTime(2024, 12, 25).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 2,
                FestivalName = "Test2 Holiday",
                Description = "Test2 Description",
                Date = new DateTime(2024, 1, 1),
                DayOfWeek = new DateTime(2024, 1, 1).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 3,
                FestivalName = "Test3 Holiday",
                Description = "Test3 Description",
                Date = new DateTime(2024, 11, 28),
                DayOfWeek = new DateTime(2024, 11, 28).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _festivalHolidayService.GetFestivalHolidayByMonthAsync(2024, 5);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);

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
                DayOfWeek = new DateTime(2024, 12, 25).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 2,
                FestivalName = "Test2 Holiday",
                Description = "Test2 Description",
                Date = new DateTime(2024, 1, 1),
                DayOfWeek = new DateTime(2024, 1, 1).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 3,
                FestivalName = "Test3 Holiday",
                Description = "Test3 Description",
                Date = new DateTime(2024, 11, 28),
                DayOfWeek = new DateTime(2024, 11, 28).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 4,
                FestivalName = "Test4 Holiday",
                Description = "Test4 Description",
                Date = new DateTime(2024, 12, 31),
                DayOfWeek = new DateTime(2024, 12, 31).DayOfWeek.ToString(),
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
    public async Task GetFestivalHoliday_MonthGreater()
    {
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday
            {
                Id = 1,
                FestivalName = "Test1 Holiday",
                Description = "Test Description",
                Date = new DateTime(2024, 12, 25),
                DayOfWeek= new DateTime(2024, 12, 25).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 2,
                FestivalName = "Test2 Holiday",
                Description = "Test2 Description",
                Date = new DateTime(2024, 1, 1),
                DayOfWeek = new DateTime(2024, 1, 1).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _festivalHolidayService.GetFestivalHolidayByMonthAsync(2024, 13);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid month"));
        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task GetFestivalHoliday_MonthLess()
    {
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday
            {
                Id = 1,
                FestivalName = "Test1 Holiday",
                Description = "Test Description",
                Date = new DateTime(2024, 12, 25),
                DayOfWeek = new DateTime(2024, 12, 25).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 2,
                FestivalName = "Test2 Holiday",
                Description = "Test2 Description",
                Date = new DateTime(2024, 1, 1),
                DayOfWeek = new DateTime(2024, 1, 1).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _festivalHolidayService.GetFestivalHolidayByMonthAsync(2024, 0);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid month"));
        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task GetFestivalHolidayByMolthly_YearLess()
    {
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday
            {
                Id = 1,
                FestivalName = "Test1 Holiday",
                Description = "Test Description",
                Date = new DateTime(2024, 12, 25),
                DayOfWeek = new DateTime(2024, 12, 25).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 2,
                FestivalName = "Test2 Holiday",
                Description = "Test2 Description",
                Date = new DateTime(2024, 1, 1),
                DayOfWeek = new DateTime(2024, 1, 1).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _festivalHolidayService.GetFestivalHolidayByMonthAsync(0, 5);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid year"));
        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task GetFestivalHolidayByMolthly_YearGreater()
    {
        _dbContext.FestivalHoliday.AddRange(
            new FestivalHoliday
            {
                Id = 1,
                FestivalName = "Test1 Holiday",
                Description = "Test Description",
                Date = new DateTime(2024, 12, 25),
                DayOfWeek = new DateTime(2024, 12, 25).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            },
            new FestivalHoliday
            {
                Id = 2,
                FestivalName = "Test2 Holiday",
                Description = "Test2 Description",
                Date = new DateTime(2024, 1, 1),
                DayOfWeek = new DateTime(2024, 1, 1).DayOfWeek.ToString(),
                IsActive = true,
                IsDeleted = false
            }
        );
        await _dbContext.SaveChangesAsync();
        var result = await _festivalHolidayService.GetFestivalHolidayByMonthAsync(3000, 5);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task CreateFestivalHoliday_Success()
    {
        var newHolidayDto = new FestivalHolidayDto
        {
            FestivalName = "New Year",
            Description = "New Year Holiday",
            Date = DateTime.Now.AddDays(1)
        };
        var result = await _festivalHolidayService.CreateFestivalHolidayAsync(newHolidayDto);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.GreaterThan(0));
        Assert.That(result.Data!.FestivalName, Is.EqualTo("New Year"));
        Assert.That(result.Data!.Description, Is.EqualTo("New Year Holiday"));
        Assert.That(result.Data!.Date.Date, Is.EqualTo(DateTime.Now.AddDays(1).Date));
    }

    [Test]
    public async Task CreateFestivalHoliday_Fail_NullRequest()
    {
        var result = await _festivalHolidayService.CreateFestivalHolidayAsync(null!);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task CreateFestivalHoliday_Fail_MissingName()
    {
        var newHolidayDto = new FestivalHolidayDto
        {
            Description = "Holiday without a name",
            Date = DateTime.Now.AddDays(1)
        };
        var result = await _festivalHolidayService.CreateFestivalHolidayAsync(newHolidayDto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Festival name is required"));
    }

    [Test]
    public async Task CreateFestivalHoliday_Fail_MissingDate()
    {
        var newHolidayDto = new FestivalHolidayDto
        {
            FestivalName = "Nameless Holiday",
            Description = "Holiday without a date"
        };
        var result = await _festivalHolidayService.CreateFestivalHolidayAsync(newHolidayDto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Festival date is required"));
    }

    [Test]
    public async Task CreateFestivalHoliday_Fail_PastDate()
    {
        var newHolidayDto = new FestivalHolidayDto
        {
            FestivalName = "Past Holiday",
            Description = "Holiday with a past date",
            Date = DateTime.Now.AddDays(-1)
        };
        var result = await _festivalHolidayService.CreateFestivalHolidayAsync(newHolidayDto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Festival date cannot be in the past"));
    }

    [Test]
    public async Task CreateFestivalHoliday_WhenOneFestivalInPastAndAddNewEntryOfSameNameInFuture_Success()
    {
        var existingHoliday = new FestivalHolidayDto
        {
            FestivalName = "Recurring Holiday",
            Description = "Past holiday",
            Date = DateTime.Now.AddDays(-10),
            IsActive = true,
            IsDeleted = false
        };
        await _festivalHolidayService.CreateFestivalHolidayAsync(existingHoliday);
        var newHolidayDto = new FestivalHolidayDto
        {
            FestivalName = "Recurring Holiday",
            Description = "Future holiday with same name",
            Date = DateTime.Now.AddDays(10),
            IsActive = true,
            IsDeleted = false
        };
        var result = await _festivalHolidayService.CreateFestivalHolidayAsync(newHolidayDto);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.FestivalName, Is.EqualTo("Recurring Holiday"));
        Assert.That(result.Data!.Date.Date, Is.EqualTo(DateTime.Now.AddDays(10).Date));
    }

    [Test]
    public async Task CreateFestivalHoliday_Fail_Duplicate()
    {
        var existingHoliday = new FestivalHolidayDto
        {
            FestivalName = "Duplicate Holiday",
            Description = "Existing holiday",
            Date = DateTime.Now.AddDays(1),
            IsActive = true,
            IsDeleted = false
        };
        await _festivalHolidayService.CreateFestivalHolidayAsync(existingHoliday);

        var newHolidayDto = new FestivalHolidayDto
        {
            FestivalName = "Duplicate Holiday",
            Description = "New holiday with duplicate name and date",
            Date = DateTime.Now.AddDays(1),
            IsActive = true,
            IsDeleted = false
        };

        var result = await _festivalHolidayService.CreateFestivalHolidayAsync(newHolidayDto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("A festival with the same name already exists"));
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
            Date = DateTime.Now.AddDays(1),
            DayOfWeek = DateTime.Now.AddDays(1).DayOfWeek.ToString(),
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.FestivalHoliday.Add(existingHoliday);
        await _dbContext.SaveChangesAsync();

        Assert.That(existingHoliday.Id, Is.GreaterThan(0));
        Assert.That(existingHoliday.FestivalName, Is.EqualTo("Old Festival"));
        Assert.That(existingHoliday.Description, Is.EqualTo("Old Description"));
        Assert.That(existingHoliday.Date.Date, Is.EqualTo(DateTime.Now.AddDays(1).Date));

        _dbContext.Entry(existingHoliday).State = EntityState.Detached;

        var updateDto = new FestivalHolidayDto
        {
            Id = existingHoliday.Id,
            FestivalName = "Updated Festival",
            Description = "Updated Description",
            Date = DateTime.Now.AddDays(2)
        };

        var result = await _festivalHolidayService.UpdateFestivalHolidayAsync(updateDto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(existingHoliday.Id));
        Assert.That(result.Data!.FestivalName, Is.EqualTo("Updated Festival"));
        Assert.That(result.Data!.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Data!.Date.Date, Is.EqualTo(DateTime.Now.AddDays(2).Date));
    }

    [Test]
    public async Task UpdateFestivalHoliday_Fail_NotFound()
    {
        var updateDto = new FestivalHolidayDto
        {
            Id = -999,
            FestivalName = "Nonexistent Festival",
            Description = "Nonexistent Description",
            Date = DateTime.Now.AddDays(1)
        };

        var result = await _festivalHolidayService.UpdateFestivalHolidayAsync(updateDto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid FestivalHoliday ID"));
    }

    [Test]
    public async Task UpdateFestivalHoliday_Fail_IdZero()
    {
        var updateDto = new FestivalHolidayDto
        {
            Id = 0,
            FestivalName = "Zero ID Festival",
            Description = "Zero ID Description",
            Date = DateTime.Now.AddDays(1)
        };
        var result = await _festivalHolidayService.UpdateFestivalHolidayAsync(updateDto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid FestivalHoliday ID"));
    }

    [Test]
    public async Task UpdateFestivalHoliday_Fail_IdNegative()
    {
        var updateDto = new FestivalHolidayDto
        {
            Id = -5,
            FestivalName = "Negative ID Festival",
            Description = "Negative ID Description",
            Date = DateTime.Now.AddDays(1)
        };
        var result = await _festivalHolidayService.UpdateFestivalHolidayAsync(updateDto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid FestivalHoliday ID"));
    }

    [Test]
    public async Task UpdateFestivalHoliday_WhereAlreadyDeleted()
    {
        var existingHoliday = new FestivalHoliday
        {
            FestivalName = "Deleted Festival",
            Description = "Deleted Description",
            Date = DateTime.Now.AddDays(1),
            DayOfWeek = DateTime.Now.AddDays(1).DayOfWeek.ToString(),
            IsActive = false,
            IsDeleted = true
        };
        _dbContext.FestivalHoliday.Add(existingHoliday);
        await _dbContext.SaveChangesAsync();
        Assert.That(existingHoliday.Id, Is.GreaterThan(0));
        _dbContext.Entry(existingHoliday).State = EntityState.Detached;
        var updateDto = new FestivalHolidayDto
        {
            Id = existingHoliday.Id,
            FestivalName = "Deleted Festival",
            Description = "Updated Description",
            Date = DateTime.Now.AddDays(1)
        };
        var result = await _festivalHolidayService.UpdateFestivalHolidayAsync(updateDto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Festival holiday not found"));
    }

    [Test]
    public async Task UpdateFestivalHoliday_Fail_DuplicateName()
    {
        var holiday1 = new FestivalHoliday
        {
            FestivalName = "Existing Festival",
            Description = "First Holiday",
            Date = DateTime.Now.AddDays(1),
            DayOfWeek = DateTime.Now.AddDays(1).DayOfWeek.ToString(),
            IsActive = true,
            IsDeleted = false
        };
        var holiday2 = new FestivalHoliday
        {
            FestivalName = "Another Festival",
            Description = "Second Holiday",
            Date = DateTime.Now.AddDays(1),
            DayOfWeek = DateTime.Now.AddDays(1).DayOfWeek.ToString(),
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.FestivalHoliday.AddRange(holiday1, holiday2);
        await _dbContext.SaveChangesAsync();
        Assert.That(holiday1.Id, Is.GreaterThan(0));
        Assert.That(holiday2.Id, Is.GreaterThan(0));

        _dbContext.Entry(holiday2).State = EntityState.Detached;

        var updateDto = new FestivalHolidayDto
        {
            Id = holiday2.Id,
            FestivalName = "Existing Festival",
            Description = "Updated Description",
            Date = DateTime.Now.AddDays(1)
        };
        var result = await _festivalHolidayService.UpdateFestivalHolidayAsync(updateDto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("A festival with the same name already exists"));
    }

    [Test]
    public async Task UpdateFestivalHoliday_Fail_PastDate()
    {
        var existingHoliday = new FestivalHoliday
        {
            FestivalName = "Future Festival",
            Description = "Future Description",
            Date = DateTime.Now.AddDays(1),
            DayOfWeek = DateTime.Now.AddDays(1).DayOfWeek.ToString(),
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.FestivalHoliday.Add(existingHoliday);
        await _dbContext.SaveChangesAsync();
        Assert.That(existingHoliday.Id, Is.GreaterThan(0));
        _dbContext.Entry(existingHoliday).State = EntityState.Detached;
        var updateDto = new FestivalHolidayDto
        {
            Id = existingHoliday.Id,
            FestivalName = "Future Festival",
            Description = "Updated Description",
            Date = DateTime.Now.AddDays(-1)
        };
        var result = await _festivalHolidayService.UpdateFestivalHolidayAsync(updateDto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Festival date cannot be in the past"));
    }

    [Test]
    public async Task UpdateFestivalHoliday_Fail_MissingName()
    {
        var existingHoliday = new FestivalHoliday
        {
            FestivalName = "Nameless Festival",
            Description = "Nameless Description",
            Date = DateTime.Now.AddDays(1),
            DayOfWeek = DateTime.Now.AddDays(1).DayOfWeek.ToString(),
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.FestivalHoliday.Add(existingHoliday);
        await _dbContext.SaveChangesAsync();
        Assert.That(existingHoliday.Id, Is.GreaterThan(0));
        _dbContext.Entry(existingHoliday).State = EntityState.Detached;
        var updateDto = new FestivalHolidayDto
        {
            Id = existingHoliday.Id,
            Description = "Updated Description",
            Date = DateTime.Now.AddDays(1)
        };
        var result = await _festivalHolidayService.UpdateFestivalHolidayAsync(updateDto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Festival name is required"));
    }

    [Test]
    public async Task UpdateFestivalHoliday_Fail_MissingDate()
    {
        var existingHoliday = new FestivalHoliday
        {
            FestivalName = "Dateless Festival",
            Description = "Dateless Description",
            Date = DateTime.Now.AddDays(1),
            DayOfWeek = DateTime.Now.AddDays(1).DayOfWeek.ToString(),
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.FestivalHoliday.Add(existingHoliday);
        await _dbContext.SaveChangesAsync();
        Assert.That(existingHoliday.Id, Is.GreaterThan(0));
        _dbContext.Entry(existingHoliday).State = EntityState.Detached;
        var updateDto = new FestivalHolidayDto
        {
            Id = existingHoliday.Id,
            FestivalName = "Dateless Festival",
            Description = "Updated Description"
        };
        var result = await _festivalHolidayService.UpdateFestivalHolidayAsync(updateDto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Festival date is required"));
    }

    [Test]
    public async Task DeleteFestivalHoliday()
    {
        var holiday = new FestivalHoliday
        {
            FestivalName = "Delete Test Holiday",
            Description = "To be deleted",
            Date = new DateTime(2024, 8, 15),
            DayOfWeek = new DateTime(2024, 8, 15).DayOfWeek.ToString(),
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

    [Test]
    public async Task DeleteFestivalHoliday_WhenIsNegativeOrZero()
    {
        var result = await _festivalHolidayService.DeleteFestivalHolidayAsync(0);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid FestivalHoliday ID"));
    }

    [Test]
    public async Task DeleteFestivalHoliday_Fail_AlreadyDeleted()
    {
        var holiday = new FestivalHoliday
        {
            FestivalName = "Already Deleted Holiday",
            Description = "Already deleted",
            Date = new DateTime(2024, 8, 15),
            DayOfWeek = new DateTime(2024, 8, 15).DayOfWeek.ToString(),
            IsActive = false,
            IsDeleted = true
        };
        _dbContext.FestivalHoliday.Add(holiday);
        await _dbContext.SaveChangesAsync();
        Assert.That(holiday.Id, Is.GreaterThan(0));
        var result = await _festivalHolidayService.DeleteFestivalHolidayAsync(holiday.Id);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Festival not found"));
    }

    [Test]
    public async Task DeleteFestivalHoliday_Fail_EmptyDB()
    {
        var result = await _festivalHolidayService.DeleteFestivalHolidayAsync(1);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Festival not found"));
    }

    [Test]
    public async Task GetFestivalHolidayByName_whenFound()
    {
        var holiday = new FestivalHoliday
        {
            FestivalName = "Unique Holiday",
            Description = "Unique Description",
            Date = DateTime.Now.AddDays(1),
            DayOfWeek = DateTime.Now.AddDays(1).DayOfWeek.ToString(),
            IsActive = true,
            IsDeleted = false
        };
        _dbContext.FestivalHoliday.Add(holiday);
        await _dbContext.SaveChangesAsync();
        var result = await _festivalHolidayService.GetFestivalHolidayByNameAsync("Unique Holiday");
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data![0]!.FestivalName, Is.EqualTo("Unique Holiday"));
        Assert.That(result.Data![0]!.Description, Is.EqualTo("Unique Description"));
    }

    [Test]
    public async Task GetFestivalHolidayByName_whenNotFound()
    {
        var result = await _festivalHolidayService.GetFestivalHolidayByNameAsync("Nonexistent Holiday");
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("No Data Found"));
    }

    [Test]
    public async Task GetFestivalHolidayByName_WhenAlreadyDeleted()
    {
        var holiday = new FestivalHoliday
        {
            FestivalName = "Deleted Holiday",
            Description = "Deleted Description",
            Date = DateTime.Now.AddDays(1),
            DayOfWeek = DateTime.Now.AddDays(1).DayOfWeek.ToString(),
            IsActive = false,
            IsDeleted = true
        };
        _dbContext.FestivalHoliday.Add(holiday);
        await _dbContext.SaveChangesAsync();
        var result = await _festivalHolidayService.GetFestivalHolidayByNameAsync("Deleted Holiday");
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("No Data Found"));
    }


    [Test]
    public async Task GetFestivalHolidayByName_whenNameIsNullOrWhitespace()
    {
        var resultNull = await _festivalHolidayService.GetFestivalHolidayByNameAsync(null!);
        Assert.That(resultNull.IsSuccess, Is.False);
        Assert.That(resultNull.Message, Is.EqualTo("Festival name is required"));

        var resultWhitespace = await _festivalHolidayService.GetFestivalHolidayByNameAsync(" ");
        Assert.That(resultWhitespace.IsSuccess, Is.False);
        Assert.That(resultWhitespace.Message, Is.EqualTo("Festival name is required"));
    }

}
