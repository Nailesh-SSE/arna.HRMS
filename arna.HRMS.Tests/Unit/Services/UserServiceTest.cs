using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Mapping;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Models.ViewModels.Attendance;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Services;

[TestFixture]
public class UserServiceTest
{
    private ApplicationDbContext _dbContext = null!;
    private IUserServices _userService = null!;
    private IMapper _mapper=null!;

    [SetUp]
    public void Setup()
    {

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var requestBaseRepo = new BaseRepository<User>(_dbContext);
        var UserRepository = new UserRepository(requestBaseRepo);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserProfile>();
        }); 
        _mapper = config.CreateMapper();
        _userService = new UserServices(
            UserRepository,
            _mapper
        );
    }
    [Test]
    public async Task GetUserByIdAsync_InvalidId_ReturnsFailResult()
    {
        // Arrange
        int invalidUserId = -1;
        // Act
        var result = await _userService.GetUserByIdAsync(invalidUserId);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid User ID"));
    }

    [Test]
    public async Task GetUserByIdAsync_NonExistentId_ReturnsFailResult()
    {
        // Arrange
        int nonExistentUserId = 999;
        // Act
        var result = await _userService.GetUserByIdAsync(nonExistentUserId);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("User not found"));
    }

    [Test]
    public async Task GetUserByIdAsync_WhenFound_ReturnsSuccessResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);

        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@user.com",
            PasswordHash = "hashedpassword",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "123456789",
            RoleId = 1,
            Role = role,
            Password = "password",
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userService.GetUserByIdAsync(1);
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(1));
        Assert.That(result.Data!.Username, Is.EqualTo("testuser"));
        Assert.That(result.Data!.Email, Is.EqualTo("test@user.com"));
        Assert.That(result.Data!.FirstName, Is.EqualTo("Test"));
        Assert.That(result.Data!.LastName, Is.EqualTo("User"));
        Assert.That(result.Data!.PhoneNumber, Is.EqualTo("123456789"));
        Assert.That(result.Data!.RoleId, Is.EqualTo(1));
    }

    [Test]
    public async Task GetUserAsync_WhenNoUsers_ReturnsEmptyList()
    {
        // Act
        var result = await _userService.GetUserAsync();
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetUserAsync_WhenUsersExist_ReturnsUserList()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.Users.AddRange(
            new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@user.com",
                PasswordHash = "hashedpassword",
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "123456789",
                RoleId = 1,
                Role = role,
                Password = "password",
                IsDeleted = false
            },
            new User
            {
                Id = 2,
                Username = "anotheruser",
                Email = "testanother@user.com",
                PasswordHash = "hashedpassword2",
                FirstName = "Test",
                LastName = "Another",
                PhoneNumber = "987654321",
                RoleId = 1,
                Role = role,
                Password = "password2",
                IsDeleted = false
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _userService.GetUserAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.EqualTo(2));
        Assert.That(result.Data!.Any(u => u.Username == "testuser"), Is.True);
        Assert.That(result.Data!.Any(u => u.Username == "anotheruser"), Is.True);
        Assert.That(result.Data!.Any(u => u.Email == "testanother@user.com"), Is.True);
        Assert.That(result.Data!.Any(u => u.Email == "test@user.com"), Is.True);
        Assert.That(result.Data!.Any(u => u.FirstName == "Test"), Is.True);
        Assert.That(result.Data!.Any(u => u.LastName == "User"), Is.True);
        Assert.That(result.Data!.Any(u => u.LastName == "Another"), Is.True);
        Assert.That(result.Data!.Any(u => u.PhoneNumber == "123456789"), Is.True);
        Assert.That(result.Data!.Any(u => u.PhoneNumber == "987654321"), Is.True);
        Assert.That(result.Data!.Any(u => u.RoleId == 1), Is.True);
        Assert.That(result.Data!.Any(u => u.RoleId == 1), Is.True);
        Assert.That(result.Data!.All(u => u.Id > 0), Is.True);
        Assert.That(result.Data!.All(u => !string.IsNullOrEmpty(u.Username)), Is.True);
        Assert.That(result.Data!.All(u => !string.IsNullOrEmpty(u.Email)), Is.True);
    }
    [Test]
    public async Task CreateUserAsync_NullDto_ReturnsFailResult()
    {
        // Act
        var result = await _userService.CreateUserAsync(null!);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task CreateUserAsync_MissingUsername_ReturnsFailResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var dto = new UserDto
        {
            Email = "test@gmail.com",
            Password = "password123",
            IsDeleted = false,
            PasswordHash = "hashedpassword",
            RoleId = 1,
        };
        // Act
        var result = await _userService.CreateUserAsync(dto);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Username is required"));
    }

    [Test]
    public async Task CreateUserAsync_MissingEmail_ReturnsFailResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        var dto = new UserDto
        {
            Username = "testuser",
            Password = "password123",
            IsDeleted = false,
            PasswordHash = "hashedpassword",
            RoleId = 1,
        };
        // Act
        var result = await _userService.CreateUserAsync(dto);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Email is required"));
    }

    [Test]
    public async Task CreateUserAsync_MissingPassword_ReturnsFailResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        var dto = new UserDto
        {
            Username = "testuser",
            Email = "test@user.com",
            IsDeleted = false,
            PasswordHash = "hashedpassword",
            RoleId = 1,
        };
        // Act
        var result = await _userService.CreateUserAsync(dto);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Password is required"));
    }

    [Test]
    public async Task CreateUserAsync_ShortPassword_ReturnsFailResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        var dto = new UserDto
        {
            Username = "testuser",
            Email = "testgmail.com",
            IsDeleted = false,
            PasswordHash = "hashedpassword",
            Password = "123",
            RoleId = 1,
            FirstName = "Test",
            LastName = "Test",
        };
        // Act
        var result = await _userService.CreateUserAsync(dto);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Password must be at least 6 characters"));
    }

    [Test]
    public async Task CreateUserAsync_DuplicateEmail_ReturnsFailResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "existinguser",
            Email = "testuser.com",
            PasswordHash = "hashedpassword",
            FirstName = "Existing",
            LastName = "User",
            RoleId = 1,
            Password = "password",
            PhoneNumber= "1234567890",
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();
        var dto = new Core.DTOs.UserDto
        {
            Username = "newuser",
            Email = "testuser.com",
            Password = "password123",
            IsDeleted = false,
            PasswordHash = "hashedpassword",
            PhoneNumber= "0987654321",
            RoleId = 1,
            FirstName = "New",
            LastName = "User",
        };
        // Act
        var result = await _userService.CreateUserAsync(dto);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Email already exists"));
    }

    [Test]
    public async Task CreateUserAsync_ValidDto_ReturnsSuccessResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        var dto = new UserDto
        {
            Username = "validuser",
            Email = "test@test1.com",
            Password = "validpassword",
            IsDeleted = false,
            PasswordHash = "hashedpassword",
            RoleId = 1,
            PhoneNumber= "1234567890",
            FirstName = "Valid",
            LastName = "User",
        };
        // Act
        var result = await _userService.CreateUserAsync(dto);
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.GreaterThan(0));
        Assert.That(result.Data!.Username, Is.EqualTo("validuser"));
        Assert.That(result.Data!.Email, Is.EqualTo("test@test1.com"));
        Assert.That(result.Data!.RoleId, Is.EqualTo(1));
        Assert.That(result.Data!.PhoneNumber, Is.EqualTo("1234567890"));
        Assert.That(result.Data!.FirstName, Is.EqualTo("Valid"));
        Assert.That(result.Data!.LastName, Is.EqualTo("User"));
        Assert.That(result.Data!.IsDeleted, Is.False);
    }

    [Test]
    public async Task UserExistsAsync_EmailExists_ReturnsTrue()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "existinguser",
            Email = "testuser.com",
            PasswordHash = "hashedpassword",
            FirstName = "Existing",
            LastName = "User",
            RoleId = 1,
            Password = "password",
            IsDeleted = false,
            PhoneNumber= "1234567890"
        });

        await _dbContext.SaveChangesAsync();
        // Act
        var result = await _userService.UserExistsAsync("testuser.com");
        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task UserExistsAsync_EmailDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _userService.UserExistsAsync("testuser.com");
        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UserExistsAsync_WhitespaceEmail_ReturnsFalse()
    {
        // Act
        var result = await _userService.UserExistsAsync("   ");
        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UserExistsAsync_ValidEmail_ReturnsTrue()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "existinguser",
            Email = "testuser.com",
            PasswordHash = "hashedpassword",
            FirstName = "Existing",
            LastName = "User",
            RoleId = 1,
            Password = "password",
            IsDeleted = false,
            PhoneNumber = "1234567890"
        });
        await _dbContext.SaveChangesAsync();
        // Act
        var result = await _userService.UserExistsAsync("testuser.com");
        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task UserExistsAsync_NullOrEmptyEmail_ReturnsFalse()
    {
        // Act
        var resultNull = await _userService.UserExistsAsync(null!);
        var resultEmpty = await _userService.UserExistsAsync(string.Empty);
        var resultWhitespace = await _userService.UserExistsAsync("   ");
        // Assert
        Assert.That(resultNull, Is.False);
        Assert.That(resultEmpty, Is.False);
        Assert.That(resultWhitespace, Is.False);
    }

    [Test]
    public async Task UpdateUserAsync_WhenEmailExists_ReturnsTrue()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "existinguser",
            Email = "testuser.com",
            PasswordHash = "hashedpassword",
            FirstName = "Existing",
            LastName = "User",
            RoleId = 1,
            Password = "password",
            IsDeleted = false,
            PhoneNumber= "1234567890"
        });
        await _dbContext.SaveChangesAsync();

        var dto = new UserDto
        {
            Id = 1,
            Username = "existinguser",
            Email = "testuser.com",
            Password = "password123",
            IsDeleted = false,
            PasswordHash = "hashedpassword",
            RoleId = 1,
            PhoneNumber= "0987654321",
            FirstName = "Existing",
            LastName = "User",
        };
        // Act
        var result = await _userService.UpdateUserAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Email already exists"));
    }

    [Test]
    public async Task UpdateUserAsync_ValidDto_ReturnsSuccessResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "existinguser",
            Email = "testuser.com",
            PasswordHash = "hashedpassword",
            FirstName = "Existing",
            LastName = "User",
            RoleId = 1,
            Password = "password",
            IsDeleted = false,
            PhoneNumber = "1234599990"
        });
        await _dbContext.SaveChangesAsync();
        
        Assert.That(_dbContext.Users.Count(), Is.EqualTo(1));
        Assert.That(_dbContext.Users.First().Email, Is.EqualTo("testuser.com"));
        Assert.That(_dbContext.Users.First().Username, Is.EqualTo("existinguser"));
        Assert.That(_dbContext.Users.First().PhoneNumber, Is.EqualTo("1234599990"));
        Assert.That(_dbContext.Users.First().FirstName, Is.EqualTo("Existing"));
        Assert.That(_dbContext.Users.First().LastName, Is.EqualTo("User"));
        Assert.That(_dbContext.Users.First().RoleId, Is.EqualTo(1));
        Assert.That(_dbContext.Users.First().IsDeleted, Is.False);

        _dbContext.ChangeTracker.Clear();

        var dto = new UserDto
        {
            Id = 1,
            Username = "updateduser",
            Email = "user@test.com",
            Password = "newpassword",
            IsDeleted = false,
            PhoneNumber = "1234567890",
            RoleId = 1,
            FirstName = "Updated",
            LastName = "User",
            PasswordHash = "newhashedpassword",
        };

        // Act
        var result = await _userService.UpdateUserAsync(dto);
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(1));
        Assert.That(result.Data!.Username, Is.EqualTo("updateduser"));
        Assert.That(result.Data!.Email, Is.EqualTo("user@test.com"));
        Assert.That(result.Data!.PhoneNumber, Is.EqualTo("1234567890"));
        Assert.That(result.Data!.RoleId, Is.EqualTo(1));
        Assert.That(result.Data!.FirstName, Is.EqualTo("Updated"));
        Assert.That(result.Data!.LastName, Is.EqualTo("User"));
    }

    [Test]
    public async Task UpdateUserAsync_WhenMessingUserName_ReturnsFailResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.SaveChanges();
        var dto = new UserDto
        {
            Id = 999,
            Email = "testuser.com", 
            Password = "password123",
            IsDeleted = false,
            PasswordHash = "hashedpassword",
            RoleId = 1,
            PhoneNumber= "0987654321",
            FirstName = "NonExistent",
            LastName = "User",
        };

        // Act
        var result = await _userService.UpdateUserAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Username is required"));
    }

    [Test]
    public async Task UpdateUserAsync_WhenNullDto_ReturnsFailResult()
    {
        // Act
        var result = await _userService.UpdateUserAsync(null!);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task UpdateUserAsync_WhenMessingEmail_ReturnsFailResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        var dto = new UserDto
        {
            Id = 1,
            Username = "testuser",
            Password = "password123",
            IsDeleted = false,
            PasswordHash = "hashedpassword",
            RoleId = 1,
        };
        // Act
        var result = await _userService.UpdateUserAsync(dto);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Email is required"));
    }

    [Test]
    public async Task DeleteUserAsync_NonExistentId_ReturnsFailResult()
    {
        // Act
        var result = await _userService.DeleteUserAsync(999);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid User ID"));
    }

    [Test]
    public async Task DeleteUserAsync_ValidId_ReturnsSuccessResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "deletetestuser",
            Email = "testUsetr.com",
            PasswordHash = "hashedpassword",
            RoleId = 1,
            FirstName = "Delete",
            LastName = "TestUser",
            Password = "password",
            IsDeleted = false,
            PhoneNumber = "1234567890"
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userService.DeleteUserAsync(1);
        
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That(result.Message, Is.EqualTo("User deleted successfully"));
    }

    [Test]
    public async Task DeleteUserAsync_AlreadyDeletedId_ReturnsFailResult()
    {
        await DeleteUserAsync_ValidId_ReturnsSuccessResult();
        // Act
        var result = await _userService.DeleteUserAsync(1);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid User ID"));
    }

    [Test]
    public async Task ChangeUserPasswordAsync_NonExistentId_ReturnsFailResult()
    {
        // Act
        var result = await _userService.ChangeUserPasswordAsync(999, "newpassword");
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid User ID"));
    }

    [Test]
    public async Task ChangeUserPasswordAsync_Valid_ReturnsSuccessResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "passwordtestuser",
            Email = "testUser.com",
            PasswordHash = "oldhashedpassword",
            RoleId = 1,
            FirstName = "Password",
            LastName = "TestUser",
            Password = "oldpassword",
            IsDeleted = false,
            PhoneNumber = "1234567890"
        });
        await _dbContext.SaveChangesAsync();
        // Act
        var result = await _userService.ChangeUserPasswordAsync(1, "newpassword");
        
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That(result.Message, Is.EqualTo("Password updated successfully"));
    }

    [Test]
    public async Task ChangeUserPasswordAsync_AlreadyDeletedId_ReturnsFailResult()
    {
        await DeleteUserAsync_ValidId_ReturnsSuccessResult();
        // Act
        var result = await _userService.ChangeUserPasswordAsync(1, "newpassword");
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid User ID"));
    }

    [Test]
    public async Task ChangeUserPasswordAsync_NullPassword_ReturnsFailResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "passwordtestuser",
            Email = "testUser.com",
            PasswordHash = "oldhashedpassword",
            RoleId = 1,
            FirstName = "Password",
            LastName = "TestUser",
            Password = "oldpassword",
            IsDeleted = false,
            PhoneNumber = "1234567890"
        });
        await _dbContext.SaveChangesAsync();
        // Act
        var result = await _userService.ChangeUserPasswordAsync(1, null!);
        
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("NewPassword is required"));
    }

    [Test]
    public async Task ChangeUserPasswordAsync_EmptyPassword_ReturnsFailResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "passwordtestuser",
            Email = "testUser.com",
            PasswordHash = "oldhashedpassword",
            RoleId = 1,
            FirstName = "Password",
            LastName = "TestUser",
            Password = "oldpassword",
            IsDeleted = false,
            PhoneNumber = "1234567890"
        });
        await _dbContext.SaveChangesAsync();
        // Act
        var result = await _userService.ChangeUserPasswordAsync(1, string.Empty);
        
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("NewPassword is required"));
    }

    [Test]
    public async Task ChangeUserPasswordAsync_WhenLessLengthPassword_ReturnsFailResult()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "passwordtestuser",
            Email = "testUser.com",
            PasswordHash = "oldhashedpassword",
            RoleId = 1,
            FirstName = "Password",
            LastName = "TestUser",
            Password = "oldpassword",
            IsDeleted = false,
            PhoneNumber = "1234567890"
        });
        await _dbContext.SaveChangesAsync();
        // Act
        var result = await _userService.ChangeUserPasswordAsync(1, "123");
        
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Password must be at least 6 characters"));
    }

    [Test]
    public async Task GetUserByUserNameAndEmail_InvalidUsernameAndEmail_ReturnsFailResult()
    {
        // Arrange
        string invalidUsername = "nonexistentuser";
        // Act
        var result = await _userService.GetUserByUserNameAndEmail(invalidUsername);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUserByUserNameAndEmail_ValidUsernameAndEmail_ReturnsbyName()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "testuser",
            Email = "testUser.com",
            PasswordHash = "oldhashedpassword",
            RoleId = 1,
            FirstName = "userTested",
            LastName = "TestUser",
            Password = "oldpassword",
            IsDeleted = false,
            PhoneNumber = "1234567890"
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userService.GetUserByUserNameAndEmail("testuser");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo("testuser"));
    }

    [Test]
    public async Task GetUserByUserNameAndEmail_ValidUsernameAndEmail_ReturnsByEmail()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };
        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "testuser",
            Email = "testUser.com",
            PasswordHash = "oldhashedpassword",
            RoleId = 1,
            FirstName = "userTested",
            LastName = "TestUser",
            Password = "oldpassword",
            IsDeleted = false,
            PhoneNumber = "1234567890"
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userService.GetUserByUserNameAndEmail("testUser.com");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo("testUser.com"));
    }

}

