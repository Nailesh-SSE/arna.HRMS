using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common.Interfaces;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Repositories;

[TestFixture]
public class EmployeeRepositoryTests
{
    private Mock<IBaseRepository<Employee>> _baseRepositoryMock;
    private EmployeeRepository _employeeRepository;

    [SetUp]
    public void Setup()
    {
        _baseRepositoryMock = new Mock<IBaseRepository<Employee>>();
        _employeeRepository = new EmployeeRepository(
            _baseRepositoryMock.Object);
    }

    // --------------------------------------------------
    // GetEmployeesAsync
    // --------------------------------------------------
    [Test]
    public async Task GetEmployeesAsync_ReturnsAllEmployees()
    {
        // Arrange
        var employees = new List<Employee>
        {
            new Employee { Id = 1, FirstName = "John", LastName = "Doe" },
            new Employee { Id = 2, FirstName = "Jane", LastName = "Smith" }
        };

        _baseRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(employees.AsEnumerable());

        // Act
        var result = await _employeeRepository.GetEmployeesAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));

        _baseRepositoryMock.Verify(
            r => r.GetAllAsync(),
            Times.Once);
    }

    // --------------------------------------------------
    // GetEmployeeByIdAsync - FOUND
    // --------------------------------------------------
    [Test]
    public async Task GetEmployeeByIdAsync_ReturnsEmployee_WhenFound()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Brown"
        };

        _baseRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(employee);

        // Act
        var result = await _employeeRepository.GetEmployeeByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
        Assert.That(result.FirstName, Is.EqualTo("Alice"));

        _baseRepositoryMock.Verify(
            r => r.GetByIdAsync(1),
            Times.Once);
    }

    // --------------------------------------------------
    // GetEmployeeByIdAsync - NOT FOUND
    // --------------------------------------------------
    [Test]
    public async Task GetEmployeeByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _baseRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _employeeRepository.GetEmployeeByIdAsync(99);

        // Assert
        Assert.That(result, Is.Null);

        _baseRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<int>()),
            Times.Once);
    }

    // --------------------------------------------------
    // CreateEmployeeAsync
    // --------------------------------------------------
    [Test]
    public async Task CreateEmployeeAsync_CallsAddAsync()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 3,
            FirstName = "Mark",
            LastName = "Taylor"
        };

        _baseRepositoryMock
            .Setup(r => r.AddAsync(employee))
            .ReturnsAsync(employee);

        // Act
        var result = await _employeeRepository.CreateEmployeeAsync(employee);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(3));

        _baseRepositoryMock.Verify(
            r => r.AddAsync(employee),
            Times.Once);
    }

    // --------------------------------------------------
    // UpdateEmployeeAsync
    // --------------------------------------------------
    [Test]
    public async Task UpdateEmployeeAsync_CallsUpdateAsync()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 4,
            FirstName = "Emily",
            LastName = "Clark"
        };

        _baseRepositoryMock
            .Setup(r => r.UpdateAsync(employee))
            .ReturnsAsync(employee);

        // Act
        var result = await _employeeRepository.UpdateEmployeeAsync(employee);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(4));

        _baseRepositoryMock.Verify(
            r => r.UpdateAsync(employee),
            Times.Once);
    }

    // --------------------------------------------------
    // DeleteEmployeeAsync
    // --------------------------------------------------
    [Test]
    public async Task DeleteEmployeeAsync_ReturnsTrue_WhenDeleted()
    {
        // Arrange
        _baseRepositoryMock
            .Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _employeeRepository.DeleteEmployeeAsync(1);

        // Assert
        Assert.That(result, Is.True);

        _baseRepositoryMock.Verify(
            r => r.DeleteAsync(1),
            Times.Once);
    }
}
