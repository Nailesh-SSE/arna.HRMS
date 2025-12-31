using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Repositories;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Repositories;

[TestFixture]
public class DepartmentRepositoryTests
{
    private Mock<IBaseRepository<Department>> _baseRepositoryMock;
    private DepartmentRepository _departmentRepository;

    [SetUp]
    public void Setup()
    {
        _baseRepositoryMock = new Mock<IBaseRepository<Department>>();
        _departmentRepository = new DepartmentRepository(
            _baseRepositoryMock.Object);
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
        var result = await _departmentRepository.GetDepartmentAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    // --------------------------------------------------
    // GetDepartmentByIdAsync - FOUND
    // --------------------------------------------------
    [Test]
    public async Task GetDepartmentByIdAsync_ReturnsDepartment_WhenFound()
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
        var result = await _departmentRepository.GetDepartmentByIdAsync(1);

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
        var result = await _departmentRepository.GetDepartmentByIdAsync(99);

        // Assert
        Assert.That(result, Is.Null);
    }

    // --------------------------------------------------
    // CreateDepartmentAsync
    // --------------------------------------------------
    [Test]
    public async Task CreateDepartmentAsync_CallsAddAsync()
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
        var result = await _departmentRepository
            .CreateDepartmentAsync(department);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(3));

        _baseRepositoryMock.Verify(
            r => r.AddAsync(department),
            Times.Once);
    }

    // --------------------------------------------------
    // UpdateDepartmentAsync
    // --------------------------------------------------
    [Test]
    public async Task UpdateDepartmentAsync_CallsUpdateAsync()
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
        var result = await _departmentRepository
            .UpdateDepartmentAsync(department);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(4));

        _baseRepositoryMock.Verify(
            r => r.UpdateAsync(department),
            Times.Once);
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
        var result = await _departmentRepository
            .DeleteDepartmentAsync(1);

        // Assert
        Assert.That(result, Is.True);

        _baseRepositoryMock.Verify(
            r => r.DeleteAsync(1),
            Times.Once);
    }
}
