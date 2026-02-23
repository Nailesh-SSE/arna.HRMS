using arna.HRMS.API.Controllers.Admin;
using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Controllers;

[TestFixture]
public class RoleControllerTests
{
    private Mock<IRoleService> _mockRoleServices = null!;
    private RoleController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRoleServices = new Mock<IRoleService>();
        _controller = new RoleController(_mockRoleServices.Object);
    }

    [Test]
    public async Task GetRoles_ReturnsOkResult()
    {
        var Data = new List<Core.DTOs.RoleDto>
            {
                new RoleDto { Id = 1, Name = "Admin" },
                new RoleDto { Id = 2, Name = "User" }
            };
        // Arrange
        _mockRoleServices.Setup(service => service.GetRoleAsync())
            .ReturnsAsync(ServiceResult<List<RoleDto>>.Success(Data));
        // Act
        var result = await _controller.GetRoles();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetRoles_ShouldSuccess_WhenNoDataFound()
    {
        // Arrange
        _mockRoleServices.Setup(service => service.GetRoleAsync())
            .ReturnsAsync(ServiceResult<List<RoleDto>>.Success(new List<RoleDto>()));
        // Act
        var result = await _controller.GetRoles();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetRoleById_ReturnsOkResult_WhenRoleExists()
    {
        // Arrange
        var roleId = 1;
        var role = new RoleDto { Id = roleId, Name = "Admin" };
        _mockRoleServices.Setup(service => service.GetRoleByIdAsync(roleId))
            .ReturnsAsync(ServiceResult<RoleDto?>.Success(role));
        // Act
        var result = await _controller.GetRoleById(roleId);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetRoleById_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        var roleId = 1;
        _mockRoleServices.Setup(service => service.GetRoleByIdAsync(roleId))
            .ReturnsAsync(ServiceResult<RoleDto?>.Fail("No data Found"));
        // Act
        var result = await _controller.GetRoleById(roleId);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetRoleById_ShouldFail_WhenIdIsZeroOrNegative()
    {
        // Arrange
        var roleId = -1;
        _mockRoleServices.Setup(service => service.GetRoleByIdAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<RoleDto?>.Fail("Invalid Id"));
        // Act
        var result = await _controller.GetRoleById(roleId);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

        //Act with zero Id
        roleId = 0;
        result = await _controller.GetRoleById(roleId);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

    }

    [Test]
    public async Task CreateRole_ReturnsOkResult_WhenRoleIsCreated()
    {
        // Arrange
        var roleDto = new RoleDto { Id = 1, Name = "Admin" };
        _mockRoleServices.Setup(service => service.CreateRoleAsync(roleDto))
            .ReturnsAsync(ServiceResult<RoleDto>.Success(roleDto));
        // Act
        var result = await _controller.CreateRole(roleDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task CreateRole_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var roleDto = new RoleDto { Id = 1 };
        _controller.ModelState.AddModelError("Name", "Name is required");
        // Act
        var result = await _controller.CreateRole(roleDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateRole_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        var roleDto = new RoleDto { Id = 1, Name = "Admin" };
        _mockRoleServices.Setup(service => service.CreateRoleAsync(roleDto))
            .ReturnsAsync(ServiceResult<RoleDto>.Fail("Failed to create role"));
        // Act
        var result = await _controller.CreateRole(roleDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateRole_ReturnsOkResult_WhenRoleIsUpdated()
    {
        // Arrange
        var roleId = 1;
        var roleDto = new RoleDto { Id = roleId, Name = "Admin" };
        _mockRoleServices.Setup(service => service.UpdateRoleAsync(roleDto))
            .ReturnsAsync(ServiceResult<RoleDto>.Success(roleDto));
        // Act
        var result = await _controller.UpdateRole(roleId, roleDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateRole_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var roleId = 1;
        var roleDto = new RoleDto { Id = roleId };
        _controller.ModelState.AddModelError("Name", "Name is required");
        // Act
        var result = await _controller.UpdateRole(roleId, roleDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateRole_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        var roleId = 1;
        var roleDto = new RoleDto { Id = 2, Name = "Admin" };
        // Act
        var result = await _controller.UpdateRole(roleId, roleDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateRole_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        var roleId = 1;
        var roleDto = new RoleDto { Id = roleId, Name = "Admin" };
        _mockRoleServices.Setup(service => service.UpdateRoleAsync(roleDto))
            .ReturnsAsync(ServiceResult<RoleDto>.Fail("Failed to update role"));
        // Act
        var result = await _controller.UpdateRole(roleId, roleDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeleteRole_ReturnsOkResult_WhenRoleIsDeleted()
    {
        // Arrange
        var roleId = 1;
        _mockRoleServices.Setup(service => service.DeleteRoleAsync(roleId))
            .ReturnsAsync(ServiceResult<bool>.Success(true));
        // Act
        var result = await _controller.DeleteRole(roleId);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task DeleteRole_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        var roleId = 1;
        _mockRoleServices.Setup(service => service.DeleteRoleAsync(roleId))
            .ReturnsAsync(ServiceResult<bool>.Fail("No data Found"));
        // Act
        var result = await _controller.DeleteRole(roleId);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteRole_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        var roleId = 1;
        _mockRoleServices.Setup(service => service.DeleteRoleAsync(roleId))
            .ReturnsAsync(ServiceResult<bool>.Fail("Failed to delete role"));
        // Act
        var result = await _controller.DeleteRole(roleId);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteRole_ReturnsBadRequest_WhenIdIsZeroOrNegative()
    {
        // Arrange
        var roleId = -1;
        _mockRoleServices.Setup(service => service.DeleteRoleAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<bool>.Fail("Invalid Id"));
        // Act
        var result = await _controller.DeleteRole(roleId);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        //Act with zero Id
        roleId = 0;
        result = await _controller.DeleteRole(roleId);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }
}
