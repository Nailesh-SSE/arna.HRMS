using arna.HRMS.API.Controllers.Admin;
using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Controllers;

[TestFixture]
public class DepartmentControllerTests
{
    private Mock<IDepartmentService> _departmentServiceMock = null!;
    private DepartmentController _departmentController = null!;

    [SetUp]
    public void SetUp()
    {
        _departmentServiceMock = new Mock<IDepartmentService>();
        _departmentController = new DepartmentController(_departmentServiceMock.Object);
    }

    [Test]
    public async Task GetDepartment_ReturnsOkResult()
    {
        var list = new List<DepartmentDto>()
        {
            new DepartmentDto(){ Id = 1, Name = "IT" },
            new DepartmentDto(){ Id = 2, Name = "HR" }
        };
        // Arrange
        _departmentServiceMock.Setup(service => service.GetDepartmentAsync())
            .ReturnsAsync(ServiceResult<List<DepartmentDto>>.Success(list));
        // Act
        var result = await _departmentController.GetDepartment();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetDepartment_ShoulsSuccess_WhenNoDepartment()
    {
        // Arrange
        _departmentServiceMock.Setup(service => service.GetDepartmentAsync())
            .ReturnsAsync(ServiceResult<List<DepartmentDto>>.Success(new List<DepartmentDto>()));
        // Act
        var result = await _departmentController.GetDepartment();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetDepartmentById_ReturnsOkResult()
    {
        var department = new DepartmentDto() { Id = 1, Name = "IT" };
        // Arrange
        _departmentServiceMock.Setup(service => service.GetDepartmentByIdAsync(1))
            .ReturnsAsync(ServiceResult<DepartmentDto?>.Success(department));
        // Act
        var result = await _departmentController.GetDepartmentById(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetDepartmentById_ReturnsNotFoundResult()
    {
        // Arrange
        _departmentServiceMock.Setup(service => service.GetDepartmentByIdAsync(1))
            .ReturnsAsync(ServiceResult<DepartmentDto?>.Fail("Department not found"));
        // Act
        var result = await _departmentController.GetDepartmentById(1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        Assert.That((result as NotFoundObjectResult)?.Value, Is.EqualTo("Department not found"));
    }

    [Test]
    public async Task GetDepartmentById_ShouldFail_WhenDepartmentIdIsZeroOrNegative()
    {
        // Arrange
        _departmentServiceMock.Setup(service => service.GetDepartmentByIdAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<DepartmentDto?>.Fail("Invalid department ID"));
        // Act
        var result = await _departmentController.GetDepartmentById(0);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        Assert.That((result as NotFoundObjectResult)?.Value, Is.EqualTo("Invalid department ID"));

        // Act
        result = await _departmentController.GetDepartmentById(-1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        Assert.That((result as NotFoundObjectResult)?.Value, Is.EqualTo("Invalid department ID"));
    }

    [Test]
    public async Task CreateDepartment_ReturnsOkResult()
    {
        var departmentDto = new DepartmentDto() { Name = "IT" };
        var createdDepartment = new DepartmentDto() { Id = 1, Name = "IT" };
        // Arrange
        _departmentServiceMock.Setup(service => service.CreateDepartmentAsync(departmentDto))
            .ReturnsAsync(ServiceResult<DepartmentDto>.Success(createdDepartment));
        // Act
        var result = await _departmentController.CreateDepartment(departmentDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task CreateDepartment_ReturnsBadRequestResult()
    {
        var departmentDto = new DepartmentDto() { Name = "IT" };
        // Arrange
        _departmentServiceMock.Setup(service => service.CreateDepartmentAsync(departmentDto))
            .ReturnsAsync(ServiceResult<DepartmentDto>.Fail("Failed to create department"));
        // Act
        var result = await _departmentController.CreateDepartment(departmentDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That((result as BadRequestObjectResult)?.Value, Is.EqualTo("Failed to create department"));
    }

    [Test]
    public async Task CreateDepartment_ReturnsBadRequestResult_WhenModelStateIsInvalid()
    {
        var departmentDto = new DepartmentDto() { Name = "" }; // Invalid name
        _departmentController.ModelState.AddModelError("Name", "Name is required");
        // Act
        var result = await _departmentController.CreateDepartment(departmentDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.TypeOf<SerializableError>());
        var errors = badRequestResult?.Value as SerializableError;
        Assert.That(errors?["Name"], Is.Not.Null);
    }

    [Test]
    public async Task UpdateDepartment_ReturnsOkResult()
    {
        var departmentDto = new DepartmentDto() { Id = 1, Name = "IT" };
        // Arrange
        _departmentServiceMock.Setup(service => service.UpdateDepartmentAsync(departmentDto))
            .ReturnsAsync(ServiceResult<DepartmentDto>.Success(departmentDto));
        // Act
        var result = await _departmentController.UpdateDepartment(1, departmentDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateDepartment_ReturnsBadRequestResult()
    {
        var departmentDto = new DepartmentDto() { Id = 1, Name = "IT" };
        // Arrange
        _departmentServiceMock.Setup(service => service.UpdateDepartmentAsync(departmentDto))
            .ReturnsAsync(ServiceResult<DepartmentDto>.Fail("Failed to update department"));
        // Act
        var result = await _departmentController.UpdateDepartment(1, departmentDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That((result as BadRequestObjectResult)?.Value, Is.EqualTo("Failed to update department"));
    }
    
    [Test]
    public async Task UpdateDepartment_ReturnsBadRequestResult_WhenModelStateIsInvalid()
    {
        var departmentDto = new DepartmentDto() { Id = 1, Name = "" }; // Invalid name
        _departmentController.ModelState.AddModelError("Name", "Name is required");
        // Act
        var result = await _departmentController.UpdateDepartment(1, departmentDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.TypeOf<SerializableError>());
        var errors = badRequestResult?.Value as SerializableError;
        Assert.That(errors?["Name"], Is.Not.Null);
    }

    [Test]
    public async Task UpdateDepartment_ReturnsBadRequestResult_WhenIdMismatch()
    {
        var departmentDto = new DepartmentDto() { Id = 1, Name = "IT" };

        _departmentServiceMock
            .Setup(service => service.UpdateDepartmentAsync(departmentDto))
            .ReturnsAsync(ServiceResult<DepartmentDto>.Fail("Failed to update department"));

        // Act
        var result = await _departmentController.UpdateDepartment(2, departmentDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That((result as BadRequestObjectResult)?.Value, Is.EqualTo("Failed to update department"));
    }

    [Test]
    public async Task DeleteDepartment_ReturnsOkResult()
    {
        var departmentDto = new DepartmentDto() { Id = 1, Name = "IT" };
        // Arrange
        _departmentServiceMock.Setup(service => service.DeleteDepartmentAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Success(true));
        // Act
        var result = await _departmentController.DeleteDepartment(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task DeleteDepartment_ReturnsBadRequestResult()
    {
        // Arrange
        _departmentServiceMock
            .Setup(service => service.DeleteDepartmentAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Fail("Failed to delete department"));
        // Act
        var result = await _departmentController.DeleteDepartment(1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        Assert.That((result as NotFoundObjectResult)?.Value, Is.EqualTo("Failed to delete department"));
    }

    [Test]
    public async Task DeleteDepartment_ReturnsBadRequestResult_WhenDepartmentIdIsZeroOrNegative()
    {
        // Arrange
        _departmentServiceMock.Setup(service => service.DeleteDepartmentAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<bool>.Fail("Invalid department ID"));
        // Act
        var result = await _departmentController.DeleteDepartment(0);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        Assert.That((result as NotFoundObjectResult)?.Value, Is.EqualTo("Invalid department ID"));
        // Act
        result = await _departmentController.DeleteDepartment(-1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        Assert.That((result as NotFoundObjectResult)?.Value, Is.EqualTo("Invalid department ID"));
    }

    [Test]
    public async Task DeleteDepartment_ReturnsNotFoundResult_WhenDepartmentDoesNotExist()
    {
        var data = new DepartmentDto() { Id = 2, Name = "IT" };
        // Arrange
        _departmentServiceMock.Setup(service => service.DeleteDepartmentAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Fail("Department not found"));
        // Act
        var result = await _departmentController.DeleteDepartment(1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        Assert.That((result as NotFoundObjectResult)?.Value, Is.EqualTo("Department not found"));
    }

}
