using arna.HRMS.API.Controllers.Admin;
using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Controllers;

[TestFixture]
public class EmployeesControllerTests
{
    private Mock<IEmployeeService> _serviceMock = null!;
    private EmployeesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IEmployeeService>();
        _controller = new EmployeesController(_serviceMock.Object);
    }

    [Test]
    public async Task GetEmployees_ShouldOk_WhenDataFound()
    {
        // Arrange
        var list = new List<EmployeeDto>
        {
            new EmployeeDto { Id = 1, FirstName = "John", LastName = "Doe" },
            new EmployeeDto { Id = 2, FirstName = "Jane", LastName = "Smith" }
        };

        _serviceMock
            .Setup(s => s.GetEmployeesAsync())
            .ReturnsAsync(ServiceResult<List<EmployeeDto>>.Success(list));

        // Act
        var result = await _controller.GetEmployees();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetEmployee_ShouldNotFound_WhenDataNotFound()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetEmployeesAsync())
            .ReturnsAsync(ServiceResult<List<EmployeeDto>>.Success(null!,"No data found"));
        // Act
        var result = await _controller.GetEmployees();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetEmployeeById_ShouldOk_WhenDataFound()
    {
        // Arrange
        var employee = new EmployeeDto { Id = 1, FirstName = "John", LastName = "Doe" };
        _serviceMock
            .Setup(s => s.GetEmployeeByIdAsync(1))
            .ReturnsAsync(ServiceResult<EmployeeDto?>.Success(employee));
        // Act
        var result = await _controller.GetEmployeeById(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetEmployeeById_ShouldNotFound_WhenDataNotFound()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetEmployeeByIdAsync(1))
            .ReturnsAsync(ServiceResult<EmployeeDto?>.Fail("No data found"));
        // Act
        var result = await _controller.GetEmployeeById(1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetEmployeeById_ShouldFail_WhenIdIsZero()
    {
        int employeeid = 0;

        _serviceMock.Setup(s => s.GetEmployeeByIdAsync(employeeid))
            .ReturnsAsync(ServiceResult<EmployeeDto?>.Fail("NoData Found"));

        var result = await _controller.GetEmployeeById(employeeid);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetEmployeeById_ShouldFail_WhenIdIsNegative()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetEmployeeByIdAsync(-8))
            .ReturnsAsync(ServiceResult<EmployeeDto?>.Fail("No DataFound"));

        // Act
        var result = await _controller.GetEmployeeById(-8);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreateEmployee_ShouldOk_WhenDataIsValid()
    {
        // Arrange
        var employeeDto = new EmployeeDto { Id = 1, FirstName = "John", LastName = "Doe" };
        _serviceMock
            .Setup(s => s.CreateEmployeeAsync(employeeDto))
            .ReturnsAsync(ServiceResult<EmployeeDto>.Success(employeeDto));
        // Act
        var result = await _controller.CreateEmployee(employeeDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task CreateEmployee_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        var dto = new EmployeeDto();

        _controller.ModelState.AddModelError("FirstName", "Required");

        var result = await _controller.CreateEmployee(dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateEmployee_ShouldReturnBadRequest_WhenIdIsProvided()
    {
        var dto = new EmployeeDto { Id = 1, FirstName = "John", LastName = "Doe" };
        _serviceMock
            .Setup(s => s.CreateEmployeeAsync(dto))
            .ReturnsAsync(ServiceResult<EmployeeDto>.Fail("Fail to Create Employee"));
        var result = await _controller.CreateEmployee(dto);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That((result as BadRequestObjectResult)?.Value, Is.EqualTo("Fail to Create Employee"));
    }

    [Test]
    public async Task UpdateEmployee_ShouldOk_WhenDataIsValid()
    {
        // Arrange
        var employeeDto = new EmployeeDto { Id = 1, FirstName = "John", LastName = "Doe" };
        _serviceMock
            .Setup(s => s.UpdateEmployeeAsync(employeeDto))
            .ReturnsAsync(ServiceResult<EmployeeDto>.Success(employeeDto));
        // Act
        var result = await _controller.UpdateEmployee(1, employeeDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateEmployee_ShouldOverrideDtoId_WithRouteId()
    {
        var dto = new EmployeeDto { Id = 99, FirstName = "John", LastName = "Doe" };

        _serviceMock
            .Setup(s => s.UpdateEmployeeAsync(It.IsAny<EmployeeDto>()))
            .ReturnsAsync(ServiceResult<EmployeeDto>.Success(dto));

        var result = await _controller.UpdateEmployee(5, dto);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That((result as BadRequestObjectResult)?.Value, Is.EqualTo("Invalid Id"));

    }

    [Test]
    public async Task UpdateEmployee_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        var dto = new EmployeeDto();
        dto.Id = 1;

        _controller.ModelState.AddModelError("FirstName", "Required");

        var result = await _controller.UpdateEmployee(1,dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateEmployee_ShouldReturnBadRequest_WhenIdMismatch()
    {
        var dto = new EmployeeDto { Id = 1, FirstName = "John", LastName = "Doe" };
        _serviceMock
            .Setup(s => s.UpdateEmployeeAsync(dto))
            .ReturnsAsync(ServiceResult<EmployeeDto>.Fail("Fail to Update Employee"));
        var result = await _controller.UpdateEmployee(2, dto);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateEmployee_ReturnsBadRequestResult_WhenIdIsZeroOrNegative()
    {
        var dto = new EmployeeDto { Id = 1, FirstName = "John", LastName = "Doe" };
        _serviceMock
            .Setup(s => s.UpdateEmployeeAsync(dto))
            .ReturnsAsync(ServiceResult<EmployeeDto>.Fail("invalid Id"));

        var result1 = await _controller.UpdateEmployee(0, dto);
        var result2 = await _controller.UpdateEmployee(-1, dto);

        Assert.That(result1, Is.TypeOf<BadRequestObjectResult>());
        Assert.That(result2, Is.TypeOf<BadRequestObjectResult>());

    }

    [Test]
    public async Task DeleteEmployee_ShouldSuccess_WhenDataFound()
    {
        var employeeDto = new EmployeeDto { Id = 1, FirstName = "John", LastName = "Doe" };
        _serviceMock.Setup(s=>s.DeleteEmployeeAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Success(true));
        
        var result = await _controller.DeleteEmployee(1);

        Assert.That(result, Is.TypeOf<OkObjectResult>());

    }

    [Test]
    public async Task DeleteEmployee_ShouldFail_WhenDataNotFound()
    {
        _serviceMock.Setup(s => s.DeleteEmployeeAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Fail("No Data Found"));

        var result=await _controller.DeleteEmployee(1);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteEmployee_ShouldFail_WhenIdIsZero()
    {
        _serviceMock.Setup(s => s.DeleteEmployeeAsync(0))
            .ReturnsAsync(ServiceResult<bool>.Fail("NoDat Found"));
        var result =await _controller.DeleteEmployee(0);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteEmployee_ShouldFail_WhenIdIsNegative()
    {
        _serviceMock.Setup(s => s.DeleteEmployeeAsync(-1))
            .ReturnsAsync(ServiceResult<bool>.Fail("NoDat Found"));
        var result = await _controller.DeleteEmployee(-1);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }
}
