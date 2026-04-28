using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Mapping;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Infrastructure.Validators;
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
        var roleValidator = new RoleValidator(RoleRepository);

        // ---------- Mapper ----------
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<RoleProfile>();
        });

        _mapper = mapperConfig.CreateMapper();

        _roleService = new RoleService(
            RoleRepository, 
            _mapper,
            roleValidator
        );
    }

    [Test]
    public async Task GetRoleAsync_ShouldHandleMultipleRecords()
    {
        // Arrange
        for (int i = 1; i <= 50; i++)
        {
            _dbContext.Roles.Add(new Role
            {
                Name = $"Role{i}",
                Description = $"Description{i}"
            });
        }

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _roleService.GetRolesAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(50));
    }


    [Test]
    public async Task GetRoleAsync_ShouldNotReturnDeletedRoles()
    {
        // Arrange
        _dbContext.Roles.AddRange(
            new Role { Name = "ActiveRole", IsActive = true, IsDeleted = false },
            new Role { Name = "DeletedRole", IsActive = false, IsDeleted = true }
        );

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _roleService.GetRolesAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Any(r => r.Name == "DeletedRole"), Is.False);
    }


    [Test]
    public async Task GetRoleAsync_ShouldReturnEmptyList_WhenNoRolesExist()
    {
        // Arrange
        // (No roles added to DB)

        // Act
        var result = await _roleService.GetRolesAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.EqualTo(0));
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
        var result = await _roleService.GetRolesAsync();
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
    public async Task GetRoleByIdAsync_ShouldReturnFail_WhenMapperReturnsNull()
    {
        // Arrange
        var role = new Role
        {
            Name = "TestRole",
            Description = "Test"
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        // Create REAL repository
        var baseRepo = new BaseRepository<Role>(_dbContext);
        var repository = new RoleRepository(baseRepo);

        // Mock ONLY mapper
        var mapperMock = new Mock<IMapper>();
        mapperMock
            .Setup(m => m.Map<RoleDto>(It.IsAny<Role>()))
            .Returns((RoleDto?)null);

        var validator = new RoleValidator(repository);

        var service = new RoleService(
            repository,
            mapperMock.Object,
            validator
        );

        // Act
        var result = await service.GetRoleByIdAsync(role.Id);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("No Data Found"));
    }



    [Test]
    public async Task GetRoleByIdAsync_ShouldReturnExactFailMessage_ForInvalidId()
    {
        // Act
        var result = await _roleService.GetRoleByIdAsync(-10);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Role ID"));
    }

    [Test]
    public async Task GetRoleByIdAsync_ShouldReturnAllMappedProperties()
    {
        // Arrange
        var role = new Role
        {
            Name = "Finance",
            Description = "Finance Role",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _roleService.GetRoleByIdAsync(role.Id);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Id, Is.EqualTo(role.Id));
        Assert.That(result.Data!.Name, Is.EqualTo(role.Name));
        Assert.That(result.Data!.Description, Is.EqualTo(role.Description));
        Assert.That(result.Data!.IsActive, Is.EqualTo(role.IsActive));
        Assert.That(result.Data!.IsDeleted, Is.EqualTo(role.IsDeleted));
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
    public async Task CreateRoleAsync_ShouldReturnFail_WhenNameExceedsMaxLength()
    {
        // Arrange
        var roleDto = new RoleDto
        {
            Name = new string('A', 101),
            Description = "Too long name"
        };

        // Act
        var result = await _roleService.CreateRoleAsync(roleDto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Role name must be at most 100 characters"));
    }

    [Test]
    public async Task CreateRoleAsync_ShouldReturnFail_WhenRoleNameAlreadyExists()
    {
        // Arrange
        _dbContext.Roles.Add(new Role
        {
            Name = "Admin",
            Description = "Existing Role"
        });

        await _dbContext.SaveChangesAsync();

        var roleDto = new RoleDto
        {
            Name = "Admin",
            Description = "Duplicate Role"
        };

        // Act
        var result = await _roleService.CreateRoleAsync(roleDto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Role name already exists"));
    }

    [Test]
    public async Task CreateRoleAsync_ShouldReturnMultipleErrors_WhenMultipleValidationFails()
    {
        // Arrange
        _dbContext.Roles.Add(new Role
        {
            Name = new string('A', 101)
        });

        await _dbContext.SaveChangesAsync();

        var roleDto = new RoleDto
        {
            Name = new string('A', 101)
        };

        // Act
        var result = await _roleService.CreateRoleAsync(roleDto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message.Contains("Role name must be at most 100 characters"));
    }

    [Test]
    public async Task CreateRoleAsync_ShouldReturnFail_WhenMapperReturnsNull()
    {
        // Arrange
        var roleDto = new RoleDto
        {
            Name = "NewRole",
            Description = "Test"
        };

        var baseRepo = new BaseRepository<Role>(_dbContext);
        var repository = new RoleRepository(baseRepo);

        var mapperMock = new Mock<IMapper>();
        mapperMock
            .Setup(m => m.Map<Role>(It.IsAny<RoleDto>()))
            .Returns(new Role { Name = "NewRole" });

        mapperMock
            .Setup(m => m.Map<RoleDto>(It.IsAny<Role>()))
            .Returns((RoleDto?)null);

        var validator = new RoleValidator(repository);

        var service = new RoleService(repository, mapperMock.Object, validator);

        // Act
        var result = await service.CreateRoleAsync(roleDto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Fail to create Role"));
    }

    [Test]
    public async Task CreateRoleAsync_ShouldReturnSuccessMessage()
    {
        // Arrange
        var roleDto = new RoleDto
        {
            Name = "HR",
            Description = "Human Resource"
        };

        // Act
        var result = await _roleService.CreateRoleAsync(roleDto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Role created successfully"));
    }

    [Test]
    public async Task CreateRoleAsync_ShouldAllowNullDescription()
    {
        // Arrange
        var roleDto = new RoleDto
        {
            Name = "NoDescriptionRole",
            Description = null
        };

        // Act
        var result = await _roleService.CreateRoleAsync(roleDto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Description, Is.Null);
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
    public async Task GetRoleByNameAsync_WhenNameIsNull_ShouldReturnFail()
    {
        // Act
        var result = await _roleService.GetRoleByNameAsync(null!);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Role name is required"));
    }

    [Test]
    public async Task GetRoleByNameAsync_WhenNameIsEmpty_ShouldReturnFail()
    {
        // Act
        var result = await _roleService.GetRoleByNameAsync("");

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Role name is required"));
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldReturnFail_WhenMapperReturnsNull()
    {
        // Arrange
        var role = new Role
        {
            Name = "Finance",
            Description = "Finance Role"
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var baseRepo = new BaseRepository<Role>(_dbContext);
        var repository = new RoleRepository(baseRepo);

        var mapperMock = new Mock<IMapper>();
        mapperMock
            .Setup(m => m.Map<RoleDto>(It.IsAny<Role>()))
            .Returns((RoleDto?)null);

        var validator = new RoleValidator(repository);

        var service = new RoleService(repository, mapperMock.Object, validator);

        // Act
        var result = await service.GetRoleByNameAsync("Finance");

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("No Data Found"));
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldReturnAllMappedProperties()
    {
        // Arrange
        var role = new Role
        {
            Name = "HR",
            Description = "Human Resource",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _roleService.GetRoleByNameAsync("HR");

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Name, Is.EqualTo(role.Name));
        Assert.That(result.Data!.Description, Is.EqualTo(role.Description));
        Assert.That(result.Data!.IsActive, Is.EqualTo(role.IsActive));
        Assert.That(result.Data!.IsDeleted, Is.EqualTo(role.IsDeleted));
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldNotModifyDatabase()
    {
        // Arrange
        var role = new Role
        {
            Name = "Audit",
            Description = "Audit Role",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var originalUpdatedOn = role.UpdatedOn;

        // Act
        var result = await _roleService.GetRoleByNameAsync("Audit");

        // Assert
        var roleInDb = await _dbContext.Roles.FindAsync(role.Id);

        Assert.That(roleInDb!.UpdatedOn, Is.EqualTo(originalUpdatedOn));
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
        Assert.That(result.Message, Is.EqualTo("Role name is required"));
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
        Assert.That(result.Message, Is.EqualTo("No Data Found"));
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
    public async Task UpdateRoleAsync_WhenIdIsInvalid_ShouldReturnFail()
    {
        // Arrange
        var dto = new RoleDto
        {
            Id = 0,
            Name = "Test",
            Description = "Test"
        };

        // Act
        var result = await _roleService.UpdateRoleAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid role ID"));
    }

    [Test]
    public async Task UpdateRoleAsync_WhenNameIsEmpty_ShouldReturnFail()
    {
        // Arrange
        var role = new Role { Name = "ExistingRole" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = "",
            Description = "Updated"
        };

        // Act
        var result = await _roleService.UpdateRoleAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Role name is required"));
    }

    [Test]
    public async Task UpdateRoleAsync_WhenNameExceedsMaxLength_ShouldReturnFail()
    {
        // Arrange
        var role = new Role { Name = "ExistingRole" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = new string('A', 101),
            Description = "Updated"
        };

        // Act
        var result = await _roleService.UpdateRoleAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Role name must be at most 100 characters"));
    }

    [Test]
    public async Task UpdateRoleAsync_WhenRoleNameAlreadyExists_ShouldReturnFail()
    {
        // Arrange
        var role1 = new Role { Name = "Admin" };
        var role2 = new Role { Name = "User" };

        _dbContext.Roles.AddRange(role1, role2);
        await _dbContext.SaveChangesAsync();

        var dto = new RoleDto
        {
            Id = role2.Id,
            Name = "Admin", // duplicate
            Description = "Updated"
        };

        // Act
        var result = await _roleService.UpdateRoleAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Role name already exists"));
    }

    [Test]
    public async Task UpdateRoleAsync_ShouldReturnFail_WhenMapperReturnsNull()
    {
        // Arrange
        var role = new Role { Name = "Role1" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = "UpdatedRole"
        };

        var baseRepo = new BaseRepository<Role>(_dbContext);
        var repository = new RoleRepository(baseRepo);

        var mapperMock = new Mock<IMapper>();

        mapperMock
            .Setup(m => m.Map<Role>(It.IsAny<RoleDto>()))
            .Returns(new Role { Id = role.Id, Name = "UpdatedRole" });

        mapperMock
            .Setup(m => m.Map<RoleDto>(It.IsAny<Role>()))
            .Returns((RoleDto?)null);

        var validator = new RoleValidator(repository);

        var service = new RoleService(repository, mapperMock.Object, validator);

        // Act
        var result = await service.UpdateRoleAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Fail to Update Role"));
    }

    [Test]
    public async Task UpdateRoleAsync_ShouldReturnSuccessMessage()
    {
        // Arrange
        var role = new Role { Name = "OldRole" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = "NewRole"
        };

        // Act
        var result = await _roleService.UpdateRoleAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Role updated successfully"));
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
    public async Task DeleteRoleAsync_WhenIdIsInvalid_ShouldReturnFail()
    {
        // Act
        var result = await _roleService.DeleteRoleAsync(0);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Role not found"));
    }

    [Test]
    public async Task DeleteRoleAsync_ShouldReturnSuccessMessage()
    {
        // Arrange
        var role = new Role { Name = "TempRole" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _roleService.DeleteRoleAsync(role.Id);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Role deleted successfully"));
    }

    [Test]
    public async Task DeleteRoleAsync_ShouldReturnTrue_WhenDeleted()
    {
        // Arrange
        var role = new Role { Name = "DeleteTest" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _roleService.DeleteRoleAsync(role.Id);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);
    }

    [Test]
    public async Task DeleteRoleAsync_ShouldNotModifyDatabase_WhenRoleNotFound()
    {
        // Arrange
        var initialCount = _dbContext.Roles.Count();

        // Act
        var result = await _roleService.DeleteRoleAsync(999);

        // Assert
        var afterCount = _dbContext.Roles.Count();

        Assert.That(initialCount, Is.EqualTo(afterCount));
    }

}
