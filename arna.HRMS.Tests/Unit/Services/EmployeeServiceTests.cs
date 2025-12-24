using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Models.DTOs;
using AutoMapper;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Services;

[TestFixture]
public class EmployeeServiceTests
{
    private Mock<IBaseRepository<Employee>> _baseRepositoryMock;
    private EmployeeRepository _employeeRepository;
    private IMapper _mapper;
    private EmployeeService _employeeService;

    [SetUp]
    public void Setup()
    {
        // 1️⃣ Mock the base repository, which is used by EmployeeRepository internally
        _baseRepositoryMock = new Mock<IBaseRepository<Employee>>();

        // 2️⃣ Real EmployeeRepository, inject the mocked base repository
        _employeeRepository = new EmployeeRepository(_baseRepositoryMock.Object);

        // 3️⃣ Configure AutoMapper as in the real app
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        });
        _mapper = mapperConfig.CreateMapper();

        // 4️⃣ Create the service
        _employeeService = new EmployeeService(_employeeRepository, _mapper);
    }

    [Test]
    public async Task GetEmployeesAsync_ReturnsEmployeeDtoList()
    {
        // Arrange: mock base repository method
        var employees = new List<Employee>
        {
            new Employee { Id = 1, FirstName = "John", LastName = "Doe" },
            new Employee { Id = 2, FirstName = "Jane", LastName = "Smith" }
        };

        _baseRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(employees);

        // Act
        var result = await _employeeService.GetEmployeesAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(employees.Count));
        Assert.That(result[0].FullName, Is.EqualTo(employees[0].FirstName + " " + employees[0].LastName));
    }

    [Test]
    public async Task GetEmployeeByIdAsync_WhenEmployeeExists_ReturnsEmployeeDto()
    {
        var employee = new Employee { Id = 1, FirstName = "John", LastName = "Doe" };
        _baseRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);

        var result = await _employeeService.GetEmployeeByIdAsync(1);

        Assert.That(result.FullName, Is.EqualTo("John Doe"));
    }

    [Test]
    public async Task GetEmployeeByIdAsync_WhenEmployeeDoesNotExist_ReturnsNull()
    {
        _baseRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Employee)null);

        var result = await _employeeService.GetEmployeeByIdAsync(99);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateEmployeeAsync_ReturnsCreatedEmployeeDto()
    {
        var employee = new Employee { Id = 1, FirstName = "John", LastName = "Doe" };

        // ✅ Mock the underlying AddAsync, NOT EmployeeRepository.CreateEmployeeAsync
        _baseRepositoryMock.Setup(r => r.AddAsync(employee)).ReturnsAsync(employee);

        var result = await _employeeService.CreateEmployeeAsync(employee);

        Assert.That(result.FullName, Is.EqualTo("John Doe"));
    }

    [Test]
    public async Task UpdateEmployeeAsync_ReturnsUpdatedEmployeeDto()
    {
        var employee = new Employee { Id = 1, FirstName = "Updated", LastName = "User" };

        // ✅ Mock the underlying UpdateAsync
        _baseRepositoryMock.Setup(r => r.UpdateAsync(employee)).ReturnsAsync(employee);

        var result = await _employeeService.UpdateEmployeeAsync(employee);

        Assert.That(result.FullName, Is.EqualTo("Updated User"));
    }

    [Test]
    public async Task DeleteEmployeeAsync_ReturnsTrue_WhenDeleted()
    {
        // ✅ Mock the underlying DeleteAsync
        _baseRepositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _employeeService.DeleteEmployeeAsync(1);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DeleteEmployeeAsync_ReturnsFalse_WhenEmployeeNotFound()
    {
        _baseRepositoryMock.Setup(r => r.DeleteAsync(99)).ReturnsAsync(false);

        var result = await _employeeService.DeleteEmployeeAsync(99);

        Assert.That(result, Is.False);
    }
}
