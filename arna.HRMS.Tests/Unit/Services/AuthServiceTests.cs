using arna.HRMS.Core.Common.Token;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Configuration;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Mapping;
using arna.HRMS.Infrastructure.Services.Authentication;
using arna.HRMS.Infrastructure.Services.Authentication.Interfaces;
using arna.HRMS.Infrastructure.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity.Data;
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
        var request = new LoginRequest
        {
            Email = "",
            Password = "123456"
        };

        var result = await _authService.LoginAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Email is required"));
    }

    [Test]
    public async Task LoginAsync_ShouldFail_WhenPasswordIsEmpty()
    {
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = ""
        };

        var result = await _authService.LoginAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Password is required"));
    }

    [Test]
    public async Task LoginAsync_ShouldFail_WhenUserNotFound()
    {
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "123456"
        };

        _userServicesMock
            .Setup(x => x.GetUserByUserNameAndEmail(request.Email))
            .ReturnsAsync((User)null!);

        var result = await _authService.LoginAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid email or password"));
    }

    [Test]
    public async Task LoginAsync_ShouldFail_WhenPasswordIsIncorrect()
    {
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "wrongpassword"
        };

        var user = new User
        {
            Id = 1,
            Email = request.Email,
            Password = "correctpassword"
        };

        _userServicesMock
            .Setup(x => x.GetUserByUserNameAndEmail(request.Email))
            .ReturnsAsync(user);

        var result = await _authService.LoginAsync(request);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid email or password"));
    }

    [Test]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "123456"
        };

        var user = new User
        {
            Id = 1,
            Email = request.Email,
            Password = "123456",
            Username = "testuser",
            Role = new Role { Name = "Admin" }
        };

        _userServicesMock
            .Setup(x => x.GetUserByUserNameAndEmail(request.Email))
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
}
