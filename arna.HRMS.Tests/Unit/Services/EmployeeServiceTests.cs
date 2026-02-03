using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
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
public class EmployeeServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private EmployeeService _employeeService = null!;
    private Mock<IUserServices> _userServicesMock = null!;
    private Mock<IRoleService> _roleServiceMock = null!;
    private IMapper _mapper = null!;

    // -------------------- SETUP --------------------

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "IT",
            Code = "IT",
            Description = "Information Technology"
        });

        await _dbContext.SaveChangesAsync();

        var baseRepository = new BaseRepository<Employee>(_dbContext);
        var employeeRepository = new EmployeeRepository(baseRepository);

        _userServicesMock = new Mock<IUserServices>();
        _roleServiceMock = new Mock<IRoleService>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            // DTO → Entity (IGNORE navigation properties)
            cfg.CreateMap<EmployeeDto, Employee>()
                .ForMember(dest => dest.Department, opt => opt.Ignore()) 
                .ForMember(dest => dest.Manager, opt => opt.Ignore());

            // Entity → DTO
            cfg.CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.DepartmentCode,
                    opt => opt.MapFrom(src => src.Department != null ? src.Department.Code : ""))
                .ForMember(dest => dest.ManagerFullName,
                    opt => opt.MapFrom(src =>
                        src.Manager != null
                            ? $"{src.Manager.FirstName} {src.Manager.LastName}"
                            : ""));
        });


        _mapper = mapperConfig.CreateMapper();

        _employeeService = new EmployeeService(
            employeeRepository,
            _mapper,
            _userServicesMock.Object,
            _roleServiceMock.Object
        );
    }

    // -------------------- GET ALL --------------------

    [Test]
    public async Task GetEmployeesAsync_ReturnsEmployeeDtoList()
    {
        _dbContext.Employees.AddRange(
            new Employee
            {
                EmployeeNumber = "Emp001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@a.com",
                PhoneNumber = "1111111111",
                Position = "Developer",
                Salary = 50000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-25)
            },
            new Employee
            {
                EmployeeNumber = "Emp002",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@b.com",
                PhoneNumber = "2222002222",
                Position = "Tester",
                Salary = 4000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-24)
            },
            new Employee
            {
                EmployeeNumber = "Emp003",
                FirstName = "Pratham",
                LastName = "Smith",
                Email = "pratham@b.com",
                PhoneNumber = "2222222222",
                Position = "Tester",
                Salary = 45000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-24)
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.GetEmployeesAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(3));
        Assert.That(result.Data![2].FullName, Is.EqualTo("John Doe"));
        Assert.That(result.Data![1].FullName, Is.EqualTo("Jane Smith"));
        Assert.That(result.Data![0].FullName, Is.EqualTo("Pratham Smith"));
    }

    // -------------------- GET BY ID --------------------

    [Test]
    public async Task GetEmployeeByIdAsync_WhenExists_ReturnsEmployee()
    {
        var employee = new Employee
        {
            EmployeeNumber = "Emp003",
            FirstName = "Alex",
            LastName = "Brown",
            Email = "alex@test.com",
            PhoneNumber = "3333333333",
            Position = "Manager",
            Salary = 80000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-30)
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.GetEmployeeByIdAsync(employee.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.FullName, Is.EqualTo("Alex Brown"));
    }

    [Test]
    public async Task GetEmployeeByIdAsync_WhenNotExists_ReturnsFail()
    {
        var result = await _employeeService.GetEmployeeByIdAsync(999);

        Assert.That(result.IsSuccess, Is.False);
    }

    // -------------------- CREATE --------------------

    [Test]
    public async Task CreateEmployeeAsync_ReturnsCreatedEmployeeDto()
    {
        var dto = new EmployeeDto
        {
            FirstName = "Chris",
            LastName = "Evans",
            DepartmentName = "INFORMATION TECHNOLOGY",
            Email = "chris@test.com",
            PhoneNumber = "4444444444",
            DateOfBirth = DateTime.Now.AddYears(-28),
            HireDate = DateTime.Now,
            DepartmentId = 1,
            DepartmentCode = "IT",
            Position = "Developer",
            Salary = 60000
        };

        _roleServiceMock.Setup(r => r.GetRoleByNameAsync("Employee"))
            .ReturnsAsync(ServiceResult<RoleDto>.Success(new RoleDto { Id = 2, Name = "Employee" }));
        var uDto = new UserDto
        {
            FullName = "Chris Evans",
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            RoleId= 2
        };
        _userServicesMock.Setup(u => u.CreateUserAsync(It.Is<UserDto>(x =>
                x.Email == uDto.Email &&
                x.FullName == uDto.FullName &&
                x.RoleId == uDto.RoleId)))
            .ReturnsAsync(ServiceResult<UserDto>.Success(uDto));

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.FullName, Is.EqualTo("Chris Evans"));
        Assert.That(await _dbContext.Employees.CountAsync(), Is.EqualTo(1));
    }

    // -------------------- UPDATE --------------------

    [Test]
    public async Task UpdateEmployeeAsync_ReturnsUpdatedEmployeeDto()
    {
        var employee = new Employee
        {
            EmployeeNumber = "Emp004",
            FirstName = "Old",
            LastName = "Name",
            Email = "update@test.com",
            PhoneNumber = "5555555555",
            Position = "Tester",
            Salary = 40000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-29)
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(employee).State = EntityState.Detached;

        var dto = new EmployeeDto
        {
            Id = employee.Id,
            FirstName = "Updated",
            LastName = "User",
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            DepartmentId = 1,
            Position = "Manager",
            Salary = 90000
        };

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.FullName, Is.EqualTo("Updated User"));
    }

    // -------------------- DELETE --------------------

    [Test]
    public async Task DeleteEmployeeAsync_WhenDeleted_ReturnsSuccess()
    {
        var employee = new Employee
        {
            EmployeeNumber = "Emp005",
            FirstName = "Delete",
            LastName = "Me",
            Email = "delete@test.com",
            PhoneNumber = "6666666666",
            Position = "Support",
            Salary = 30000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-27)
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.DeleteEmployeeAsync(employee.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.True);
    }

    [Test]
    public async Task DeleteEmployeeAsync_WhenNotFound_ReturnsFail()
    {
        var result = await _employeeService.DeleteEmployeeAsync(999);

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public async Task EmployeeExist_when_found()
    {
        var employee = new Employee
        {
            EmployeeNumber = "Emp005",
            FirstName = "employee",
            LastName = "Me",
            Email = "exist@test.com",
            PhoneNumber = "6666666666",
            Position = "Support Manager",
            Salary = 30000,
            DepartmentId = 1,
            HireDate = DateTime.Now.AddYears(-5),
            DateOfBirth = DateTime.Now.AddYears(-27)
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var employee2 = new Employee
        {
            EmployeeNumber = "Emp006",
            FirstName = "Employee",
            LastName = "Me",
            Email = "exist@test.com",
            PhoneNumber = "6666666666",
            Position = "Support",
            Salary = 3000000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-27)
        };

        var result = await _employeeService.EmployeeExistsAsync(employee2.Email, employee2.PhoneNumber);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task EmployeeExist_when_Not_Phone_found()
    {
        var employee = new Employee
        {
            EmployeeNumber = "Emp005",
            FirstName = "employee",
            LastName = "Me",
            Email = "exist@test.com",
            PhoneNumber = "6666666666",
            Position = "Support Manager",
            Salary = 30000,
            DepartmentId = 1,
            HireDate = DateTime.Now.AddYears(-5),
            DateOfBirth = DateTime.Now.AddYears(-27)
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var employee2 = new Employee
        {
            EmployeeNumber = "Emp006",
            FirstName = "Employee",
            LastName = "Me",
            Email = "exist2@test.com",
            PhoneNumber = "6666446666",
            Position = "Support",
            Salary = 3000000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-27)
        };

        var result = await _employeeService.EmployeeExistsAsync(employee2.Email, employee2.PhoneNumber);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Employee does not exist"));
    }
}
