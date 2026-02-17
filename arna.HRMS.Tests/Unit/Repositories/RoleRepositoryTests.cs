using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Repositories;

[TestFixture]
public class RoleRepositoryTests
{
    private ApplicationDbContext _dbContext = null!;
    private RoleRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                   .UseInMemoryDatabase(Guid.NewGuid().ToString())
                   .Options;

        _dbContext = new ApplicationDbContext(options);

        var roleBaseRepo = new BaseRepository<Role>(_dbContext);
        _repository = new RoleRepository(roleBaseRepo);
    }

    [Test]
    public async Task GetRolesAsync_ShouldReturnOnlyActiveAndNotDeletedRoles()
    {
        // Arrange
        _dbContext.Roles.AddRange(
            new Role { Id = 1, Name = "Admin", IsActive = true, IsDeleted = false },
            new Role { Id = 2, Name = "Manager", IsActive = false, IsDeleted = false },
            new Role { Id = 3, Name = "HR", IsActive = true, IsDeleted = true }
        );

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRolesAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<List<Role>>());
        Assert.That(result.Count, Is.EqualTo(1));

        var role = result.First();

        Assert.That(role.Id, Is.EqualTo(1));
        Assert.That(role.IsActive, Is.True);
        Assert.That(role.IsDeleted, Is.False);
    }

    [Test]
    public async Task GetRolesAsync_ShouldReturnRolesOrderedByIdDescending()
    {
        // Arrange
        _dbContext.Roles.AddRange(
            new Role { Id = 1, Name = "Role1", IsActive = true, IsDeleted = false },
            new Role { Id = 2, Name = "Role2", IsActive = true, IsDeleted = false },
            new Role { Id = 3, Name = "Role3", IsActive = true, IsDeleted = false }
        );

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRolesAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result[0].Id, Is.EqualTo(3));
        Assert.That(result[1].Id, Is.EqualTo(2));
        Assert.That(result[2].Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetRolesAsync_ShouldReturnEmptyList_WhenNoActiveRoles()
    {
        // Arrange
        _dbContext.Roles.Add(
            new Role { Id = 1, Name = "Admin", IsActive = false, IsDeleted = false }
        );

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRolesAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetRolesAsync_ShouldReturnEmptyList_WhenAllRolesDeleted()
    {
        // Arrange
        _dbContext.Roles.AddRange(
            new Role { Id = 1, Name = "Admin", IsActive = true, IsDeleted = true },
            new Role { Id = 2, Name = "Manager", IsActive = true, IsDeleted = true }
        );

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRolesAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetRoleByIdAsync_ShouldReturnRole_WhenRoleExistsAndIsActive()
    {
        // Arrange
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRoleByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Admin"));
        Assert.That(result.IsActive, Is.True);
        Assert.That(result.IsDeleted, Is.False);
    }

    [Test]
    public async Task GetRoleByIdAsync_ShouldReturnNull_WhenRoleDoesNotExist()
    {
        // Act
        var result = await _repository.GetRoleByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetRoleByIdAsync_ShouldReturnNull_WhenRoleIsInactive()
    {
        // Arrange
        var role = new Role
        {
            Id = 2,
            Name = "Manager",
            IsActive = false,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRoleByIdAsync(2);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetRoleByIdAsync_ShouldReturnNull_WhenRoleIsDeleted()
    {
        // Arrange
        var role = new Role
        {
            Id = 3,
            Name = "HR",
            IsActive = true,
            IsDeleted = true
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRoleByIdAsync(3);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetRoleByIdAsync_ShouldReturnNull_WhenIdIsInvalid()
    {
        var resultZero = await _repository.GetRoleByIdAsync(0);
        var resultNegative = await _repository.GetRoleByIdAsync(-1);

        Assert.That(resultZero, Is.Null);
        Assert.That(resultNegative, Is.Null);
    }

    [Test]
    public async Task GetRoleByIdAsync_ShouldReturnCorrectRole_WhenMultipleExist()
    {
        _dbContext.Roles.AddRange(
            new Role { Id = 1, Name = "Admin", IsActive = true, IsDeleted = false },
            new Role { Id = 2, Name = "Manager", IsActive = true, IsDeleted = false },
            new Role { Id = 3, Name = "HR", IsActive = true, IsDeleted = false }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetRoleByIdAsync(2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(2));
        Assert.That(result.Name, Is.EqualTo("Manager"));
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldReturnRole_WhenNameMatches()
    {
        // Arrange
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRoleByNameAsync("Admin");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Admin"));
        Assert.That(result.IsActive, Is.True);
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldReturnNull_WhenRoleDoesNotExist()
    {
        var result = await _repository.GetRoleByNameAsync("Unknown");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldReturnNull_WhenRoleIsInactive()
    {
        var role = new Role
        {
            Id = 2,
            Name = "Manager",
            IsActive = false,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetRoleByNameAsync("Manager");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldReturnNull_WhenRoleIsDeleted()
    {
        var role = new Role
        {
            Id = 3,
            Name = "HR",
            IsActive = true,
            IsDeleted = true
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetRoleByNameAsync("HR");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldBeCaseSensitive()
    {
        var role = new Role
        {
            Id = 4,
            Name = "Admin",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetRoleByNameAsync("admin");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldReturnNull_WhenNameIsNull()
    {
        var result = await _repository.GetRoleByNameAsync(null!);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldReturnNull_WhenNameIsEmpty()
    {
        var result = await _repository.GetRoleByNameAsync("");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldReturnCorrectRole_WhenMultipleExist()
    {
        _dbContext.Roles.AddRange(
            new Role { Id = 1, Name = "Admin", IsActive = true, IsDeleted = false },
            new Role { Id = 2, Name = "Manager", IsActive = true, IsDeleted = false },
            new Role { Id = 3, Name = "HR", IsActive = true, IsDeleted = false }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetRoleByNameAsync("Manager");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(2));
    }

    [Test]
    public async Task CreateRoleAsync_ShouldCreateRoleSuccessfully()
    {
        // Arrange
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsActive = true,
            IsDeleted = false
        };

        // Act
        var result = await _repository.CreateRoleAsync(role);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<Role>());
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Admin"));

        var dbRole = await _dbContext.Roles.FindAsync(1);

        Assert.That(dbRole, Is.Not.Null);
        Assert.That(dbRole!.Name, Is.EqualTo("Admin"));
    }

    [Test]
    public async Task CreateRoleAsync_ShouldPersistRoleInDatabase()
    {
        var role = new Role
        {
            Id = 2,
            Name = "Manager",
            IsActive = true,
            IsDeleted = false
        };

        await _repository.CreateRoleAsync(role);

        var roles = await _dbContext.Roles.ToListAsync();

        Assert.That(roles, Is.Not.Null);
        Assert.That(roles.Count, Is.EqualTo(1));
        Assert.That(roles.First().Name, Is.EqualTo("Manager"));
    }

    [Test]
    public void CreateRoleAsync_ShouldThrowArgumentNullException_WhenRoleIsNull()
    {
        Role? role = null;

        Assert.That(async () => await _repository.CreateRoleAsync(role!),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public async Task CreateRoleAsync_ShouldAllowMultipleRoles()
    {
        var role1 = new Role
        {
            Id = 10,
            Name = "HR",
            IsActive = true,
            IsDeleted = false
        };

        var role2 = new Role
        {
            Id = 11,
            Name = "Finance",
            IsActive = true,
            IsDeleted = false
        };

        await _repository.CreateRoleAsync(role1);
        await _repository.CreateRoleAsync(role2);

        var roles = await _dbContext.Roles.ToListAsync();

        Assert.That(roles.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task CreateRoleAsync_ShouldAllowDuplicateNames_IfNoValidationExists()
    {
        var role1 = new Role { Id = 20, Name = "Admin", IsActive = true, IsDeleted = false };
        var role2 = new Role { Id = 21, Name = "Admin", IsActive = true, IsDeleted = false };

        await _repository.CreateRoleAsync(role1);
        await _repository.CreateRoleAsync(role2);

        var roles = await _dbContext.Roles.ToListAsync();

        Assert.That(roles.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task UpdateRoleAsync_ShouldUpdateRoleSuccessfully()
    {
        // Arrange
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        // Modify
        role.Name = "SuperAdmin";

        // Act
        var result = await _repository.UpdateRoleAsync(role);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("SuperAdmin"));

        var dbRole = await _dbContext.Roles.FindAsync(1);

        Assert.That(dbRole, Is.Not.Null);
        Assert.That(dbRole!.Name, Is.EqualTo("SuperAdmin"));
    }

    [Test]
    public async Task UpdateRoleAsync_ShouldPersistMultipleChanges()
    {
        var role = new Role
        {
            Id = 2,
            Name = "Manager",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        role.Name = "UpdatedManager";
        role.IsActive = false;

        await _repository.UpdateRoleAsync(role);

        var dbRole = await _dbContext.Roles.FindAsync(2);

        Assert.That(dbRole, Is.Not.Null);
        Assert.That(dbRole!.Name, Is.EqualTo("UpdatedManager"));
        Assert.That(dbRole.IsActive, Is.False);
    }

    [Test]
    public void UpdateRoleAsync_ShouldThrowArgumentNullException_WhenRoleIsNull()
    {
        Role? role = null;

        Assert.That(async () => await _repository.UpdateRoleAsync(role!),
            Throws.TypeOf<NullReferenceException>());
    }

    [Test]
    public void UpdateRoleAsync_ShouldThrowConcurrencyException_WhenRoleDoesNotExist()
    {
        var role = new Role
        {
            Id = 999,
            Name = "GhostRole",
            IsActive = true,
            IsDeleted = false
        };

        Assert.That(async () => await _repository.UpdateRoleAsync(role),
            Throws.TypeOf<DbUpdateConcurrencyException>());
    }

    [Test]
    public async Task UpdateRoleAsync_ShouldUpdateSoftDeleteFlags()
    {
        var role = new Role
        {
            Id = 5,
            Name = "TempRole",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        role.IsDeleted = true;
        role.IsActive = false;

        await _repository.UpdateRoleAsync(role);

        var dbRole = await _dbContext.Roles.FindAsync(5);

        Assert.That(dbRole, Is.Not.Null);
        Assert.That(dbRole!.IsDeleted, Is.True);
        Assert.That(dbRole.IsActive, Is.False);
    }

    [Test]
    public async Task DeleteRoleAsync_ShouldReturnTrue_WhenRoleExists()
    {
        // Arrange
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsActive = true,
            IsDeleted = false,
            UpdatedOn = DateTime.Now.AddDays(-2)
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteRoleAsync(1);

        // Assert
        Assert.That(result, Is.True);

        var dbRole = await _dbContext.Roles.FindAsync(1);

        Assert.That(dbRole, Is.Not.Null);
        Assert.That(dbRole!.IsActive, Is.False);
        Assert.That(dbRole.IsDeleted, Is.True);
    }

    [Test]
    public async Task DeleteRoleAsync_ShouldReturnFalse_WhenRoleDoesNotExist()
    {
        var result = await _repository.DeleteRoleAsync(999);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteRoleAsync_ShouldReturnFalse_WhenRoleAlreadyDeleted()
    {
        var role = new Role
        {
            Id = 2,
            Name = "Manager",
            IsActive = false,
            IsDeleted = true
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.DeleteRoleAsync(2);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteRoleAsync_ShouldReturnFalse_WhenRoleIsInactive()
    {
        var role = new Role
        {
            Id = 3,
            Name = "HR",
            IsActive = false,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.DeleteRoleAsync(3);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteRoleAsync_ShouldUpdate_UpdatedOn()
    {
        var role = new Role
        {
            Id = 4,
            Name = "Finance",
            IsActive = true,
            IsDeleted = false,
            UpdatedOn = DateTime.Now.AddDays(-5)
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var oldUpdatedOn = role.UpdatedOn;

        await _repository.DeleteRoleAsync(4);

        var dbRole = await _dbContext.Roles.FindAsync(4);

        Assert.That(dbRole, Is.Not.Null);
        Assert.That(dbRole!.UpdatedOn, Is.GreaterThan(oldUpdatedOn));
    }

    [Test]
    public async Task DeleteRoleAsync_ShouldPersistChanges()
    {
        var role = new Role
        {
            Id = 5,
            Name = "Support",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        await _repository.DeleteRoleAsync(5);

        var dbRole = await _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == 5);

        Assert.That(dbRole, Is.Not.Null);
        Assert.That(dbRole!.IsDeleted, Is.True);
        Assert.That(dbRole.IsActive, Is.False);
    }

    [Test]
    public async Task RoleExistsAsync_ShouldReturnTrue_WhenRoleExists()
    {
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.RoleExistsAsync("Admin", null);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task RoleExistsAsync_ShouldReturnFalse_WhenRoleDoesNotExist()
    {
        var result = await _repository.RoleExistsAsync("Unknown", 1);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RoleExistsAsync_ShouldBeCaseInsensitive()
    {
        var role = new Role
        {
            Id = 2,
            Name = "Manager",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.RoleExistsAsync("manager", null);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task RoleExistsAsync_ShouldTrimInput()
    {
        var role = new Role
        {
            Id = 3,
            Name = "HR",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.RoleExistsAsync("  HR  ",null);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task RoleExistsAsync_ShouldReturnFalse_WhenRoleIsInactive()
    {
        var role = new Role
        {
            Id = 4,
            Name = "Finance",
            IsActive = false,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.RoleExistsAsync("Finance",4);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RoleExistsAsync_ShouldReturnFalse_WhenRoleIsDeleted()
    {
        var role = new Role
        {
            Id = 5,
            Name = "Support",
            IsActive = true,
            IsDeleted = true
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.RoleExistsAsync("Support",5);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RoleExistsAsync_ShouldReturnFalse_WhenNameIsNull()
    {
        var result = await _repository.RoleExistsAsync(null!, 0);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RoleExistsAsync_ShouldReturnFalse_WhenNameIsEmpty()
    {
        var result = await _repository.RoleExistsAsync("", 0);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RoleExistsAsync_ShouldReturnTrue_WhenMultipleRolesExist()
    {
        _dbContext.Roles.AddRange(
            new Role { Id = 1, Name = "Admin", IsActive = true, IsDeleted = false },
            new Role { Id = 2, Name = "Manager", IsActive = true, IsDeleted = false },
            new Role { Id = 3, Name = "HR", IsActive = true, IsDeleted = false }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _repository.RoleExistsAsync("Manager",null);

        Assert.That(result, Is.True);
    }

}
