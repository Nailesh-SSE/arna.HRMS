using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using arna.HRMS.Infrastructure.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Department, DepartmentDto>();
            cfg.CreateMap<DepartmentDto, Department>();
        });

        _mapper = mapperConfig.CreateMapper();

        _departmentService = new DepartmentService(
            departmentRepository,
            _mapper);
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

        var result = await _departmentService.GetDepartmentAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(2));
        Assert.That(result.Data![0].Name, Is.EqualTo("IT"));
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

        
    }
}
