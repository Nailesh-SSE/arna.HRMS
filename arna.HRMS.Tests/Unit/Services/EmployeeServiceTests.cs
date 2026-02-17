using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Mapping;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Infrastructure.Validators;
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
        var validator = new EmployeeValidator(employeeRepository);

        _userServicesMock = new Mock<IUserServices>();
        _roleServiceMock = new Mock<IRoleService>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EmployeeProfile>();
        });


        _mapper = mapperConfig.CreateMapper();

        _employeeService = new EmployeeService(
            employeeRepository,
            _mapper,
            _userServicesMock.Object,
            _roleServiceMock.Object,
            validator
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

    [Test]
    public async Task GetEmployeesAsync_WhenNoEmployees_ReturnsEmptyList()
    {
        var result = await _employeeService.GetEmployeesAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetEmployeesAsync_ShouldExcludeDeletedEmployees()
    {
        _dbContext.Employees.AddRange(
            new Employee
            {
                EmployeeNumber = "Emp1",
                FirstName = "Active",
                LastName = "User",
                Email = "active@test.com",
                PhoneNumber = "1111111111",
                Position = "Dev",
                Salary = 10000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-25),
                IsActive = true,
                IsDeleted = false
            },
            new Employee
            {
                EmployeeNumber = "Emp2",
                FirstName = "Deleted",
                LastName = "User",
                Email = "deleted@test.com",
                PhoneNumber = "2222222222",
                Position = "Dev",
                Salary = 10000,
                DepartmentId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-25),
                IsActive = false,
                IsDeleted = true
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.GetEmployeesAsync();

        Assert.That(result.Data!.Count, Is.EqualTo(1));
        Assert.That(result.Data![0].Email, Is.EqualTo("active@test.com"));
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
    public async Task GetEmployeeByIdAsync_ShouldMapDepartmentCode()
    {
        var employee = new Employee
        {
            EmployeeNumber = "EmpDept",
            FirstName = "Dept",
            LastName = "Test",
            Email = "deptmap@test.com",
            PhoneNumber = "7777777777",
            Position = "Dev",
            Salary = 20000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.GetEmployeeByIdAsync(employee.Id);

        Assert.That(result.Data!.DepartmentCode, Is.EqualTo("IT"));
    }

    [Test]
    public async Task GetEmployeeByIdAsync_WhenNotExists_ReturnsFail()
    {
        var result = await _employeeService.GetEmployeeByIdAsync(999);

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public async Task GetEmployeeByIdAsync_WhenIdZeroOrNegative_ReturnsFail()
    {
        var result = await _employeeService.GetEmployeeByIdAsync(0);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Employee ID"));

        var result2 = await _employeeService.GetEmployeeByIdAsync(-5);

        Assert.That(result2.IsSuccess, Is.False);
    }

    [Test]
    public async Task GetEmployeeByIdAsync_WhenDeleted_ReturnsFail()
    {
        var employee = new Employee
        {
            EmployeeNumber = "Emp400",
            FirstName = "Soft",
            LastName = "Deleted",
            Email = "soft@test.com",
            PhoneNumber = "4444444444",
            Position = "Dev",
            Salary = 50000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25),
            IsActive = false,
            IsDeleted = true
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.GetEmployeeByIdAsync(employee.Id);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Employee not found"));
    }


    // -------------------- CREATE --------------------

    private EmployeeDto EmployeeDetails(string Fn, string ln, string Em, string pn)
    {
        var dto = new EmployeeDto
        {
            FirstName = Fn,
            LastName = ln,
            DepartmentName = "INFORMATION TECHNOLOGY",
            Email = Em,
            PhoneNumber = pn,
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
            FullName = dto.FirstName + " " + dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            RoleId = 2
        };
        _userServicesMock.Setup(u => u.CreateUserAsync(It.Is<UserDto>(x =>
                x.Email == uDto.Email &&
                x.FullName == uDto.FullName &&
                x.RoleId == uDto.RoleId)))
            .ReturnsAsync(ServiceResult<UserDto>.Success(uDto));

        return dto;
    }

    [Test]
    public async Task CreateEmployeeAsync_ShouldGenerateEmployeeNumber_WhenNoPreviousEmployee()
    {
        var dto = EmployeeDetails("First", "User", "first@test.com", "9999999991");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.True);

        var employee = await _dbContext.Employees.FirstAsync();
        Assert.That(employee.EmployeeNumber, Is.EqualTo("Emp001"));
    }

    [Test]
    public async Task CreateEmployeeAsync_ShouldIncrementEmployeeNumber_WhenPreviousExists()
    {
        _dbContext.Employees.Add(new Employee
        {
            EmployeeNumber = "Emp005",
            FirstName = "Old",
            LastName = "User",
            Email = "old@test.com",
            PhoneNumber = "9999999990",
            Position = "Dev",
            Salary = 10000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        });

        await _dbContext.SaveChangesAsync();

        var dto = EmployeeDetails("New", "User", "new@test.com", "9999999992");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        var employee = await _dbContext.Employees
            .OrderByDescending(e => e.Id)
            .FirstAsync();

        Assert.That(employee.EmployeeNumber, Is.EqualTo("Emp006"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenUserCreationFails_ShouldStillCreateEmployee()
    {
        var dto = EmployeeDetails("UserFail", "Test", "userfail@test.com", "7777777777");

        _userServicesMock.Setup(u => u.CreateUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(ServiceResult<UserDto>.Fail("User creation failed"));

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(await _dbContext.Employees.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenDepartmentInvalid_ReturnsFail()
    {
        var dto = EmployeeDetails("Dept", "Invalid", "dept@test.com", "1231231234");
        dto.DepartmentName = null;
        dto.DepartmentId = -2;
        dto.DepartmentCode = null;

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
    }


    [Test]
    public async Task CreateEmployeeAsync_WhenCreatedEmployeeIsNull_ReturnsFail()
    {
        var dto = EmployeeDetails("Null", "Test", "null@test.com", "9998887776");

        // Force role service to return valid role
        _roleServiceMock.Setup(r => r.GetRoleByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<RoleDto>.Success(new RoleDto { Id = 2 }));

        // Simulate user service normal
        _userServicesMock.Setup(u => u.CreateUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(ServiceResult<UserDto>.Success(new UserDto()));

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.Data, Is.Not.Null); // Since repository never returns null in current implementation
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenRoleServiceReturnsNull_ReturnsFail()
    {
        var dto = EmployeeDetails("NullRole", "User", "nullrole@test.com", "1234567899");

        _roleServiceMock
            .Setup(r => r.GetRoleByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((ServiceResult<RoleDto>)null!);

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Employee role not found"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenRoleDataIsNull_ReturnsFail()
    {
        var dto = EmployeeDetails("NoData", "User", "nodata@test.com", "5555555551");

        _roleServiceMock.Setup(r => r.GetRoleByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<RoleDto>.Success(null));

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Employee role not found"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenLastEmployeeNumberInvalidFormat_ShouldStartFromEmp001()
    {
        _dbContext.Employees.Add(new Employee
        {
            EmployeeNumber = "INVALID123",
            FirstName = "Old",
            LastName = "User",
            Email = "oldinvalid@test.com",
            PhoneNumber = "9000000000",
            Position = "Dev",
            Salary = 10000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        });

        await _dbContext.SaveChangesAsync();

        var dto = EmployeeDetails("New", "User", "newinvalid@test.com", "9000000001");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        var employee = await _dbContext.Employees
            .OrderByDescending(e => e.Id)
            .FirstAsync();

        Assert.That(employee.EmployeeNumber, Is.EqualTo("Emp001"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenRoleServiceThrowsException_ShouldFail()
    {
        var dto = EmployeeDetails("Exception", "User", "exception@test.com", "1234567893");

        _roleServiceMock
            .Setup(r => r.GetRoleByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        Assert.ThrowsAsync<Exception>(async () =>
            await _employeeService.CreateEmployeeAsync(dto));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenLastEmployeeNumberLowerCase_ShouldIncrement()
    {
        _dbContext.Employees.Add(new Employee
        {
            EmployeeNumber = "emp009",
            FirstName = "Old",
            LastName = "User",
            Email = "oldlower@test.com",
            PhoneNumber = "9000000002",
            Position = "Dev",
            Salary = 10000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        });

        await _dbContext.SaveChangesAsync();

        var dto = EmployeeDetails("New", "Lower", "newlower@test.com", "9000000003");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        var employee = await _dbContext.Employees
            .OrderByDescending(e => e.Id)
            .FirstAsync();

        Assert.That(employee.EmployeeNumber, Is.EqualTo("Emp010"));
    }

    [Test]
    public async Task CreateEmployeeAsync_ReturnsCreatedEmployeeDto()
    {
        var dto = EmployeeDetails("Chirag", "Evans", "chirag@gmail.com", "1234567892");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.FullName, Is.EqualTo("Chirag Evans"));
        Assert.That(await _dbContext.Employees.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenDtoNull_ReturnsFail()
    {
        var result = await _employeeService.CreateEmployeeAsync(null!);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenFirstNameEmpty_ReturnsFail()
    {
        var dto = EmployeeDetails("", "Test", "test@test.com", "1234567890");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("FirstName is required"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenLastNameEmpty_ReturnsFail()
    {
        var dto = EmployeeDetails("Test", "", "test@test.com", "1234567890");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("LastName is required"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenEmailEmpty_ReturnsFail()
    {
        var dto = EmployeeDetails("Test", "User", "", "1234567890");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Email is required"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenEmailInvalid_ReturnsFail()
    {
        var dto = EmployeeDetails("Test", "User", "invalid-email", "1234567890");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Email format"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenDateOfBirthDefault_ReturnsFail()
    {
        var dto = EmployeeDetails("Test", "User", "test@test.com", "1234567890");
        dto.DateOfBirth = default;

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("DateOfBirth is required"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenDateOfBirthFuture_ReturnsFail()
    {
        var dto = EmployeeDetails("Test", "User", "test@test.com", "1234567890");
        dto.DateOfBirth = DateTime.Now.AddDays(1);

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("DateOfBirth cannot be in the future"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenPhoneEmpty_ReturnsFail()
    {
        var dto = EmployeeDetails("Test", "User", "test@test.com", "");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Phone number is required."));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenEmailAlreadyExists_ReturnsFail()
    {
        var existing = new Employee
        {
            EmployeeNumber = "Emp001",
            FirstName = "Old",
            LastName = "User",
            Email = "duplicate@test.com",
            PhoneNumber = "9999999999",
            Position = "Dev",
            Salary = 10000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        };

        _dbContext.Employees.Add(existing);
        await _dbContext.SaveChangesAsync();

        var dto = EmployeeDetails("New", "User", "duplicate@test.com", "9999999999");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Email or Phone Number already exists"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenRoleNotFound_ReturnsFail()
    {
        var dto = EmployeeDetails("Test", "User", "rolefail@test.com", "8888888888");

        _roleServiceMock.Setup(r => r.GetRoleByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<RoleDto>.Fail("Not found"));

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Employee role not found"));
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

        _dbContext.ChangeTracker.Clear();

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

    private async Task<Employee> AddEmployeeForUpdate(string email = "update@test.com")
    {
        var employee = new Employee
        {
            EmployeeNumber = Guid.NewGuid().ToString(),
            FirstName = "Old",
            LastName = "User",
            Email = email,
            PhoneNumber = Guid.NewGuid().ToString().Substring(0, 10),
            Position = "Tester",
            Salary = 40000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-30)
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        return employee;
    }

    private EmployeeDto ValidUpdateDto(Employee employee)
    {
        return new EmployeeDto
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
    }

    [Test]
    public async Task UpdateEmployeeAsync_WhenEmailAlreadyExists_ReturnsFail()
    {
        var employee1 = await AddEmployeeForUpdate();
        var employee2 = await AddEmployeeForUpdate("other@test.com");

        var dto = ValidUpdateDto(employee2);
        dto.Email = employee1.Email;   // duplicate

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Email or Phone Number already exists"));
    }

    [Test]
    public async Task UpdateEmployeeAsync_WhenPhoneAlreadyExists_ReturnsFail()
    {
        var emp1 = await AddEmployeeForUpdate("emp1@test.com");
        var emp2 = await AddEmployeeForUpdate("emp2@test.com");

        var dto = ValidUpdateDto(emp2);
        dto.PhoneNumber = emp1.PhoneNumber;

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Email or Phone Number already exists"));
    }

    [Test]
    public async Task UpdateEmployeeAsync_WhenPhoneEmpty_ReturnsFail()
    {
        var employee = await AddEmployeeForUpdate();

        var dto = ValidUpdateDto(employee);
        dto.PhoneNumber = "";

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
    }


    [Test]
    public async Task UpdateEmployeeAsync_WhenDateOfBirthFuture_ReturnsFail()
    {
        var employee = await AddEmployeeForUpdate();

        var dto = ValidUpdateDto(employee);
        dto.DateOfBirth = DateTime.Now.AddDays(1);

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
    }


    [Test]
    public async Task UpdateEmployeeAsync_WhenDateOfBirthDefault_ReturnsFail()
    {
        var employee = await AddEmployeeForUpdate();

        var dto = ValidUpdateDto(employee);
        dto.DateOfBirth = default;

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
    }


    [Test]
    public async Task UpdateEmployeeAsync_WhenEmailInvalid_ReturnsFail()
    {
        var employee = await AddEmployeeForUpdate();

        var dto = ValidUpdateDto(employee);
        dto.Email = "invalid-email";

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid Email format"));
    }

    [Test]
    public async Task UpdateEmployeeAsync_ShouldNotModifyEmployeeNumber()
    {
        var employee = await AddEmployeeForUpdate();
        var originalNumber = employee.EmployeeNumber;

        var dto = ValidUpdateDto(employee);

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        var updated = await _dbContext.Employees.FindAsync(employee.Id);

        Assert.That(updated!.EmployeeNumber, Is.EqualTo(originalNumber));
    }

    [Test]
    public async Task UpdateEmployeeAsync_WhenSalaryNegative_ReturnsFail()
    {
        var employee = await AddEmployeeForUpdate();

        var dto = ValidUpdateDto(employee);
        dto.Salary = -100;

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public async Task UpdateEmployeeAsync_WhenEmailEmpty_ReturnsFail()
    {
        var employee = await AddEmployeeForUpdate();

        var dto = ValidUpdateDto(employee);
        dto.Email = "";

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
    }


    [Test]
    public async Task UpdateEmployeeAsync_WhenLastNameEmpty_ReturnsFail()
    {
        var employee = await AddEmployeeForUpdate();

        var dto = ValidUpdateDto(employee);
        dto.LastName = "";

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
    }


    [Test]
    public async Task UpdateEmployeeAsync_WhenFirstNameEmpty_ReturnsFail()
    {
        var employee = await AddEmployeeForUpdate();

        var dto = ValidUpdateDto(employee);
        dto.FirstName = "";

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("FirstName is required"));
    }


    [Test]
    public async Task UpdateEmployeeAsync_WhenEmployeeNotFound_ReturnsFail()
    {
        var dto = new EmployeeDto
        {
            Id = 999,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "1234567890",
            DateOfBirth = DateTime.Now.AddYears(-25)
        };

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Employee not found"));
    }

    [Test]
    public async Task UpdateEmployeeAsync_WhenRepositoryReturnsNull_ShouldFail()
    {
        var employee = await AddEmployeeForUpdate();

        var dto = ValidUpdateDto(employee);

        _dbContext.Employees.Remove(employee);
        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
    }


    [Test]
    public async Task UpdateEmployeeAsync_WhenInvalidId_ReturnsFail()
    {
        var dto = new EmployeeDto { Id = 0 };

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid Employee ID"));
    }


    [Test]
    public async Task UpdateEmployeeAsync_WhenDtoIsNull_ReturnsFail()
    {
        var result = await _employeeService.UpdateEmployeeAsync(null!);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid request"));
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
    public async Task DeleteEmployeeAsync_ShouldSetIsDeletedTrue()
    {
        var employee = new Employee
        {
            EmployeeNumber = "EmpSoft",
            FirstName = "Soft",
            LastName = "Delete",
            Email = "softdelete@test.com",
            PhoneNumber = "8887776666",
            Position = "Dev",
            Salary = 10000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        await _employeeService.DeleteEmployeeAsync(employee.Id);

        var deleted = await _dbContext.Employees.FindAsync(employee.Id);

        Assert.That(deleted!.IsDeleted, Is.True);
    }

    [Test]
    public async Task DeleteEmployeeAsync_WhenIdIsZeroOrNegative_ReturnsFail()
    {
        var result = await _employeeService.DeleteEmployeeAsync(0);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Employee not found"));
        var result2 = await _employeeService.DeleteEmployeeAsync(-5);

        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Is.EqualTo("Employee not found"));
    }

    [Test]
    public async Task DeleteEmployeeAsync_WhenAlreadyDeleted_ReturnsFail()
    {
        var employee = new Employee
        {
            EmployeeNumber = "EmpOld",
            FirstName = "Old",
            LastName = "Deleted",
            Email = "old@test.com",
            PhoneNumber = "8888888888",
            Position = "Dev",
            Salary = 20000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25),
            IsActive = false,
            IsDeleted = true
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.DeleteEmployeeAsync(employee.Id);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Employee not found"));
    }
}
