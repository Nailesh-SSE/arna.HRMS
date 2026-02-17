using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Mapping;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Infrastructure.Validators;
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
        var validator = new UserValidator(UserRepository);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserProfile>();
        }); 
        _mapper = config.CreateMapper();
        _userService = new UserServices(
            UserRepository,
            _mapper,
            validator
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
        Assert.That(result.Message, Does.Contain("Username is required"));
    }

    [Test]
    public async Task CreateUserAsync_MissingFirstName_ReturnsFail()
    {
        var dto = new UserDto
        {
            Username = "testuser",
            Email = "test@test.com",
            Password = "password123",
            LastName = "User",
            RoleId = 1,
            PhoneNumber = "1234567890"
        };

        var result = await _userService.CreateUserAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("First name is required"));
    }

    [Test]
    public async Task CreateUserAsync_MissingLastName_ReturnsFail()
    {
        var dto = new UserDto
        {
            Username = "testuser",
            Email = "test@test.com",
            Password = "password123",
            FirstName = "Test",
            RoleId = 1,
            PhoneNumber = "1234567890"
        };

        var result = await _userService.CreateUserAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Last name is required"));
    }

    [Test]
    public async Task CreateUserAsync_InvalidRole_ReturnsFail()
    {
        var dto = new UserDto
        {
            Username = "testuser",
            Email = "test@test.com",
            Password = "password123",
            FirstName = "Test",
            LastName = "User",
            RoleId = 0,
            PhoneNumber = "1234567890"
        };

        var result = await _userService.CreateUserAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Role is required"));
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
        Assert.That(result.Message, Does.Contain("Email is required"));
    }

    [Test]
    public async Task CreateUserEntityAsync_ValidDto_ReturnsEntity()
    {
        var dto = new UserDto
        {
            Username = "entityuser",
            Email = "entity@test.com",
            Password = "password123",
            FirstName = "Entity",
            LastName = "User",
            RoleId = 1,
            PhoneNumber = "1234567890"
        };

        var result = await _userService.CreateUserEntityAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo("entityuser"));
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
        Assert.That(result.Message, Does.Contain("Password is required"));
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
        Assert.That(result.Message, Does.Contain("Password must be between 6 and 100 characters"));
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
            Email = "test@user.com",
            PasswordHash = "hashedpassword",
            FirstName = "Existing",
            LastName = "User",
            RoleId = 1,
            Password = "password",
            PhoneNumber= "1234567895",
            IsActive=true,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();
        var dto = new UserDto
        {
            Username = "newuser",
            Email = "test@user.com",
            Password = "password123",
            IsActive = true,
            IsDeleted = false,
            PasswordHash = "hashedpassword",
            PhoneNumber= "1987654321",
            RoleId = 1,
            FirstName = "New",
            LastName = "User",
        };
        // Act
        var result = await _userService.CreateUserAsync(dto);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Email or Phone Number already exists"));
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
        var result = await _userService.UserExistsAsync("testuser.com", "1234567890", null);
        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task UserExistsAsync_EmailDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _userService.UserExistsAsync("testuser.com", "1234567890", 1);
        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UserExistsAsync_WhitespaceEmail_ReturnsFalse()
    {
        // Act
        var result = await _userService.UserExistsAsync("", "",0);
        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UserExistsAsync_DuplicatePhone_ReturnsTrue()
    {
        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "user1",
            Email = "user1@test.com",
            Password = "password",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User",
            RoleId = 1,
            PhoneNumber = "1234567890"
        });

        await _dbContext.SaveChangesAsync();

        var result = await _userService.UserExistsAsync("new@test.com", "1234567890", null);

        Assert.That(result, Is.True);
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
        var result = await _userService.UserExistsAsync("testuser.com", "1234567890", null);
        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task UpdateUserAsync_InvalidId_ReturnsFailResult()
    {
        var dto = new UserDto
        {
            Id = 0,
            Username = "testuser",
            Email = "test@test.com",
            Password = "password123",
            FirstName = "Test",
            LastName = "User",
            RoleId = 1,
            PhoneNumber = "1234567890"
        };

        var result = await _userService.UpdateUserAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid User ID"));
    }

    [Test]
    public async Task UpdateUserAsync_NonExistingUser_ReturnsFailResult()
    {
        var dto = new UserDto
        {
            Id = 999,
            Username = "testuser",
            Email = "test@test.com",
            Password = "password123",
            FirstName = "Test",
            LastName = "User",
            RoleId = 1,
            PhoneNumber = "1234567890"
        };

        var result = await _userService.UpdateUserAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("No Data Found"));
    }


    [Test]
    public async Task UserExistsAsync_NullOrEmptyEmail_ReturnsFalse()
    {
        // Act
        var resultNull = await _userService.UserExistsAsync(null!, null!, 0);
        var resultEmpty = await _userService.UserExistsAsync(string.Empty, string.Empty, 0);
        var resultWhitespace = await _userService.UserExistsAsync("", "",0);
        // Assert
        Assert.That(resultNull, Is.False);
        Assert.That(resultEmpty, Is.False);
        Assert.That(resultWhitespace, Is.False);
    }

    [Test]
    public async Task UpdateUserAsync_WhenEmailExists_ReturnsFalse()
    {
        // Arrange

        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);

        // First user (the one we will update)
        var user1 = new User
        {
            Id = 1,
            Username = "user1",
            Email = "first@user.com",
            PasswordHash = "hash1",
            FirstName = "First",
            LastName = "User",
            RoleId = 1,
            Password = "password",
            IsDeleted = false,
            IsActive = true,
            PhoneNumber = "1111111111"
        };

        // Second user (already using target email)
        var user2 = new User
        {
            Id = 2,
            Username = "user2",
            Email = "test@user.com",   // 🔥 duplicate email
            PasswordHash = "hash2",
            FirstName = "Second",
            LastName = "User",
            RoleId = 1,
            Password = "password",
            IsDeleted = false,
            IsActive = true,
            PhoneNumber = "2222222222"
        };

        _dbContext.Users.AddRange(user1, user2);
        await _dbContext.SaveChangesAsync();

        // Act → Try to update user1 with email already used by user2
        var dto = new UserDto
        {
            Id = 1,
            Username = "user1",
            Email = "test@user.com", 
            Password = "newpassword",
            RoleId = 1,
            PhoneNumber = "1111111111",
            FirstName = "First",
            LastName = "User",
            IsDeleted = false
        };

        var result = await _userService.UpdateUserAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Email or Phone Number already exists"));
    }

    [Test]
    public async Task UpdateUserAsync_MissingFirstName_ReturnsFail()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "user",
            Email = "user@test.com",
            Password = "password",
            PasswordHash = "hash",
            FirstName = "Old",
            LastName = "User",
            RoleId = 1,
            PhoneNumber = "1234567890"
        });

        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var dto = new UserDto
        {
            Id = 1,
            Username = "user",
            Email = "user@test.com",
            Password = "password123",
            FirstName = "",
            LastName = "User",
            RoleId = 1,
            PhoneNumber = "1234567890"
        };

        var result = await _userService.UpdateUserAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("First name is required"));
    }

    [Test]
    public async Task UpdateUserAsync_InvalidRole_ReturnsFail()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "user",
            Email = "user@test.com",
            Password = "password",
            PasswordHash = "hash",
            FirstName = "Old",
            LastName = "User",
            RoleId = 1,
            PhoneNumber = "1234567890"
        });

        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var dto = new UserDto
        {
            Id = 1,
            Username = "user",
            Email = "user@test.com",
            Password = "password123",
            FirstName = "Updated",
            LastName = "User",
            RoleId = 0,
            PhoneNumber = "1234567890"
        };

        var result = await _userService.UpdateUserAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Role is required"));
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

        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "existinguser",
            Email = "test@user.com",
            PasswordHash = "hashedpassword",
            FirstName = "Existing",
            LastName = "User",
            RoleId = 1,
            Password = "password",
            IsDeleted = false,
            PhoneNumber = "1234599990"
        });
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();
        var dto = new UserDto
        {
            Id = 1,
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
        Assert.That(result.Message, Does.Contain("Username is required"));
    }

    [Test]
    public async Task UpdateUserAsync_InvalidEmailFormat_ReturnsFail()
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
            Email = "test@user.com",
            PasswordHash = "hashedpassword",
            FirstName = "Existing",
            LastName = "User",
            RoleId = 1,
            Password = "password",
            IsDeleted = false,
            PhoneNumber = "1234599990"
        });
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var dto = new UserDto
        {
            Id = 1,
            Username = "testuser",
            Email = "invalid-email",
            Password = "password123",
            FirstName = "Test",
            LastName = "User",
            RoleId = 1,
            PhoneNumber = "1234567890"
        };

        var result = await _userService.UpdateUserAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid email format"));
    }

    [Test]
    public async Task UpdateUserAsync_InvalidPhoneFormat_ReturnsFail()
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
            Email = "test@user.com",
            PasswordHash = "hashedpassword",
            FirstName = "Existing",
            LastName = "User",
            RoleId = 1,
            Password = "password",
            IsDeleted = false,
            PhoneNumber = "1234599990"
        });
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();
        var dto = new UserDto
        {
            Id = 1,
            Username = "testuser",
            Email = "test@test.com",
            Password = "password123",
            FirstName = "Test",
            LastName = "User",
            RoleId = 1,
            PhoneNumber = "abc"
        };

        var result = await _userService.UpdateUserAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid phone number format"));
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

        _dbContext.Users.Add(new User
        {
            Id = 1,
            Username = "existinguser",
            Email = "test@user.com",
            PasswordHash = "hashedpassword",
            FirstName = "Existing",
            LastName = "User",
            RoleId = 1,
            Password = "password",
            IsDeleted = false,
            PhoneNumber = "1234599990"
        });
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

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
        Assert.That(result.Message, Does.Contain("Email is required"));
    }

    [Test]
    public async Task DeleteUserAsync_NonExistentId_ReturnsFailResult()
    {
        // Act
        var result = await _userService.DeleteUserAsync(999);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Fail to Delete User"));
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
        Assert.That(result.Message, Is.EqualTo("Fail to Delete User"));
    }

    [Test]
    public async Task ChangeUserPasswordAsync_NonExistentId_ReturnsFailResult()
    {
        // Act
        var result = await _userService.ChangeUserPasswordAsync(999, "newpassword");
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("No Data Found"));
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
        Assert.That(result.Message, Is.EqualTo("No Data Found"));
    }

    [Test]
    public async Task CreateUserAsync_PhoneNot10Digits_ReturnsFail()
    {
        var dto = new UserDto
        {
            Username = "testuser",
            Email = "test@test.com",
            Password = "password123",
            FirstName = "Test",
            LastName = "User",
            RoleId = 1,
            PhoneNumber = "12345"
        };

        var result = await _userService.CreateUserAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Phone number must be exactly 10 digits"));
    }


    [Test]
    public async Task CreateUserAsync_InvalidPhoneFormat_ReturnsFail()
    {
        var dto = new UserDto
        {
            Username = "testuser",
            Email = "test@test.com",
            Password = "password123",
            FirstName = "Test",
            LastName = "User",
            RoleId = 1,
            PhoneNumber = "abc123"
        };

        var result = await _userService.CreateUserAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid phone number format"));
    }


    [Test]
    public async Task CreateUserAsync_InvalidEmailFormat_ReturnsFail()
    {
        var dto = new UserDto
        {
            Username = "testuser",
            Email = "invalid-email",
            Password = "password123",
            FirstName = "Test",
            LastName = "User",
            RoleId = 1,
            PhoneNumber = "1234567890"
        };

        var result = await _userService.CreateUserAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid email format"));
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
        Assert.That(result.Message, Is.EqualTo("Password is required"));
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
        Assert.That(result.Message, Is.EqualTo("Password is required"));
    }

    [Test]
    public async Task ChangeUserPasswordAsync_InvalidId_ReturnsFailResult()
    {
        var result = await _userService.ChangeUserPasswordAsync(0, "password123");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid User ID"));
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
        Assert.That(result.Message, Is.EqualTo("Password must be between 6 and 100 characters"));
    }

    [Test]
    public async Task GetUserByUserNameAndEmail_Whitespace_ReturnsNull()
    {
        var result = await _userService.GetUserByUserNameAndEmail("   ");

        Assert.That(result, Is.Null);
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

