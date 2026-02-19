using arna.HRMS.API.Controllers.Admin;
using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Controllers;

[TestFixture]
public class FestivalHolidayControllerTests
{
    private Mock<IFestivalHolidayService> _festivalHolidayServiceMock = null!;
    private FestivalHolidayController _FestivalHolidayController = null!;

    [SetUp]
    public void SetUp()
    {
        _festivalHolidayServiceMock = new Mock<IFestivalHolidayService>();
        _FestivalHolidayController = new FestivalHolidayController(_festivalHolidayServiceMock.Object);
    }

    [Test]
    public async Task GetFestivalHoliday_ReturnsOkResult()
    {
        // Arrange
        var festivalHolidays = new List<FestivalHolidayDto>
        {
            new FestivalHolidayDto { Id = 1, FestivalName = "New Year", Date = new DateTime(2024, 1, 1) },
            new FestivalHolidayDto { Id = 2, FestivalName = "Christmas", Date = new DateTime(2024, 12, 25) }
        };
        _festivalHolidayServiceMock.Setup(service => service.GetFestivalHolidayAsync())
            .ReturnsAsync(ServiceResult<List<FestivalHolidayDto>>.Success(festivalHolidays));
        // Act
        var result = await _FestivalHolidayController.GetFestivalHoliday();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetFestivalHoliday_ShouldSucces_WhenDatabaseEmpty()
    {
        // Arrange
        var festivalHolidays = new List<FestivalHolidayDto>();
        _festivalHolidayServiceMock.Setup(service => service.GetFestivalHolidayAsync())
            .ReturnsAsync(ServiceResult<List<FestivalHolidayDto>>.Success(festivalHolidays));
        // Act
        var result = await _FestivalHolidayController.GetFestivalHoliday();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetFestivalHolidayById_ReturnsOkResult_WhenHolidayExists()
    {
        // Arrange
        var festivalHoliday = new FestivalHolidayDto { Id = 1, FestivalName = "New Year", Date = new DateTime(2024, 1, 1) };
        _festivalHolidayServiceMock.Setup(service => service.GetFestivalHolidayByIdAsync(festivalHoliday.Id))
            .ReturnsAsync(ServiceResult<FestivalHolidayDto?>.Success(festivalHoliday));
        // Act
        var result = await _FestivalHolidayController.GetFestivalHolidayById(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetFestivalHolidayById_ReturnsNotFound_WhenHolidayDoesNotExist()
    {
        // Arrange
        _festivalHolidayServiceMock.Setup(service => service.GetFestivalHolidayByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<FestivalHolidayDto?>.Success(null));
        // Act
        var result = await _FestivalHolidayController.GetFestivalHolidayById(999);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetFestivalHolidayById_ShouldFail_WhenIdIsZeroOrNegative()
    {
        // Arrange
        _festivalHolidayServiceMock.Setup(service => service.GetFestivalHolidayByIdAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<FestivalHolidayDto?>.Fail("Invalid ID"));
        // Act
        var result = await _FestivalHolidayController.GetFestivalHolidayById(0);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());

        // Act
        result = await _FestivalHolidayController.GetFestivalHolidayById(-1);

        //Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetHolidaysByMonth_ReturnsOkResult_WhenHolidaysExist()
    {
        // Arrange
        var festivalHolidays = new List<FestivalHolidayDto>
        {
            new FestivalHolidayDto { Id = 1, FestivalName = "New Year", Date = new DateTime(2024, 1, 1) },
            new FestivalHolidayDto { Id = 2, FestivalName = "Christmas", Date = new DateTime(2024, 12, 25) }
        };

        _festivalHolidayServiceMock
            .Setup(service => service.GetFestivalHolidayByMonthAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((int year, int month) =>
            {
                var filtered = festivalHolidays
                    .Where(x => x.Date.Year == year && x.Date.Month == month)
                    .ToList();

                return ServiceResult<List<FestivalHolidayDto>>.Success(filtered);
            });

        // Act
        var actionResult = await _FestivalHolidayController.GetHolidaysByMonth(2024, 1);

        // Assert
        Assert.That(actionResult, Is.TypeOf<OkObjectResult>());

        var okResult = actionResult as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        var serviceResult = okResult!.Value as ServiceResult<List<FestivalHolidayDto>>;
        Assert.That(serviceResult, Is.Not.Null);

        Assert.That(serviceResult!.Data!.Count, Is.EqualTo(1));
    }

}
