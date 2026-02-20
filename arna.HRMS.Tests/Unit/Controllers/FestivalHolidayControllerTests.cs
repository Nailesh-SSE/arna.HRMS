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
            .ReturnsAsync(ServiceResult<FestivalHolidayDto?>.Fail("No data found"));
        // Act
        var result = await _FestivalHolidayController.GetFestivalHolidayById(999);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
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

    [Test]
    public async Task GetHolidaysByMonth_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        _festivalHolidayServiceMock
            .Setup(service => service.GetFestivalHolidayByMonthAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<List<FestivalHolidayDto>>.Fail("Service error"));
        // Act
        var result = await _FestivalHolidayController.GetHolidaysByMonth(2024, 1);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateFestivalHoliday_ReturnsOkResult_WhenCreationIsSuccessful()
    {
        // Arrange
        var festivalHolidayDto = new FestivalHolidayDto { Id = 1, FestivalName = "New Year", Date = new DateTime(2024, 1, 1) };
        _festivalHolidayServiceMock.Setup(service => service.CreateFestivalHolidayAsync(festivalHolidayDto))
            .ReturnsAsync(ServiceResult<FestivalHolidayDto>.Success(festivalHolidayDto));
        // Act
        var result = await _FestivalHolidayController.CreateFestivalHoliday(festivalHolidayDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task CreateFestivalHoliday_ReturnsBadRequest_WhenCreationFails()
    {
        // Arrange
        var festivalHolidayDto = new FestivalHolidayDto { Id = 1, FestivalName = "New Year", Date = new DateTime(2024, 1, 1) };
        _festivalHolidayServiceMock.Setup(service => service.CreateFestivalHolidayAsync(festivalHolidayDto))
            .ReturnsAsync(ServiceResult<FestivalHolidayDto>.Fail("Creation failed"));
        // Act
        var result = await _FestivalHolidayController.CreateFestivalHoliday(festivalHolidayDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateFestivalHoliday_ReturnsBadRequest_WhenModelIsInvalid()
    {
        // Arrange
        var festivalHolidayDto = new FestivalHolidayDto { Id = 1, FestivalName = "", Date = new DateTime(2024, 1, 1) };
        _FestivalHolidayController.ModelState.AddModelError("FestivalName", "Festival name is required");
        // Act
        var result = await _FestivalHolidayController.CreateFestivalHoliday(festivalHolidayDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateFestivalHoliday_ReturnsOkResult_WhenUpdateIsSuccessful()
    {
        // Arrange
        var festivalHolidayDto = new FestivalHolidayDto { Id = 1, FestivalName = "New Year", Date = new DateTime(2024, 1, 1) };
        _festivalHolidayServiceMock.Setup(service => service.UpdateFestivalHolidayAsync(festivalHolidayDto))
            .ReturnsAsync(ServiceResult<FestivalHolidayDto>.Success(festivalHolidayDto));
        // Act
        var result = await _FestivalHolidayController.UpdateFestivalHoliday(1, festivalHolidayDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateFestivalHoliday_ReturnsBadRequest_WhenUpdateFails()
    {
        // Arrange
        var festivalHolidayDto = new FestivalHolidayDto { Id = 1, FestivalName = "New Year", Date = new DateTime(2024, 1, 1) };
        _festivalHolidayServiceMock.Setup(service => service.UpdateFestivalHolidayAsync(festivalHolidayDto))
            .ReturnsAsync(ServiceResult<FestivalHolidayDto>.Fail("Update failed"));
        // Act
        var result = await _FestivalHolidayController.UpdateFestivalHoliday(1, festivalHolidayDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateFestivalHoliday_ReturnBadRequest_WhenInvalidModelState()
    {
        var festivalHolidayDto = new FestivalHolidayDto { Id = 1, Date = new DateTime(2024, 1, 1) };

        _FestivalHolidayController.ModelState.AddModelError("FestivalName", "Festival name is required");

        var result = await _FestivalHolidayController.UpdateFestivalHoliday(1, festivalHolidayDto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateFestivalHoliday_ReturnsBadRequestResult_WhenIdIsZeroOrNegative()
    {
        // Arrange
        var festivalHolidayDto = new FestivalHolidayDto { Id = 1, FestivalName = "New Year", Date = new DateTime(2024, 1, 1) };
        _festivalHolidayServiceMock.Setup(service => service.UpdateFestivalHolidayAsync(It.Is<FestivalHolidayDto>(dto => dto.Id <= 0)))
            .ReturnsAsync(ServiceResult<FestivalHolidayDto>.Fail("Invalid ID"));
        // Act
        var result = await _FestivalHolidayController.UpdateFestivalHoliday(0, festivalHolidayDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        // Act
        result = await _FestivalHolidayController.UpdateFestivalHoliday(-1, festivalHolidayDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateFestivalHoliday_ReturnsBadRequest_WhenModelIsInvalid()
    {
        // Arrange
        var festivalHolidayDto = new FestivalHolidayDto { Id = 1, FestivalName = "", Date = new DateTime(2024, 1, 1) };
        _FestivalHolidayController.ModelState.AddModelError("FestivalName", "Festival name is required");
        // Act
        var result = await _FestivalHolidayController.UpdateFestivalHoliday(1, festivalHolidayDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateFestivalHoliday_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        var festivalHolidayDto = new FestivalHolidayDto { Id = 1, FestivalName = "New Year", Date = new DateTime(2024, 1, 1) };

        _festivalHolidayServiceMock.Setup(service => service.UpdateFestivalHolidayAsync(festivalHolidayDto))
            .ReturnsAsync(ServiceResult<FestivalHolidayDto>.Fail("Update failed"));
        // Act
        var result = await _FestivalHolidayController.UpdateFestivalHoliday(2, festivalHolidayDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeleteFestivalHoliday_ReturnsOkResult_WhenDeletionIsSuccessful()
    {
        // Arrange
        _festivalHolidayServiceMock.Setup(service => service.DeleteFestivalHolidayAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Success(true));
        // Act
        var result = await _FestivalHolidayController.DeleteFestivalHoliday(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task DeleteFestivalHoliday_ReturnsBadRequest_WhenDeletionFails()
    {
        // Arrange
        _festivalHolidayServiceMock.Setup(service => service.DeleteFestivalHolidayAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Fail("Deletion failed"));
        // Act
        var result = await _FestivalHolidayController.DeleteFestivalHoliday(1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteFestivalHoliday_ShouldFail_WhenIdIsZeroOrNehative() 
    {
        // Arrange
        _festivalHolidayServiceMock.Setup(service => service.DeleteFestivalHolidayAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<bool>.Fail("Invalid ID"));
        // Act
        var result = await _FestivalHolidayController.DeleteFestivalHoliday(0);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        // Act
        result = await _FestivalHolidayController.DeleteFestivalHoliday(-1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteFestivalHoliday_ReturnsNotFound_WhenHolidayDoesNotExist()
    {
        // Arrange
        _festivalHolidayServiceMock.Setup(service => service.DeleteFestivalHolidayAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<bool>.Fail("No data found"));
        // Act
        var result = await _FestivalHolidayController.DeleteFestivalHoliday(999);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetFestivalHolidayByNameAsync_ReturnsOkResult_WhenHolidayExists()
    {
        // Arrange
        var festivalHoliday = new FestivalHolidayDto { Id = 1, FestivalName = "New Year", Date = new DateTime(2024, 1, 1) };
        _festivalHolidayServiceMock.Setup(service => service.GetFestivalHolidayByNameAsync(festivalHoliday.FestivalName))
            .ReturnsAsync(ServiceResult<List<FestivalHolidayDto?>>.Success(new List<FestivalHolidayDto?> { festivalHoliday }));
        // Act
        var result = await _FestivalHolidayController.CheckFestivalHolidayName(festivalHoliday.FestivalName);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetFestivalHolidayByNameAsync_ReturnsBadRequest_WhenHolidayDoesNotExist()
    {
        // Arrange
        _festivalHolidayServiceMock.Setup(service => service.GetFestivalHolidayByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<List<FestivalHolidayDto?>>.Fail("No data found"));
        // Act
        var result = await _FestivalHolidayController.CheckFestivalHolidayName("NonExistingHoliday");
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetFestivalHolidayByNameAsync_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        _festivalHolidayServiceMock.Setup(service => service.GetFestivalHolidayByNameAsync(It.Is<string>(name => string.IsNullOrEmpty(name))))
            .ReturnsAsync(ServiceResult<List<FestivalHolidayDto?>>.Fail("Invalid name"));
        // Act
        var result = await _FestivalHolidayController.CheckFestivalHolidayName("");
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetFestivalHolidayByNameAsync_ShouldFail_WhenNameIsWhitespace()
    {
        // Arrange
        _festivalHolidayServiceMock.Setup(service => service.GetFestivalHolidayByNameAsync(It.Is<string>(name => string.IsNullOrWhiteSpace(name))))
            .ReturnsAsync(ServiceResult<List<FestivalHolidayDto?>>.Fail("Invalid name"));
        // Act
        var result = await _FestivalHolidayController.CheckFestivalHolidayName("   ");
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }
}
