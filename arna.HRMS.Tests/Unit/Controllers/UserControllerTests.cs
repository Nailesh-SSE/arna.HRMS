using arna.HRMS.API.Controllers.Admin;
using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Controllers;

[TestFixture]
public class UserControllerTests
{
    private Mock<IUserServices> _mockUserServices = null!;
    private UsersController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mockUserServices = new Mock<IUserServices>();
        _controller = new UsersController(_mockUserServices.Object);
    }

    [Test]
    public async Task GetUsers_ReturnsOkResult()
    {
        var users = new List<UserDto>
        {
            new UserDto { Id = 1, FullName = "John Doe", Email = "john@gmail.com", PhoneNumber = "1234567890" },
            new UserDto { Id = 2, FullName = "Jane Smith", Email = "jane@gmail.com", PhoneNumber = "0987654321" }
        };
        // Arrange
        _mockUserServices
            .Setup(s => s.GetUserAsync())
            .ReturnsAsync(ServiceResult<List<UserDto>>.Success(users));
        // Act
        var result = await _controller.GetUsers();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetUsers_ShouldSuccess_WhenNoDataFound()
    {
        _mockUserServices
            .Setup(s => s.GetUserAsync())
            .ReturnsAsync(ServiceResult<List<UserDto>>.Success(null!));
        // Act
        var result = await _controller.GetUsers();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetUserById_ReturnsOkResult_WhenUserExists()
    {
        var users = new List<UserDto>
        {
            new UserDto { Id = 1, FullName = "John Doe", Email = "john@gmail.com", PhoneNumber = "1234567890" },
            new UserDto { Id = 2, FullName = "Jane Smith", Email = "jane@gmail.com", PhoneNumber = "0987654321" }
        };
        _mockUserServices
           .Setup(s => s.GetUserByIdAsync(1))
           .ReturnsAsync(ServiceResult<UserDto?>.Success(users.FirstOrDefault(u => u.Id == 1)));

        // Act
        var result = await _controller.GetUserById(1);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetUserById_ReturnsNotFoundResult_WhenUserDoesNotExist()
    {
        _mockUserServices
            .Setup(s => s.GetUserByIdAsync(1))
            .ReturnsAsync(ServiceResult<UserDto?>.Fail("Notfound"));
        // Act
        var result = await _controller.GetUserById(1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetUserById_ReturnsBadRequest_WhenIdIsZeroOrNegative()
    {
        _mockUserServices
            .Setup(s => s.GetUserByIdAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<UserDto?>.Fail("Invalid User ID"));

        // Act
        var result = await _controller.GetUserById(0);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

        // Act
        result = await _controller.GetUserById(-1);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

    }

    [Test]
    public async Task CreateUser_ReturnsOkResult_WhenUserIsCreated()
    {
        var users = new UserDto { Id = 2, FullName = "Jane Smith", Email = "jane@gmail.com", PhoneNumber = "0987654321" };

        _mockUserServices
            .Setup(s => s.CreateUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(ServiceResult<UserDto>.Success(users, "User created successfully"));

        // Act
        var result = await _controller.CreateUser(users);

        // Assert

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task CreateUser_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Email", "Email is required");
        // Act
        var result = await _controller.CreateUser(new UserDto());
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateUser_ReturnsBadRequest_WhenServiceFails()
    {
        var users = new UserDto { Id = 2, FullName = "Jane Smith", Email = "jane@gmail.com", PhoneNumber = "0987654321" };

        _mockUserServices
            .Setup(s => s.CreateUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(ServiceResult<UserDto>.Fail("Failed to create user"));

        // Act
        var result = await _controller.CreateUser(users);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateUser_ReturnSuccess_WhenDataFound()
    {
        var users = new UserDto { Id = 2, FullName = "Jane Smith", Email = "jane@gmail.com", PhoneNumber = "0987654321" };
        _mockUserServices
            .Setup(s => s.UpdateUserAsync(users))
            .ReturnsAsync(ServiceResult<UserDto>.Success(users, "User updated successfully"));
        var result = await _controller.UpdateUser(2, users);
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateUser_ReturnBadRequest_WhenIdMismatch()
    {
        var users = new UserDto { Id = 2, FullName = "Jane Smith", Email = "jane@gmail.com", PhoneNumber = "0987654321" };
        _mockUserServices
            .Setup(s => s.UpdateUserAsync(users))
            .ReturnsAsync(ServiceResult<UserDto>.Success(users, "User updated successfully"));
        var result = await _controller.UpdateUser(3, users);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateUser_ReturnBadRequest_WhenModelStateIsInvalid()
    {
        var users = new UserDto { Id = 2, FullName = "Jane Smith", PhoneNumber = "0987654321" };
        _controller.ModelState.AddModelError("Email", "Email is required");
        _mockUserServices
            .Setup(s => s.UpdateUserAsync(users))
            .ReturnsAsync(ServiceResult<UserDto>.Success(users, "User updated successfully"));
        var result = await _controller.UpdateUser(2, users);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateUser_ReturnBadRequest_WhenServiceFails()
    {
        var users = new UserDto { Id = 2, FullName = "Jane Smith", Email = "jane@gmail.com", PhoneNumber = "0987654321" };
        _mockUserServices
            .Setup(s => s.UpdateUserAsync(users))
            .ReturnsAsync(ServiceResult<UserDto>.Fail("Failed to update user"));

        var result = await _controller.UpdateUser(2, users);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeleteUser_ReturnsOkResult_WhenUserIsDeleted()
    {
        var users = new UserDto { Id = 1, FullName = "Jane Smith", Email = "jane@gmail.com", PhoneNumber = "0987654321" };
        _mockUserServices
            .Setup(s => s.DeleteUserAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Success(true, "User deleted successfully"));
        var result = await _controller.DeleteUser(1);
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task DeleteUser_ReturnsBadRequest_WhenServiceFails()
    {
        _mockUserServices
            .Setup(s => s.DeleteUserAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Fail("Failed to delete user"));
        var result = await _controller.DeleteUser(1);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteUser_ReturnsBadRequest_WhenUserNotFound()
    {
        new UserDto { Id = 2, FullName = "Jane Smith", Email = "jane@gmail.com", PhoneNumber = "0987654321" };
        _mockUserServices
            .Setup(s => s.DeleteUserAsync(11))
            .ReturnsAsync(ServiceResult<bool>.Fail("User not found"));
        var result = await _controller.DeleteUser(11);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteUser_ReturnsBadRequest_WhenIdIsZeroOrNegative()
    {
        _mockUserServices
            .Setup(s => s.DeleteUserAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<bool>.Fail("Invalid User ID"));
        var result = await _controller.DeleteUser(0);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        result = await _controller.DeleteUser(-1);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task ChangeUserPassword_ShouldSuccess_WhenDataNotFound()
    {
        var users = new UserDto { Id = 2, FullName = "Jane Smith", Email = "jane@gmail.com", PhoneNumber = "0987654321" };
        _mockUserServices
            .Setup(s => s.ChangeUserPasswordAsync(2, "newpassword"))
            .ReturnsAsync(ServiceResult<bool>.Success(true, "Password changed successfully"));

        var result = await _controller.ChangeUserPassword(2, "newpassword");
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public  async Task ChangeUserPassword_ShouldFail_WhenServiceFails()
    {
        _mockUserServices
            .Setup(s => s.ChangeUserPasswordAsync(2, "newpassword"))
            .ReturnsAsync(ServiceResult<bool>.Fail("Failed to change password"));
        var result = await _controller.ChangeUserPassword(2, "newpassword");
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ChangeUserPassword_ShouldFail_WhenIdIsZeroOrNegative()
    {
        _mockUserServices
            .Setup(s => s.ChangeUserPasswordAsync(It.Is<int>(id => id <= 0), "newpassword"))
            .ReturnsAsync(ServiceResult<bool>.Fail("Invalid User ID"));
        var result = await _controller.ChangeUserPassword(0, "newpassword");
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        result = await _controller.ChangeUserPassword(-1, "newpassword");
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ChangeUserPassword_ShouldFail_WhenNewPasswordIsInvalid()
    {
        _mockUserServices
            .Setup(s => s.ChangeUserPasswordAsync(2, "short"))
            .ReturnsAsync(ServiceResult<bool>.Fail("Invalid new password"));
        var result = await _controller.ChangeUserPassword(2, "short");
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ChangeUserPassword_ShouldFail_WhenUserNotFound()
    {
        _mockUserServices
            .Setup(s => s.ChangeUserPasswordAsync(11, "newpassword"))
            .ReturnsAsync(ServiceResult<bool>.Fail("User not found"));
        var result = await _controller.ChangeUserPassword(11, "newpassword");
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }
}
