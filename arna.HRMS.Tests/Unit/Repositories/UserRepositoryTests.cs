using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Repositories;

[TestFixture]
public class UserRepositoryTests
{
    private ApplicationDbContext _dbContext = null!;
    private UserRepository _userRepository = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var userBaseRepo = new BaseRepository<User>(_dbContext);

        _userRepository = new UserRepository(userBaseRepo);
    }

    [Test]
    public async Task GetUserAsync_ShouldReturn_OnlyActiveAndNotDeletedUsers()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };
        var employee = new Employee
        {
            Id = 1,
            FirstName = "Emp",
            LastName = "Test",
            Email = "emp@test.com",
            EmployeeNumber = "EMP001",
            PhoneNumber = "9876543210",
            Position = "Developer",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        _dbContext.Employees.Add(employee);

        _dbContext.Users.AddRange(
            new User
            {
                Id = 1,
                Username = "user1",
                Email = "user1@test.com",
                PasswordHash = "hash",
                Password = "123",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "1234567890",
                RoleId = 1,
                EmployeeId = 1,
                IsActive = true,
                IsDeleted = false
            },
            new User
            {
                Id = 2,
                Username = "user2",
                Email = "user2@test.com",
                PasswordHash = "hash",
                Password = "123",
                FirstName = "Jane",
                LastName = "Smith",
                PhoneNumber = "1234567890",
                RoleId = 1,
                IsActive = false,  
                IsDeleted = false
            },
            new User
            {
                Id = 3,
                Username = "user3",
                Email = "user3@test.com",
                PasswordHash = "hash",
                Password = "123",
                FirstName = "Mike",
                LastName = "Tyson",
                PhoneNumber = "1234567890",
                RoleId = 1,
                IsActive = true,
                IsDeleted = true   
            }
        );

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetUserAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<List<User>>());
        Assert.That(result.Count, Is.EqualTo(1));

        var user = result.First();

        Assert.That(user.IsActive, Is.True);
        Assert.That(user.IsDeleted, Is.False);
        Assert.That(user.Username, Is.EqualTo("user1"));
        Assert.That(user.Email, Is.EqualTo("user1@test.com"));
    }

    [Test]
    public async Task GetUserAsync_ShouldReturn_UsersOrderedByIdDescending()
    {
        // Arrange

        var role = new Role
        {
            Id = 1,
            Name = "Admin"
        };

        _dbContext.Roles.Add(role);

        var employee1 = CreateEmployee(1);
        var employee2 = CreateEmployee(2);
        var employee3 = CreateEmployee(3);

        _dbContext.Employees.AddRange(employee1, employee2, employee3);

        await _dbContext.SaveChangesAsync();

        _dbContext.Users.AddRange(
            CreateUser(1, true, false, 1),
            CreateUser(2, true, false, 2),
            CreateUser(3, true, false, 3)
        );

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetUserAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Count, Is.EqualTo(3));

        Assert.That(result[0].Id, Is.EqualTo(3));
        Assert.That(result[1].Id, Is.EqualTo(2));
        Assert.That(result[2].Id, Is.EqualTo(1));
    }


    [Test]
    public async Task GetUserAsync_ShouldInclude_Employee_And_Role()
    {
        // Arrange
        var role = new Role
        {
            Id = 10,
            Name = "Manager"
        };

        var employee = new Employee
        {
            Id = 20,
            FirstName = "Emp",
            LastName = "Test",
            Email = "emp@test.com",
            EmployeeNumber = "EMP001",
            PhoneNumber = "9876543210",
            Position = "Developer",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        _dbContext.Employees.Add(employee);

        _dbContext.Users.Add(new User
        {
            Id = 100,
            Username = "includeTest",
            Email = "include@test.com",
            PasswordHash = "hash",
            Password = "123",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890",
            RoleId = 10,
            EmployeeId = 20,
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetUserAsync();

        var user = result.First();

        // Assert
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Role, Is.Not.Null);
        Assert.That(user.Employee, Is.Not.Null);

        Assert.That(user.Role!.Id, Is.EqualTo(10));
        Assert.That(user.Role!.Name, Is.EqualTo("Manager"));

        Assert.That(user.Employee!.Id, Is.EqualTo(20));
        Assert.That(user.Employee!.Email, Is.EqualTo("emp@test.com"));
        Assert.That(user.Employee!.EmployeeNumber, Is.EqualTo("EMP001"));
    }


    [Test]
    public async Task GetUserAsync_ShouldReturn_EmptyList_WhenNoActiveUsers()
    {
        // Arrange
        _dbContext.Users.Add(CreateUser(1, false, false, null));
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetUserAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
        Assert.That(result.Count, Is.EqualTo(0));
    }

    private User CreateUser(int id, bool isActive, bool isDeleted, int? empId)
    {
        return new User
        {
            Id = id,
            Username = $"user{id}",
            Email = $"user{id}@test.com",
            PasswordHash = "hash",
            Password = "123",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890",
            RoleId = 1,
            EmployeeId = empId,
            IsActive = isActive,
            IsDeleted = isDeleted
        };
    }
    private Employee CreateEmployee(int id)
    {
        return new Employee
        {
            Id = id,
            FirstName = $"Emp{id}",
            LastName = "Test",
            Email = $"emp{id}@test.com",
            EmployeeNumber = $"EMP{id}",
            PhoneNumber = "9999999999",
            Position = "Developer",
            IsActive = true,
            IsDeleted = false
        };
    }

    [Test]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExistsAndIsActive()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };
        var employee = CreateEmployee(1);

        _dbContext.Roles.Add(role);
        _dbContext.Employees.Add(employee);

        var user = CreateUser(1, true, false, 1);
        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetUserByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
        Assert.That(result.IsActive, Is.True);
        Assert.That(result.IsDeleted, Is.False);
        Assert.That(result.Username, Is.EqualTo("user1"));
    }

    [Test]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var result = await _userRepository.GetUserByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserIsInactive()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        var user = CreateUser(1, false, false, null);
        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetUserByIdAsync(1);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserIsDeleted()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        var user = CreateUser(1, true, true, null);
        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetUserByIdAsync(1);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUserByIdAsync_ShouldInclude_Employee_And_Role()
    {
        // Arrange
        var role = new Role { Id = 5, Name = "Manager" };
        var employee = CreateEmployee(10);

        _dbContext.Roles.Add(role);
        _dbContext.Employees.Add(employee);

        var user = new User
        {
            Id = 50,
            Username = "includeUser",
            Email = "include@test.com",
            PasswordHash = "hash",
            Password = "123",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890",
            RoleId = 5,
            EmployeeId = 10,
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetUserByIdAsync(50);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Role, Is.Not.Null);
        Assert.That(result.Employee, Is.Not.Null);

        Assert.That(result.Role!.Id, Is.EqualTo(5));
        Assert.That(result.Role!.Name, Is.EqualTo("Manager"));

        Assert.That(result.Employee!.Id, Is.EqualTo(10));
        Assert.That(result.Employee!.Email, Is.EqualTo("emp10@test.com"));
    }

    [Test]
    public async Task CreateUserAsync_ShouldAddUserSuccessfully()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };
        var employee = CreateEmployee(1);

        _dbContext.Roles.Add(role);
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var user = CreateUser(1, true, false, 1);

        // Act
        var result = await _userRepository.CreateUserAsync(user);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<User>());
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Username, Is.EqualTo("user1"));
        Assert.That(result.Email, Is.EqualTo("user1@test.com"));
        Assert.That(result.RoleId, Is.EqualTo(1));
        Assert.That(result.EmployeeId, Is.EqualTo(1));

        var dbUser = await _dbContext.Users.FindAsync(1);

        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser!.Username, Is.EqualTo("user1"));
    }

    [Test]
    public async Task CreateUserAsync_ShouldPersistUserInDatabase()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var user = CreateUser(2, true, false, null);

        // Act
        await _userRepository.CreateUserAsync(user);

        // Assert
        var allUsers = await _dbContext.Users.ToListAsync();

        Assert.That(allUsers, Is.Not.Null);
        Assert.That(allUsers.Count, Is.EqualTo(1));
        Assert.That(allUsers.First().Id, Is.EqualTo(2));
    }

    [Test]
    public async Task CreateUserAsync_ShouldCreateUser_WithRoleAndEmployee()
    {
        // Arrange
        var role = new Role { Id = 5, Name = "Manager" };
        var employee = CreateEmployee(10);

        _dbContext.Roles.Add(role);
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var user = new User
        {
            Id = 50,
            Username = "navUser",
            Email = "nav@test.com",
            PasswordHash = "hash",
            Password = "123",
            FirstName = "Nav",
            LastName = "User",
            PhoneNumber = "1234567890",
            RoleId = 5,
            EmployeeId = 10,
            IsActive = true,
            IsDeleted = false
        };

        // Act
        var result = await _userRepository.CreateUserAsync(user);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.RoleId, Is.EqualTo(5));
        Assert.That(result.EmployeeId, Is.EqualTo(10));

        var savedUser = await _dbContext.Users
            .Include(x => x.Role)
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == 50);

        Assert.That(savedUser, Is.Not.Null);
        Assert.That(savedUser!.Role, Is.Not.Null);
        Assert.That(savedUser!.Employee, Is.Not.Null);
        Assert.That(savedUser.Role!.Name, Is.EqualTo("Manager"));
    }

    [Test]
    public void CreateUserAsync_ShouldThrowException_WhenUserIsNull()
    {
        // Arrange
        User? user = null;

        // Act & Assert
        Assert.That(async () => await _userRepository.CreateUserAsync(user!),
            Throws.TypeOf<ArgumentNullException>().Or.TypeOf<Exception>());
    }

    [Test]
    public async Task UpdateUserAsync_ShouldUpdateUserSuccessfully()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var user = CreateUser(1, true, false, null);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Modify values
        user.Username = "updatedUser";
        user.Email = "updated@test.com";
        user.FirstName = "Updated";
        user.LastName = "Name";

        // Act
        var result = await _userRepository.UpdateUserAsync(user);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo("updatedUser"));
        Assert.That(result.Email, Is.EqualTo("updated@test.com"));

        var dbUser = await _dbContext.Users.FindAsync(1);

        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser!.Username, Is.EqualTo("updatedUser"));
        Assert.That(dbUser.Email, Is.EqualTo("updated@test.com"));
        Assert.That(dbUser.FirstName, Is.EqualTo("Updated"));
        Assert.That(dbUser.LastName, Is.EqualTo("Name"));
    }

    [Test]
    public async Task UpdateUserAsync_ShouldUpdate_Role_And_Employee()
    {
        // Arrange
        var role1 = new Role { Id = 1, Name = "Admin" };
        var role2 = new Role { Id = 2, Name = "Manager" };

        var employee1 = CreateEmployee(1);
        var employee2 = CreateEmployee(2);

        _dbContext.Roles.AddRange(role1, role2);
        _dbContext.Employees.AddRange(employee1, employee2);

        await _dbContext.SaveChangesAsync();

        var user = CreateUser(10, true, false, 1);
        user.RoleId = 1;

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Change Role & Employee
        user.RoleId = 2;
        user.EmployeeId = 2;

        // Act
        var result = await _userRepository.UpdateUserAsync(user);

        // Assert
        Assert.That(result.RoleId, Is.EqualTo(2));
        Assert.That(result.EmployeeId, Is.EqualTo(2));

        var updatedUser = await _dbContext.Users
            .Include(x => x.Role)
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == 10);

        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser!.Role, Is.Not.Null);
        Assert.That(updatedUser.Role!.Name, Is.EqualTo("Manager"));
        Assert.That(updatedUser.Employee!.Id, Is.EqualTo(2));
    }

    [Test]
    public async Task UpdateUserAsync_ShouldPersistChanges()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var user = CreateUser(5, true, false, null);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        user.PhoneNumber = "9999999999";

        // Act
        await _userRepository.UpdateUserAsync(user);

        // Assert
        var dbUser = await _dbContext.Users.FindAsync(5);

        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser!.PhoneNumber, Is.EqualTo("9999999999"));
    }

    [Test]
    public void UpdateUserAsync_ShouldThrowNullReferenceException_WhenUserIsNull()
    {
        User? user = null;

        Assert.That(async () => await _userRepository.UpdateUserAsync(user!),
            Throws.TypeOf<NullReferenceException>());
    }

    [Test]
    public void UpdateUserAsync_ShouldThrowConcurrencyException_WhenUserDoesNotExist()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);
        _dbContext.SaveChanges();

        var user = CreateUser(999, true, false, null);

        // Act & Assert
        Assert.That(async () => await _userRepository.UpdateUserAsync(user),
            Throws.TypeOf<DbUpdateConcurrencyException>());
    }

    [Test]
    public async Task GetByUsernameOrEmailAsync_ShouldReturnUser_WhenUsernameMatches()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };
        var employee = CreateEmployee(1);

        _dbContext.Roles.Add(role);
        _dbContext.Employees.Add(employee);

        var user = CreateUser(1, true, false, 1);
        user.Username = "testuser";

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetByUsernameOrEmailAsync("testuser");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo("testuser"));
    }

    [Test]
    public async Task GetByUsernameOrEmailAsync_ShouldReturnUser_WhenEmailMatches()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        var user = CreateUser(2, true, false, null);
        user.Email = "user@test.com";

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var result = await _userRepository.GetByUsernameOrEmailAsync("user@test.com");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo("user@test.com"));
    }

    [Test]
    public async Task GetByUsernameOrEmailAsync_ShouldBeCaseInsensitive()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        var user = CreateUser(3, true, false, null);
        user.Username = "CaseUser";

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var result = await _userRepository.GetByUsernameOrEmailAsync("caseuser");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo("CaseUser"));
    }

    [Test]
    public async Task GetByUsernameOrEmailAsync_ShouldTrimInput()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        var user = CreateUser(4, true, false, null);
        user.Username = "trimUser";

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var result = await _userRepository.GetByUsernameOrEmailAsync("  trimUser  ");

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetByUsernameOrEmailAsync_ShouldReturnNull_WhenInputIsNull()
    {
        var result = await _userRepository.GetByUsernameOrEmailAsync(null!);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByUsernameOrEmailAsync_ShouldReturnNull_WhenInputIsEmpty()
    {
        var result = await _userRepository.GetByUsernameOrEmailAsync("");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByUsernameOrEmailAsync_ShouldReturnNull_WhenUserIsInactive()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        var user = CreateUser(5, false, false, null);
        user.Username = "inactiveUser";

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var result = await _userRepository.GetByUsernameOrEmailAsync("inactiveUser");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByUsernameOrEmailAsync_ShouldReturnNull_WhenUserIsDeleted()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        var user = CreateUser(6, true, true, null);
        user.Username = "deletedUser";

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var result = await _userRepository.GetByUsernameOrEmailAsync("deletedUser");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByUsernameOrEmailAsync_ShouldReturnNull_WhenUserNotFound()
    {
        var result = await _userRepository.GetByUsernameOrEmailAsync("unknownUser");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByUsernameOrEmailAsync_ShouldInclude_Employee_And_Role()
    {
        var role = new Role { Id = 10, Name = "Manager" };
        var employee = CreateEmployee(20);

        _dbContext.Roles.Add(role);
        _dbContext.Employees.Add(employee);

        var user = new User
        {
            Id = 50,
            Username = "includeUser",
            Email = "include@test.com",
            PasswordHash = "hash",
            Password = "123",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890",
            RoleId = 10,
            EmployeeId = 20,
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var result = await _userRepository.GetByUsernameOrEmailAsync("includeUser");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Role, Is.Not.Null);
        Assert.That(result.Employee, Is.Not.Null);
        Assert.That(result.Role!.Name, Is.EqualTo("Manager"));
    }

    [Test]
    public async Task ChangeUserPasswordAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        var user = CreateUser(1, true, false, null);
        user.Password = "oldPassword";
        user.PasswordHash = "oldHash";

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userRepository.ChangeUserPasswordAsync(1, "newPassword");

        // Assert
        Assert.That(result, Is.True);

        var dbUser = await _dbContext.Users.FindAsync(1);

        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser!.Password, Is.EqualTo("newPassword"));
        Assert.That(dbUser.PasswordHash, Is.Not.EqualTo("oldHash"));
        Assert.That(dbUser.PasswordHash, Is.Not.Null);
    }

    [Test]
    public async Task ChangeUserPasswordAsync_ShouldReturnFalse_WhenUserNotFound()
    {
        var result = await _userRepository.ChangeUserPasswordAsync(999, "newPassword");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ChangeUserPasswordAsync_ShouldReturnFalse_WhenUserIsInactive()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        var user = CreateUser(2, false, false, null);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var result = await _userRepository.ChangeUserPasswordAsync(2, "newPassword");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ChangeUserPasswordAsync_ShouldUpdate_UpdatedOn()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        var user = CreateUser(4, true, false, null);
        user.UpdatedOn = DateTime.Now.AddDays(-5);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var oldUpdatedOn = user.UpdatedOn;

        await _userRepository.ChangeUserPasswordAsync(4, "newPassword");

        var dbUser = await _dbContext.Users.FindAsync(4);

        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser!.UpdatedOn, Is.GreaterThan(oldUpdatedOn));
    }

    [Test]
    public async Task ChangeUserPasswordAsync_ShouldPersistChanges()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        var user = CreateUser(5, true, false, null);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        await _userRepository.ChangeUserPasswordAsync(5, "secure123");

        var dbUser = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == 5);

        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser!.Password, Is.EqualTo("secure123"));
    }

    [Test]
    public async Task ChangeUserPasswordAsync_ShouldReturnFalse_WhenPasswordIsEmpty()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _dbContext.Roles.Add(role);

        var user = CreateUser(6, true, false, null);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var result = await _userRepository.ChangeUserPasswordAsync(6, "");

        Assert.That(result, Is.False);
    }


}
