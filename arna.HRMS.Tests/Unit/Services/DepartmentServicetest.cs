using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Services;

[TestFixture]
public class DepartmentServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private DepartmentService _departmentService = null!;
    private IMapper _mapper = null!;

    // =========================
    // SETUP
    // =========================
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var baseRepository = new BaseRepository<Department>(_dbContext);
        var departmentRepository = new DepartmentRepository(baseRepository);
        var departmentValidator = new DepartmentValidator(departmentRepository);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Department, DepartmentDto>();
            cfg.CreateMap<DepartmentDto, Department>();
        });

        _mapper = mapperConfig.CreateMapper();

        _departmentService = new DepartmentService(
            departmentRepository,
            _mapper,
            departmentValidator
        );
    }

    // =====================================================
    // GetDepartmentAsync
    // =====================================================
    [Test]
    public async Task GetDepartmentAsync_ReturnsAllDepartments()
    {
        _dbContext.Departments.AddRange(
            new Department
            {
                Name = "HR",
                Code = "CHR",
                Description = "Human Resource",
                IsActive = true,
                IsDeleted = false
            },
            new Department
            {
                Name = "IT",
                Code = "CIT",
                Description = "Information Technology",
                IsActive = true,
                IsDeleted = false
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _departmentService.GetDepartmentsAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(2));
        Assert.That(result.Data![0].Name, Is.EqualTo("IT"));
    }

    [Test]
    public async Task GetDepartmentAsync_WhenNoDepartmentsExist_ReturnsEmptyList()
    {
        var result = await _departmentService.GetDepartmentsAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetDepartmentAsync_ShouldExcludeDeletedDepartments()
    {
        _dbContext.Departments.AddRange(
            new Department
            {
                Name = "ActiveDept",
                Code = "A1",
                Description = "Active",
                IsActive = true,
                IsDeleted = false
            },
            new Department
            {
                Name = "DeletedDept",
                Code = "D1",
                Description = "Deleted",
                IsActive = false,
                IsDeleted = true
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _departmentService.GetDepartmentsAsync();

        Assert.That(result.Data!.Count, Is.EqualTo(1));
        Assert.That(result.Data![0].Name, Is.EqualTo("ActiveDept"));
    }

    [Test]
    public async Task GetDepartmentAsync_ShouldReturnInDescendingOrder()
    {
        var dept1 = new Department
        {
            Name = "First",
            Code = "F1",
            Description = "First Dept",
            IsActive = true,
            IsDeleted = false
        };

        var dept2 = new Department
        {
            Name = "Second",
            Code = "S1",
            Description = "Second Dept",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.AddRange(dept1, dept2);
        await _dbContext.SaveChangesAsync();

        var result = await _departmentService.GetDepartmentsAsync();

        Assert.That(result.Data![0].Id, Is.GreaterThan(result.Data![1].Id));
    }


    // =====================================================
    // GetDepartmentByIdAsync
    // =====================================================
    [Test]
    public async Task GetDepartmentByIdAsync_ReturnsDto_WhenFound()
    {
        var department = new Department
        {
            Name = "Finance",
            Code = "FIN",
            Description = "Finance Department",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        var result = await _departmentService.GetDepartmentByIdAsync(department.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Name, Is.EqualTo("Finance"));
    }

    [Test]
    public async Task GetDepartmentByIdAsync_ReturnsFail_WhenNotFound()
    {
        var result = await _departmentService.GetDepartmentByIdAsync(999);

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public async Task GetDepartmentByIdAsync_WhenIdZero_ReturnsInvalidIdFail()
    {
        var result = await _departmentService.GetDepartmentByIdAsync(0);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid department ID"));
    }

    [Test]
    public async Task GetDepartmentByIdAsync_WhenIdNegativeOrZero_ReturnsInvalidIdFail()
    {
        var result = await _departmentService.GetDepartmentByIdAsync(-5);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid department ID"));

        var result2 = await _departmentService.GetDepartmentByIdAsync(0);

        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Is.EqualTo("Invalid department ID"));
    }

    [Test]
    public async Task GetDepartmentByIdAsync_WhenDeleted_ReturnsFail()
    {
        var department = new Department
        {
            Name = "DeletedDept",
            Code = "DD",
            Description = "Deleted",
            IsActive = false,
            IsDeleted = true
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        var result = await _departmentService.GetDepartmentByIdAsync(department.Id);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Department not found"));
    }

    [Test]
    public async Task GetDepartmentByIdAsync_WhenMappingReturnsNull_ReturnsFail()
    {
        var department = new Department
        {
            Name = "TestDept",
            Code = "TD",
            Description = "Test",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        // Create service with broken mapper
        var mockMapper = new Mock<IMapper>();
        mockMapper.Setup(m => m.Map<DepartmentDto>(It.IsAny<Department>()))
                  .Returns((DepartmentDto)null!);

        var baseRepo = new BaseRepository<Department>(_dbContext);
        var repo = new DepartmentRepository(baseRepo);
        var Validat = new DepartmentValidator(repo);

        var serviceWithBrokenMapper = new DepartmentService(repo, mockMapper.Object, Validat);

        var result = await serviceWithBrokenMapper.GetDepartmentByIdAsync(department.Id);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to Find department."));
    }


    [Test]
    public async Task CreateDepartmentAsync_ReturnsCreatedDepartmentDto()
    {
        var dto = new DepartmentDto
        {
            Name = "Operations",
            Code = "OPS",
            Description = "Operations Department"
        };

        var result = await _departmentService.CreateDepartmentAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Name, Is.EqualTo("Operations"));
        Assert.That(await _dbContext.Departments.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task CreateDepartmentAsync_WhenDtoIsNull_ReturnsFail()
    {
        var result = await _departmentService.CreateDepartmentAsync(null!);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task CreateDepartmentAsync_WhenNameIsEmpty_ReturnsFail()
    {
        var dto = new DepartmentDto
        {
            Name = "",
            Code = "IT",
            Description = "Test"
        };

        var result = await _departmentService.CreateDepartmentAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Department Name is required"));
    }

    [Test]
    public async Task CreateDepartmentAsync_WhenCodeIsEmpty_ReturnsFail()
    {
        var dto = new DepartmentDto
        {
            Name = "IT",
            Code = "",
            Description = "Test"
        };

        var result = await _departmentService.CreateDepartmentAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Department Code is required"));
    }

    [Test]
    public async Task CreateDepartmentAsync_WhenDepartmentAlreadyExists_ReturnsFail()
    {
        _dbContext.Departments.Add(new Department
        {
            Name = "HR",
            Code = "HR01",
            Description = "HR Dept",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        var dto = new DepartmentDto
        {
            Name = "HR",
            Code = "NEW",
            Description = "Duplicate"
        };

        var result = await _departmentService.CreateDepartmentAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Department already exists"));
    }

    [Test]
    public async Task CreateDepartmentAsync_WhenMappingReturnsNull_ReturnsFail()
    {
        var dto = new DepartmentDto
        {
            Name = "TestDept",
            Code = "TD",
            Description = "Test"
        };

        var mockMapper = new Mock<IMapper>();

        // First mapping: DTO -> Entity (valid)
        mockMapper.Setup(m => m.Map<Department>(It.IsAny<DepartmentDto>()))
                  .Returns(new Department
                  {
                      Name = "TestDept",
                      Code = "TD",
                      Description = "Test",
                      IsActive = true,
                      IsDeleted = false
                  });

        // Second mapping: Entity -> DTO (force null)
        mockMapper.Setup(m => m.Map<DepartmentDto>(It.IsAny<Department>()))
                  .Returns((DepartmentDto)null!);

        var baseRepo = new BaseRepository<Department>(_dbContext);
        var repo = new DepartmentRepository(baseRepo);
        var validator = new DepartmentValidator(repo);

        var serviceWithBrokenMapper =
            new DepartmentService(repo, mockMapper.Object, validator);

        // ✅ CALL THE CORRECT SERVICE
        var result = await serviceWithBrokenMapper.CreateDepartmentAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to create department."));
    }


    [Test]
    public async Task UpdateDepartmentAsync_ReturnsUpdatedDepartmentDto()
    {
        var department = new Department
        {
            Name = "Marketing",
            Code = "MKT",
            Description = "Marketing Dept",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(department).State = EntityState.Detached;

        var dto = new DepartmentDto
        {
            Id = department.Id,
            Name = "Marketing Updated",
            Code = "MKT",
            Description = "Updated Marketing Dept"
        };

        var result = await _departmentService.UpdateDepartmentAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Name, Is.EqualTo("Marketing Updated"));
    }

    [Test]
    public async Task UpdateDepartmentAsync_WhenDtoIsNull_ReturnsFail()
    {
        var result = await _departmentService.UpdateDepartmentAsync(null!);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task UpdateDepartmentAsync_WhenIdInvalid_ReturnsFail()
    {
        var dto = new DepartmentDto
        {
            Id = 0,
            Name = "Test",
            Code = "T01"
        };

        var result = await _departmentService.UpdateDepartmentAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Department ID"));
    }

    [Test]
    public async Task UpdateDepartmentAsync_WhenDepartmentNotFound_ReturnsFail()
    {
        var dto = new DepartmentDto
        {
            Id = 999,
            Name = "Test",
            Code = "T01",
            Description = "Test"
        };

        var result = await _departmentService.UpdateDepartmentAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Data not found"));
    }

    [Test]
    public async Task UpdateDepartmentAsync_WhenNameEmpty_ReturnsFail()
    {
        var department = new Department
        {
            Name = "IT",
            Code = "IT01",
            Description = "IT Dept",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        var dto = new DepartmentDto
        {
            Id = department.Id,
            Name = "",
            Code = "IT01"
        };

        var result = await _departmentService.UpdateDepartmentAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Department Name is required"));
    }

    [Test]
    public async Task UpdateDepartmentAsync_WhenCodeEmpty_ReturnsFail()
    {
        var department = new Department
        {
            Name = "IT",
            Code = "IT01",
            Description = "IT Dept",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        var dto = new DepartmentDto
        {
            Id = department.Id,
            Name = "IT Updated",
            Code = ""
        };

        var result = await _departmentService.UpdateDepartmentAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Department Code is required"));
    }

    [Test]
    public async Task UpdateDepartmentAsync_WhenDuplicateName_ReturnsFail()
    {
        var dept1 = new Department
        {
            Name = "HR",
            Code = "HR01",
            Description = "HR",
            IsActive = true,
            IsDeleted = false
        };

        var dept2 = new Department
        {
            Name = "IT",
            Code = "IT01",
            Description = "IT",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.AddRange(dept1, dept2);
        await _dbContext.SaveChangesAsync();

        var dto = new DepartmentDto
        {
            Id = dept2.Id,
            Name = "HR",  // duplicate
            Code = "IT01"
        };

        var result = await _departmentService.UpdateDepartmentAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Department already exists"));
    }

    [Test]
    public async Task UpdateDepartmentAsync_WhenMappingReturnsNull_ReturnsFail()
    {
        // Arrange
        var department = new Department
        {
            Name = "Test",
            Code = "T01",
            Description = "Test",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        var dto = new DepartmentDto
        {
            Id = department.Id,
            Name = "Test Updated",
            Code = "T01",
            Description = "Test"
        };

        var mockMapper = new Mock<IMapper>();

        // DTO -> Entity mapping
        mockMapper.Setup(m => m.Map<Department>(It.IsAny<DepartmentDto>()))
                  .Returns(new Department
                  {
                      Id = department.Id,
                      Name = "Test Updated",
                      Code = "T01",
                      Description = "Updated",
                      IsActive = true,
                      IsDeleted = false
                  });

        // Entity -> DTO mapping returns null (simulate broken mapping)
        mockMapper.Setup(m => m.Map<DepartmentDto>(It.IsAny<Department>()))
                  .Returns((DepartmentDto)null!);

        var baseRepo = new BaseRepository<Department>(_dbContext);
        var repo = new DepartmentRepository(baseRepo);
        var validator = new DepartmentValidator(repo);

        var serviceWithBrokenMapper = new DepartmentService(repo, mockMapper.Object, validator);

        // Act  ✅ CALL CORRECT SERVICE
        var result = await serviceWithBrokenMapper.UpdateDepartmentAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to Update department."));
    }


    // =====================================================
    // DeleteDepartmentAsync
    // =====================================================
    [Test]
    public async Task DeleteDepartmentAsync_ReturnsTrue_WhenDeleted()
    {
        var department = new Department
        {
            Name = "HR",
            Code = "CHR",
            Description = "Human Resource",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        var result = await _departmentService.DeleteDepartmentAsync(department.Id);
        var updated = await _dbContext.Departments.FindAsync(department.Id);
        Assert.That(updated!.IsActive, Is.False);
        Assert.That(updated.IsDeleted, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That(result.IsSuccess, Is.True);

    }

    [Test]
    public async Task DeleteDepartmentAsync_ReturnFail_WhereIdInNegativeOrZero()
    {
        var result = await _departmentService.DeleteDepartmentAsync(0);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Department not found"));

        var result2 = await _departmentService.DeleteDepartmentAsync(-5);

        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Is.EqualTo("Department not found"));
    }

    [Test]
    public async Task DeleteDepartmentAsync_WhenDepartmentNotFound_ReturnsFail()
    {
        var result = await _departmentService.DeleteDepartmentAsync(999);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Department not found"));
    }

    [Test]
    public async Task DeleteDepartmentAsync_WhenAlreadyDeleted_ReturnsFail()
    {
        var department = new Department
        {
            Name = "IT",
            Code = "IT01",
            Description = "IT Dept",
            IsActive = false,
            IsDeleted = true
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        var result = await _departmentService.DeleteDepartmentAsync(department.Id);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Department not found"));
    }
}
