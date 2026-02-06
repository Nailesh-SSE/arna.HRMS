using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Mapping;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Infrastructure.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Services;

[TestFixture]
public class RoleServiceTest
{
    private ApplicationDbContext _dbContext =null!;
    private IRoleService _roleService =null!;
    private IMapper _mapper =null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var requestBaseRepo = new BaseRepository<Role>(_dbContext);
        var RoleRepository = new RoleRepository(requestBaseRepo);

        // ---------- Mapper ----------
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<RoleProfile>();
        });

        _mapper = mapperConfig.CreateMapper();

        _roleService = new RoleService(
            RoleRepository, 
            _mapper
        );
    }

    [Test]
    public async Task GetRoleAsync_ShouldReturnAllRoles()
    {
        // Arrange
        _dbContext.Roles.AddRange(new List<Role>
        {
            new Role { Name = "Admin", Description = "Administrator" },
            new Role { Name = "User", Description = "Regular User" }
        });
        await _dbContext.SaveChangesAsync();
        // Act
        var result = await _roleService.GetRoleAsync();
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That( result.Data!.Count, Is.EqualTo(2));
        Assert.That(result.Data!.Any(r => r.Name == "Admin"), Is.True);
        Assert.That(result.Data!.Any(r => r.Name == "User"), Is.True);
        Assert.That(result.Data!.All(r => r.Description != null), Is.True);
        Assert.That(result.Data!.All(r => r.Id > 0), Is.True);
    }

    [Test]
    public async Task GetRoleByIdAsync_ShouldReturnRole_WhenRoleExists()
    {
        // Arrange
        var role = new Role { Name = "Manager", Description = "Manager Role" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        // Act
        var result = await _roleService.GetRoleByIdAsync(role.Id);
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Name, Is.EqualTo("Manager"));
        Assert.That(result.Data!.Description, Is.EqualTo("Manager Role"));
    }

    [Test]
    public async Task GetRoleByIdAsync_ShouldReturnFail_WhenRoleDoesNotExist()
    {
        // Act
        var result = await _roleService.GetRoleByIdAsync(999);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Role not found"));
    }

    [Test]
    public async Task GetRoleByIdAsync_WhenIdIsInvalid_ShouldReturnFail()
    {
        // Act
        var result = await _roleService.GetRoleByIdAsync(0);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Invalid Role ID"));
    }

    [Test]
    public async Task CreateRoleAsync_ShouldCreateRole_WhenDataIsValid()
    {
        // Arrange
        var roleDto = new RoleDto
        {
            Name = "Tester",
            Description = "Testing Role"
        };
        // Act
        var result = await _roleService.CreateRoleAsync(roleDto);
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.GreaterThan(0));
        Assert.That(result.Data!.Name, Is.EqualTo("Tester"));
        Assert.That(result.Data!.Description, Is.EqualTo("Testing Role"));
        var roleInDb = await _dbContext.Roles.FindAsync(result.Data!.Id);
        Assert.That(roleInDb, Is.Not.Null);
        Assert.That(roleInDb!.Name, Is.EqualTo("Tester"));
    }

    [Test]
    public  async Task CreateRoleAsync_ShouldReturnFail_WhenDataIsInvalid()
    {
        // Arrange
        RoleDto? roleDto = null;

        // Act
        var result = await _roleService.CreateRoleAsync(roleDto!);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task CreateRoleAsync_ShouldReturnFail_WhenNameIsMissing()
    {
        // Arrange
        var roleDto = new RoleDto
        {
            Name = "",
            Description = "No Name Role"
        };
        // Act
        var result = await _roleService.CreateRoleAsync(roleDto);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Role name is required"));
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldReturnRole_WhenRoleExists()
    {
        // Arrange
        var role = new Role { Name = "Support", Description = "Support Role" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        // Act
        var result = await _roleService.GetRoleByNameAsync("Support");
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Name, Is.EqualTo("Support"));
        Assert.That(result.Data!.Description, Is.EqualTo("Support Role"));
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldReturnFail_WhenRoleDoesNotExist()
    {
        // Act
        var result = await _roleService.GetRoleByNameAsync("NonExistentRole");
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Role not found"));
    }

    [Test]
    public async Task GetRoleByNameAsync_WhenNameIsInvalid_ShouldReturnFail()
    {
        // Act
        var result = await _roleService.GetRoleByNameAsync("   ");
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Invalid Role"));
    }

    [Test]
    public async Task UpdateRoleAsync_WhenDataIsValid_ShouldUpdateRole()
    {
        // Arrange
        var role = new Role { Name = "OldRole", Description = "Old Description", IsActive=true, IsDeleted=false };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        Assert.That(role.Id, Is.GreaterThan(0));
        Assert.That(role.Name, Is.EqualTo("OldRole"));
        Assert.That(role.Description, Is.EqualTo("Old Description"));

        _dbContext.ChangeTracker.Clear();
        var updatedRoleDto = new RoleDto
        {
            Id = role.Id,
            Name = "UpdatedRole",
            Description = "Updated Description",
            IsActive = true,
            IsDeleted = false
        };
        // Act
        var result = await _roleService.UpdateRoleAsync(updatedRoleDto);
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(role.Id));
        Assert.That(result.Data!.Name, Is.EqualTo("UpdatedRole"));
        Assert.That(result.Data!.Description, Is.EqualTo("Updated Description"));
    }

    [Test]
    public async Task UpdateRoleAsync_WhenRoleDoesNotExist_ShouldReturnFail()
    {
        // Arrange
        var updatedRoleDto = new RoleDto
        {
            Id = 999,
            Name = "NonExistentRole",
            Description = "No Description"
        };
        // Act
        var result = await _roleService.UpdateRoleAsync(updatedRoleDto);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Failed to update role"));
    }

    [Test]
    public async Task UpdateRoleAsync_WhenDataIsInvalid_ShouldReturnFail()
    {
        // Arrange
        RoleDto? updatedRoleDto = null;
        // Act
        var result = await _roleService.UpdateRoleAsync(updatedRoleDto!);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task DeleteRoleAsync_WhenRoleExists_ShouldDeleteRole()
    {
        // Arrange
        var role = new Role { Name = "ToBeDeleted", Description = "To be deleted" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        Assert.That(role.Id, Is.GreaterThan(0));
        // Act
        var result = await _roleService.DeleteRoleAsync(role.Id);
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var roleInDb = await _dbContext.Roles.FindAsync(role.Id);
        Assert.That(roleInDb!.IsActive, Is.EqualTo(false));
        Assert.That(roleInDb.IsDeleted, Is.EqualTo(true));
    }

    [Test]
    public async Task DeleteRoleAsync_WhenRoleDoesNotExist_ShouldReturnFail()
    {
        // Act
        var result = await _roleService.DeleteRoleAsync(999);
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Role not found"));
    }

    [Test]
    public async Task RoleExistsAsync_ShouldReturnTrue_WhenRoleExists()
    {
        // Arrange
        var role = new Role { Name = "ExistingRole", Description = "Existing Role" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        // Act
        var result = await _roleService.RoleExistsAsync("ExistingRole");
        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task RoleExistsAsync_ShouldReturnFalse_WhenRoleDoesNotExist()
    {
        // Act
        var result = await _roleService.RoleExistsAsync("NonExistentRole");
        // Assert
        Assert.That(result, Is.False);
    }

}
