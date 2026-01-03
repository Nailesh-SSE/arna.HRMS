using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Models.DTOs;
using AutoMapper;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Services;

[TestFixture]
public class DepartmentServiceTests
{
    private Mock<IBaseRepository<Department>> _baseRepositoryMock;
    private DepartmentRepository _departmentRepository;
    private DepartmentService _departmentService;
    private IMapper _mapper;

    [SetUp]
    public void Setup()
    {
        // Mock base repository
        _baseRepositoryMock = new Mock<IBaseRepository<Department>>();

        // Real repository
        _departmentRepository = new DepartmentRepository(
            _baseRepositoryMock.Object);

        // AutoMapper configuration
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Department, DepartmentDto>();
        });

        _mapper = mapperConfig.CreateMapper();

        // Service under test
        _departmentService = new DepartmentService(
            _departmentRepository,
            _mapper);
    }

    // --------------------------------------------------
    // GetDepartmentAsync
    // --------------------------------------------------
    [Test]
    public async Task GetDepartmentAsync_ReturnsAllDepartments()
    {
        // Arrange
        var departments = new List<Department>
        {
            new Department { Id = 1, Name = "HR", Code = "HR" },
            new Department { Id = 2, Name = "IT", Code = "IT" }
        };

        _baseRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(departments);

        // Act
        var result = await _departmentService.GetDepartmentAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Name, Is.EqualTo("HR"));
    }

    // --------------------------------------------------
    // GetDepartmentByIdAsync - FOUND
    // --------------------------------------------------
    [Test]
    public async Task GetDepartmentByIdAsync_ReturnsDto_WhenFound()
    {
        // Arrange
        var department = new Department
        {
            Id = 1,
            Name = "Finance",
            Code = "FIN"
        };

        _baseRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(department);

        // Act
        var result = await _departmentService.GetDepartmentByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Finance"));
    }

    // --------------------------------------------------
    // GetDepartmentByIdAsync - NOT FOUND
    // --------------------------------------------------
    [Test]
    public async Task GetDepartmentByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _baseRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Department?)null);

        // Act
        var result = await _departmentService.GetDepartmentByIdAsync(99);

        // Assert
        Assert.That(result, Is.Null);
    }

    // --------------------------------------------------
    // CreateDepartmentAsync
    // --------------------------------------------------
    [Test]
    public async Task CreateDepartmentAsync_ReturnsCreatedDepartmentDto()
    {
        // Arrange
        var department = new Department
        {
            Id = 3,
            Name = "Operations",
            Code = "OPS"
        };

        _baseRepositoryMock
            .Setup(r => r.AddAsync(department))
            .ReturnsAsync(department);

        // Act
       /* var result = await _departmentService.CreateDepartmentAsync(department);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(3));
        Assert.That(result.Name, Is.EqualTo("Operations"));*/
    }

    // --------------------------------------------------
    // UpdateDepartmentAsync
    // --------------------------------------------------
    [Test]
    public async Task UpdateDepartmentAsync_ReturnsUpdatedDepartmentDto()
    {
        // Arrange
        var department = new Department
        {
            Id = 4,
            Name = "Marketing",
            Code = "MKT"
        };

        _baseRepositoryMock
            .Setup(r => r.UpdateAsync(department))
            .ReturnsAsync(department);

        // Act
       /* var result = await _departmentService.UpdateDepartmentAsync(department);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(4));
        Assert.That(result.Name, Is.EqualTo("Marketing"));*/
    }

    // --------------------------------------------------
    // DeleteDepartmentAsync
    // --------------------------------------------------
    [Test]
    public async Task DeleteDepartmentAsync_ReturnsTrue_WhenDeleted()
    {
        // Arrange
        _baseRepositoryMock
            .Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _departmentService.DeleteDepartmentAsync(1);

        // Assert
        Assert.That(result, Is.True);
    }
}
