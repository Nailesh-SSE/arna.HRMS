using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.DTOs.Auth;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Dependency.Identity;
using arna.HRMS.Infrastructure.Mapping;
using arna.HRMS.Infrastructure.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Services;

[TestFixture]
public class AuthServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private AuthService _authService = null!;
    private IMapper _mapper = null!;
    private Mock<IJwtService> _jwtServiceMock = null!;
    private Mock<IUserServices> _userServicesMock = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserProfile>();
        });

        _mapper = mapperConfig.CreateMapper();

        _jwtServiceMock = new Mock<IJwtService>();
        _userServicesMock = new Mock<IUserServices>();

        var jwtSettings = Options.Create(new JwtSettings
        {
            AccessTokenExpirationInMinutes = 60
        });

        _authService = new AuthService(
            _jwtServiceMock.Object,
            _userServicesMock.Object,
            _mapper,
            jwtSettings,
            _dbContext);
    }

    [Test]
    public async Task LoginAsync_ShouldFail_WhenRequestIsNull()
    {
        var result = await _authService.LoginAsync(null!);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task LoginAsync_ShouldFail_WhenEmailIsEmpty()
    {
        var request = new LoginDto
        {
            UserName = "",
            Password = "123456"
        };

        var result = await _authService.LoginAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Email is required"));
    }

    [Test]
    public async Task LoginAsync_ShouldFail_WhenPasswordIsEmpty()
    {
        var request = new LoginDto
        {
            UserName = "test@test.com",
            Password = ""
        };

        var result = await _authService.LoginAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Password is required"));
    }

    [Test]
    public async Task LoginAsync_ShouldFail_WhenUserNotFound()
    {
        var request = new LoginDto
        {
            UserName = "test@test.com",
            Password = "123456"
        };

        _userServicesMock
            .Setup(x => x.GetUserByUserNameOrEmailAsync(request.UserName))
            .ReturnsAsync((User)null!);

        var result = await _authService.LoginAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid email or password"));
    }

    [Test]
    public async Task LoginAsync_ShouldFail_WhenPasswordIsIncorrect()
    {
        var request = new LoginDto
        {
            UserName = "test@test.com",
            Password = "wrongpassword"
        };

        var user = new User
        {
            Id = 1,
            Email = request.UserName,
            Password = "correctpassword"
        };

        _userServicesMock
            .Setup(x => x.GetUserByUserNameOrEmailAsync(request.UserName))
            .ReturnsAsync(user);

        var result = await _authService.LoginAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid email or password"));
    }

    [Test]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        var request = new LoginDto
        {
            UserName = "test@test.com",
            Password = "123456"
        };

        var user = new User
        {
            Id = 1,
            Email = request.UserName,
            Password = "123456",
            Username = "testuser",
            Role = new Role { Name = "Admin" }
        };

        _userServicesMock
            .Setup(x => x.GetUserByUserNameOrEmailAsync(request.UserName))
            .ReturnsAsync(user);

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("access_token");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        _jwtServiceMock
            .Setup(x => x.SaveRefreshTokenAsync(user.Id, "refresh_token"))
            .Returns(Task.CompletedTask);

        var result = await _authService.LoginAsync(request);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Login successful"));
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.AccessToken, Is.EqualTo("access_token"));
        Assert.That(result.Data.RefreshToken, Is.EqualTo("refresh_token"));
    }

    [Test]
    public async Task RegisterAsync_ShouldFail_WhenDtoIsNull()
    {
        var result = await _authService.RegisterAsync(null!);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task RegisterAsync_ShouldFail_WhenUsernameIsEmpty()
    {
        var dto = new UserDto
        {
            Username = "",
            Email = "test@test.com",
            Password = "123456"
        };

        var result = await _authService.RegisterAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Username is required"));
    }

    [Test]
    public async Task RegisterAsync_ShouldFail_WhenEmailIsEmpty()
    {
        var dto = new UserDto
        {
            Username = "test",
            Email = "",
            Password = "123456"
        };

        var result = await _authService.RegisterAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Email is required"));
    }

    [Test]
    public async Task RegisterAsync_ShouldFail_WhenPasswordIsEmpty()
    {
        var dto = new UserDto
        {
            Username = "test",
            Email = "test@test.com",
            Password = ""
        };

        var result = await _authService.RegisterAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Password is required"));
    }

    [Test]
    public async Task RegisterAsync_ShouldFail_WhenPasswordTooShort()
    {
        var dto = new UserDto
        {
            Username = "test",
            Email = "test@test.com",
            Password = "123"
        };

        var result = await _authService.RegisterAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Password must be at least 6 characters"));
    }

    [Test]
    public async Task RegisterAsync_ShouldFail_WhenUserAlreadyExists()
    {
        var dto = new UserDto
        {
            Id = 1,
            Username = "test",
            Email = "test@test.com",
            PhoneNumber = "9999999999",
            Password = "123456"
        };

        _userServicesMock
            .Setup(x => x.UserExistsAsync(dto.Email, dto.PhoneNumber, dto.Id))
            .ReturnsAsync(true);

        var result = await _authService.RegisterAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Email or Phone Number already exists"));
    }

    [Test]
    public async Task RegisterAsync_ShouldFail_WhenUserCreationFails()
    {
        var dto = new UserDto
        {
            Id = 1,
            Username = "test",
            Email = "test@test.com",
            PhoneNumber = "9999999999",
            Password = "123456"
        };

        _userServicesMock
            .Setup(x => x.UserExistsAsync(dto.Email, dto.PhoneNumber, dto.Id))
            .ReturnsAsync(false);

        _userServicesMock
            .Setup(x => x.CreateUserEntityAsync(It.IsAny<UserDto>()))
            .ReturnsAsync((User)null!);

        var result = await _authService.RegisterAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("User creation failed"));
    }

    [Test]
    public async Task RegisterAsync_ShouldReturnSuccess_WhenValidData()
    {
        var dto = new UserDto
        {
            Id = 1,
            Username = "testuser",
            Email = "test@test.com",
            PhoneNumber = "9999999999",
            Password = "123456",
            FirstName = "Test",
            LastName = "User"
        };

        var createdUser = new User
        {
            Id = 1,
            Username = dto.Username,
            Email = dto.Email,
            Password = dto.Password,
            Role = new Role { Name = "Admin" }
        };

        _userServicesMock
            .Setup(x => x.UserExistsAsync(dto.Email, dto.PhoneNumber, dto.Id))
            .ReturnsAsync(false);

        _userServicesMock
            .Setup(x => x.CreateUserEntityAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(createdUser);

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(createdUser))
            .Returns("access_token");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        _jwtServiceMock
            .Setup(x => x.SaveRefreshTokenAsync(createdUser.Id, "refresh_token"))
            .Returns(Task.CompletedTask);

        var result = await _authService.RegisterAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Registration successful"));
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.AccessToken, Is.EqualTo("access_token"));
        Assert.That(result.Data.RefreshToken, Is.EqualTo("refresh_token"));
    }

    [Test]
    public async Task RefreshTokenAsync_ShouldFail_WhenRequestIsNull()
    {
        var result = await _authService.RefreshTokenAsync(null!);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task RefreshTokenAsync_ShouldFail_WhenUserIdIsInvalid()
    {
        var request = new RefreshTokenDto
        {
            UserId = 0,
            RefreshToken = "token"
        };

        var result = await _authService.RefreshTokenAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid UserId"));
    }

    [Test]
    public async Task RefreshTokenAsync_ShouldFail_WhenRefreshTokenIsEmpty()
    {
        var request = new RefreshTokenDto
        {
            UserId = 1,
            RefreshToken = ""
        };

        var result = await _authService.RefreshTokenAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("RefreshToken is required"));
    }

    [Test]
    public async Task RefreshTokenAsync_ShouldFail_WhenUserNotFound()
    {
        var request = new RefreshTokenDto
        {
            UserId = 99,
            RefreshToken = "token"
        };

        var result = await _authService.RefreshTokenAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("User not found"));
    }

    [Test]
    public async Task RefreshTokenAsync_ShouldFail_WhenNoRefreshTokenStored()
    {
        var user = new User
        {
            Id = 1,
            RefreshToken = null,
            RefreshTokenExpiryTime = null,
            Email = "one@gmail.com",
            FirstName = "Test",
            LastName= "Last Test",
            Password= "Password", 
            PasswordHash = "Hashed pasword", 
            PhoneNumber= "1234567891",
            Username = "testuser",

        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var request = new RefreshTokenDto
        {
            UserId = 1,
            RefreshToken = "token"
        };

        var result = await _authService.RefreshTokenAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("No refresh token found. Please login again."));
    }

    [Test]
    public async Task RefreshTokenAsync_ShouldFail_WhenTokenDoesNotMatch()
    {
        var user = new User
        {
            Id = 1,
            RefreshToken = "correct_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(10),
            Email = "one@gmail.com",
            FirstName = "Test",
            LastName = "Last Test",
            Password = "Password",
            PasswordHash = "Hashed pasword",
            PhoneNumber = "1234567891",
            Username = "testuser",
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var request = new RefreshTokenDto
        {
            UserId = 1,
            RefreshToken = "wrong_token"
        };

        var result = await _authService.RefreshTokenAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid refresh token. Please login again."));
    }

    [Test]
    public async Task RefreshTokenAsync_ShouldFail_WhenTokenExpired()
    {
        var user = new User
        {
            Id = 1,
            RefreshToken = "token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-5),
            Email = "one@gmail.com",
            FirstName = "Test",
            LastName = "Last Test",
            Password = "Password",
            PasswordHash = "Hashed pasword",
            PhoneNumber = "1234567891",
            Username = "testuser",
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var request = new RefreshTokenDto
        {
            UserId = 1,
            RefreshToken = "token"
        };

        var result = await _authService.RefreshTokenAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Refresh token has expired. Please login again."));
    }

    [Test]
    public async Task RefreshTokenAsync_ShouldReturnSuccess_WhenTokenIsValid()
    {
        var user = new User
        {
            Id = 1,
            Email = "one@gmail.com",
            FirstName = "Test",
            LastName = "Last Test",
            Password = "Password",
            PasswordHash = "Hashed pasword",
            PhoneNumber = "1234567891",
            Username = "testuser",
            RefreshToken = "valid_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(10),
            Role = new Role { Name = "Admin" }
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("new_access_token");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("new_refresh_token");

        _jwtServiceMock
            .Setup(x => x.SaveRefreshTokenAsync(user.Id, "new_refresh_token"))
            .Returns(Task.CompletedTask);

        var request = new RefreshTokenDto
        {
            UserId = 1,
            RefreshToken = "valid_token"
        };

        var result = await _authService.RefreshTokenAsync(request);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Token refreshed successfully"));
        Assert.That(result.Data.AccessToken, Is.EqualTo("new_access_token"));
        Assert.That(result.Data.RefreshToken, Is.EqualTo("new_refresh_token"));
    }

    [Test]
    public async Task LogoutAsync_ShouldFail_WhenUserIdIsInvalid()
    {
        var result = await _authService.LogoutAsync(0);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid UserId"));

        // Ensure JWT service was NOT called
        _jwtServiceMock.Verify(
            x => x.RevokeRefreshTokenAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Test]
    public async Task LogoutAsync_ShouldReturnSuccess_WhenUserIdIsValid()
    {
        int userId = 1;

        _jwtServiceMock
            .Setup(x => x.RevokeRefreshTokenAsync(userId))
            .Returns(Task.CompletedTask);

        var result = await _authService.LogoutAsync(userId);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Logged out successfully"));
        Assert.That(result.Data, Is.True);

        // Verify JWT revoke was called once
        _jwtServiceMock.Verify(
            x => x.RevokeRefreshTokenAsync(userId),
            Times.Once);
    }

    [Test]
    public async Task GenerateAuthResponseAsync_ShouldReturnValidAuthResponse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@test.com",
            EmployeeId = 10,
            Role = new Role { Name = "Admin" }
        };

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("access_token");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        _jwtServiceMock
            .Setup(x => x.SaveRefreshTokenAsync(user.Id, "refresh_token"))
            .Returns(Task.CompletedTask);

        var method = typeof(AuthService)
            .GetMethod("GenerateAuthResponseAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var task = (Task<AuthResponse>)method!
            .Invoke(_authService, new object[] { user, "Success message" })!;

        var result = await task;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Success message"));
        Assert.That(result.AccessToken, Is.EqualTo("access_token"));
        Assert.That(result.RefreshToken, Is.EqualTo("refresh_token"));
        Assert.That(result.UserId, Is.EqualTo(user.Id));
        Assert.That(result.Username, Is.EqualTo(user.Username));
        Assert.That(result.FullName, Is.EqualTo(user.FullName));
        Assert.That(result.Email, Is.EqualTo(user.Email));
        Assert.That(result.EmployeeId, Is.EqualTo(user.EmployeeId));
        Assert.That(result.Role, Is.EqualTo("Admin"));

        _jwtServiceMock.Verify(x => x.GenerateAccessToken(user), Times.Once);
        _jwtServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
        _jwtServiceMock.Verify(x => x.SaveRefreshTokenAsync(user.Id, "refresh_token"), Times.Once);
    }

    [Test]
    public async Task GenerateAuthResponseAsync_ShouldHandleNullValues()
    {
        var user = new User
        {
            Id = 1,
            Username = null,
            Email = null,
            Role = null
        };

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("access_token");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        _jwtServiceMock
            .Setup(x => x.SaveRefreshTokenAsync(user.Id, "refresh_token"))
            .Returns(Task.CompletedTask);

        var method = typeof(AuthService)
            .GetMethod("GenerateAuthResponseAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var task = (Task<AuthResponse>)method!
            .Invoke(_authService, new object[] { user, "Success" })!;

        var result = await task;

        Assert.That(result.Username, Is.EqualTo(string.Empty));
        Assert.That(result.Role, Is.EqualTo(string.Empty));
    }
}
