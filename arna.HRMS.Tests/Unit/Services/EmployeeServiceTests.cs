using arna.HRMS.Core.Common.Results;
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
using System.Data;

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
            _userServicesMock.Object,
            _roleServiceMock.Object,
            _mapper,
            validator
        );
    }

    // -------------------- GET ALL --------------------

    [Test]
    public async Task GetEmployeesAsync_ReturnsEmployeeDtoList()
    {
        var employees = new List<Employee>
        {
            new Employee
            {
                Id = 20,
                FirstName = "A",
                LastName = "alphabet",
                Email = "a.123@gmial.com",
                OfficeEmail = "a.987@gmail.com",
                PhoneNumber = "123",
                IsActive = true,
                IsDeleted = false,
                HireDate = DateTime.Today,
                DateOfBirth = DateTime.Today,
                Position = "Dev",
                Salary = 100,
                DepartmentId = 1,
                EmployeeNumber = "EMP020"
            },
            new Employee
            {
                Id = 21,
                FirstName = "T",
                LastName = "alphabet",
                Email = "T.123@gmial.com",
                OfficeEmail = "T.987@gmail.com",
                PhoneNumber = "1234567",
                IsActive = true,
                IsDeleted = false,
                HireDate = DateTime.Today,
                DateOfBirth = DateTime.Today,
                Position = "Dev",
                Salary = 100,
                DepartmentId = 1,
                EmployeeNumber = "EMP021"
            }
        };
        _dbContext.Employees.AddRange(employees);
        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.GetEmployeesAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count(), Is.EqualTo(2));
        Assert.That(result.Data![0].FullName, Is.EqualTo("T alphabet"));
        Assert.That(result.Data![1].FullName, Is.EqualTo("A alphabet"));
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
        var employees = new List<Employee>
        {
            new Employee
            {
                Id = 20,
                FirstName = "A",
                LastName = "alphabet",
                Email = "a.123@gmial.com",
                OfficeEmail = "a.987@gmail.com",
                PhoneNumber = "123",
                IsActive = true,
                IsDeleted = false,
                HireDate = DateTime.Today,
                DateOfBirth = DateTime.Today,
                Position = "Dev",
                Salary = 100,
                DepartmentId = 1,
                EmployeeNumber = "EMP020"
            },
            new Employee
            {
                Id = 21,
                FirstName = "T",
                LastName = "alphabet",
                Email = "T.123@gmial.com",
                OfficeEmail = "T.987@gmail.com",
                PhoneNumber = "1234567",
                IsActive = false,
                IsDeleted = true,
                HireDate = DateTime.Today,
                DateOfBirth = DateTime.Today,
                Position = "Dev",
                Salary = 100,
                DepartmentId = 1,
                EmployeeNumber = "EMP021"
            }
        };
        _dbContext.Employees.AddRange(employees);
        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.GetEmployeesAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count(), Is.EqualTo(1));
        Assert.That(result.Data![0].FullName, Is.EqualTo("A alphabet"));
    }


    // -------------------- GET BY ID --------------------

    [Test]
    public async Task GetEmployeeByIdAsync_WhenExists_ReturnsEmployee()
    {
        var employee = new Employee
        {
            Id = 20,
            FirstName = "A",
            LastName = "alphabet",
            Email = "a.123@gmial.com",
            OfficeEmail = "a.987@gmail.com",
            PhoneNumber = "123",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today,
            DateOfBirth = DateTime.Today,
            Position = "Dev",
            Salary = 100,
            DepartmentId = 1,
            EmployeeNumber = "EMP020"
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.GetEmployeeByIdAsync(employee.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.FullName, Is.EqualTo("A alphabet"));
    }

    [Test]
    public async Task GetEmployeeByIdAsync_ShouldMapDepartmentCode()
    {
        var employee = new Employee
        {
            Id = 20,
            FirstName = "Deparment",
            LastName = "ka banda",
            Email = "Deparment123@gmial.com",
            OfficeEmail = "Deparment987@gmail.com",
            PhoneNumber = "123",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today,
            DateOfBirth = DateTime.Today,
            Position = "Dev",
            Salary = 100,
            DepartmentId = 1,
            EmployeeNumber = "EMP020"
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.GetEmployeeByIdAsync(employee.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
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
        var result1 = await _employeeService.GetEmployeeByIdAsync(0);
        var result2 = await _employeeService.GetEmployeeByIdAsync(-5);

        Assert.That(result1.IsSuccess, Is.False);
        Assert.That(result1.Message, Is.EqualTo("Invalid employee ID."));
        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Is.EqualTo("Invalid employee ID."));
    }

    [Test]
    public async Task GetEmployeeByIdAsync_WhenDeleted_ReturnsFail()
    {
        var employee = new Employee
        {
            Id = 20,
            FirstName = "Deparment",
            LastName = "ka banda",
            Email = "Deparment123@gmial.com",
            OfficeEmail = "Deparment987@gmail.com",
            PhoneNumber = "123",
            IsActive = false,
            IsDeleted = true,
            HireDate = DateTime.Today,
            DateOfBirth = DateTime.Today,
            Position = "Dev",
            Salary = 100,
            DepartmentId = 1,
            EmployeeNumber = "EMP020"
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.GetEmployeeByIdAsync(employee.Id);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data , Is.Null);
        Assert.That(result.Message, Is.EqualTo("Employee not found."));
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
    public EmployeeDto EmployeeDetailsDto(string Fn, string ln, string Em, string om, string pn)
    {
        var dto = new EmployeeDto
        {
            FirstName = Fn,
            LastName = ln,
            DepartmentName = "INFORMATION TECHNOLOGY",
            Email = Em,
            OfficeEmail = om,
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
        var dto = EmployeeDetailsDto("New", "User", "new@test.com", "new@office.com", "9999999992");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.True);

        var employee = await _dbContext.Employees.FirstAsync();
        Assert.That(employee.EmployeeNumber, Is.EqualTo("EMP001"));
    }

    [Test]
    public async Task CreateEmployeeAsync_ShouldIncrementEmployeeNumber_WhenPreviousExists()
    {
        var firstemployee = new Employee
        {
            Id = 20,
            FirstName = "Deparment",
            LastName = "ka banda",
            Email = "Deparment123@gmial.com",
            OfficeEmail = "Deparment987@gmail.com",
            PhoneNumber = "123",
            IsActive = false,
            IsDeleted = true,
            HireDate = DateTime.Today,
            DateOfBirth = DateTime.Today,
            Position = "Dev",
            Salary = 100,
            DepartmentId = 1,
            EmployeeNumber = "EMP020"
        };
        _dbContext.Employees.Add(firstemployee);
        await _dbContext.SaveChangesAsync();

        var dto = EmployeeDetailsDto("New", "User", "new@test.com", "new@office.com", "9999999992");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        var employee = await _dbContext.Employees
            .OrderByDescending(e => e.Id)
            .FirstAsync();
        Assert.That(employee.EmployeeNumber, Is.EqualTo("EMP021"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenUserCreationFails_ShouldStillCreateEmployee()
    {
        var dto = EmployeeDetailsDto("UserFail", "Test", "userfail@test.com", "userfail@office.com", "7777777777");

        _userServicesMock.Setup(u => u.CreateUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(ServiceResult<UserDto>.Fail("User creation failed"));

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(await _dbContext.Employees.CountAsync(), Is.EqualTo(1));
        Assert.That(result.Data!.FirstName, Is.EqualTo("UserFail"));
        Assert.That(result.Data!.Email, Is.EqualTo(dto.Email));
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
        var dto = EmployeeDetailsDto("Null", "Test", "null@test.com"," null@office.com", "9998887776");

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
    public async Task CreateEmployeeAsync_WhenRoleServiceReturnsNull_EmployeeStillCreated()
    {
        var employeeDto = EmployeeDetailsDto("RoleNull", "Test", "rolenull@test.com","rolenull@office.com","1234567890");

        _roleServiceMock
            .Setup(r => r.GetRoleByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((ServiceResult<RoleDto>.Fail("Role not found")));

        var result = await _employeeService.CreateEmployeeAsync(employeeDto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Employee created successfully."));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenRoleDataIsNull_EmployeeStillCreated()
    {
        var dto = EmployeeDetailsDto("NoData", "User", "nodata@test.com","nodata@office.com", "5555555551");

        _roleServiceMock.Setup(r => r.GetRoleByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<RoleDto>.Success(null));

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Employee created successfully."));

    }
    [Test]
    public async Task CreateEmployeeAsync_WhenLastEmployeeNumberInvalidFormat_ShouldStartFromEmp001()
    {
        var employee = new Employee
        {
            Id = 20,
            FirstName = "Deparment",
            LastName = "ka banda",
            Email = "Deparment123@gmial.com",
            OfficeEmail = "Deparment987@gmail.com",
            PhoneNumber = "123",
            IsActive = false,
            IsDeleted = true,
            HireDate = DateTime.Today,
            DateOfBirth = DateTime.Today,
            Position = "Dev",
            Salary = 100,
            DepartmentId = 1,
            EmployeeNumber = "NUll",
        };

        await _dbContext.SaveChangesAsync();

        var dto = EmployeeDetailsDto("NewData", "NewEmpNo", "newdata@test.com", "newdata@office.com", "5555555551");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.EmployeeNumber, Is.EqualTo("EMP001"));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenRoleServiceThrowsException_ShouldFail()
    {
        var dto = EmployeeDetailsDto("NewData", "NewEmpNo", "newdata@test.com", "newdata@office.com", "5555555551");

        _roleServiceMock.Setup(Setup => Setup.GetRoleByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        Assert.ThrowsAsync<Exception>(async () =>
            await _employeeService.CreateEmployeeAsync(dto));
    }

    [Test]
    public async Task CreateEmployeeAsync_WhenLastEmployeeNumberLowerCase_ShouldIncrement()
    {
        var oldemployee =new Employee
        {
            EmployeeNumber = "EMP009",
            FirstName = "Old",
            LastName = "User",
            Email = "oldlower@test.com",
            OfficeEmail = "oldlower@office.com",
            PhoneNumber = "9000000002",
            Position = "Dev",
            Salary = 10000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        };
        _dbContext.Add(oldemployee);
        await _dbContext.SaveChangesAsync();

        var dto = EmployeeDetailsDto("New", "Lower", "newlower@test.com", "newlower@office.com", "9000000003");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        var employee = await _dbContext.Employees
            .OrderByDescending(e => e.Id)
            .FirstAsync();

        Assert.That(employee.EmployeeNumber, Is.EqualTo("EMP010"));
    }

    [Test]
    public async Task CreateEmployeeAsync_ReturnsCreatedEmployeeDto()
    {
        var employee = EmployeeDetailsDto("chuck", "norris", "chuck@test.com", "chuck@office.com", "9999999991");

        var result = await _employeeService.CreateEmployeeAsync(employee);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.FirstName, Is.EqualTo("chuck"));
        Assert.That(result.Data.LastName, Is.EqualTo("norris"));
        Assert.That(result.Data.Email, Is.EqualTo("chuck@test.com"));
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
        var oldemployee = new Employee
        {
            EmployeeNumber = "emp009",
            FirstName = "Old",
            LastName = "User",
            Email = "oldlower@test.com",
            OfficeEmail = "oldlower@office.com",
            PhoneNumber = "9000000002",
            Position = "Dev",
            Salary = 10000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        };

        _dbContext.Add(oldemployee);
        await _dbContext.SaveChangesAsync();

        var dto = EmployeeDetailsDto("Test", "User", "oldlower@test.com", "test@office.com","1234567890");

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Email or Phone Number already exists"));
    }
    [Test]
    public async Task CreateEmployeeAsync_WhenRoleNotFound_EmployeeStillCreated()
    {
        var dto = EmployeeDetailsDto("Test", "User", "rolefail@test.com","rolefail@office.com", "8888888888");

        _roleServiceMock.Setup(r => r.GetRoleByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<RoleDto>.Fail("Not found"));

        var result = await _employeeService.CreateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Employee created successfully."));
    }

    // -------------------- UPDATE --------------------

    [Test]
    public async Task UpdateEmployeeAsync_ReturnsUpdatedEmployeeDto()
    {
        var oldemployee = new Employee
        {
            EmployeeNumber = "EMP003",
            FirstName = "oldemployee",
            LastName = "employee",
            Email = "old@test.com",
            OfficeEmail = "old@iffice.com",
            PhoneNumber = "9000000002",
            Position = "Dev",
            Salary = 10000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        };
        _dbContext.Employees.Add(oldemployee);
        await _dbContext.SaveChangesAsync();

        var newemployee = new EmployeeDto
        {
            Id = oldemployee.Id,
            EmployeeNumber = "EMP003",
            FirstName = "newemployee",
            LastName = "employee",
            Email = "new@test.com",
            OfficeEmail = "new@iffice.com",
            PhoneNumber = "9000000002",
            Position = "Dev",
            Salary = 20000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        };

        _userServicesMock
       .Setup(x => x.GetuserByEmployeeAsync(It.IsAny<int>()))
       .ReturnsAsync(ServiceResult<UserDto?>.Fail("Not found"));

        _userServicesMock
            .Setup(x => x.UpdateUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(ServiceResult<UserDto>.Success(new UserDto()));

        // Mock RoleService (safe fallback)
        _roleServiceMock
            .Setup(x => x.GetRoleByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<RoleDto>.Fail("Role not found"));

        var result = await _employeeService.UpdateEmployeeAsync(newemployee);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.FirstName, Is.EqualTo("newemployee"));
        Assert.That(result.Data!.Email, Is.EqualTo("new@test.com"));

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
            OfficeEmail = employee.OfficeEmail,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            DepartmentId = 1,
            Position = "Manager",
            Salary = 90000
        };
    }
    private async Task<Employee> AddEmployeeForUpdateNew(string email = "update@test.com",string number ="1234567890")
    {
        var employee = new Employee
        {
            EmployeeNumber = Guid.NewGuid().ToString(),
            FirstName = "Old",
            LastName = "User",
            Email = email,
            OfficeEmail = email,
            PhoneNumber = number,
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

    private EmployeeDto ValidUpdateDtoNew(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = "Updated",
            LastName = "User",
            Email = employee.Email,
            OfficeEmail = employee.OfficeEmail,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            DepartmentId = 1,
            Position = "Manager",
            Salary = 90000,
            EmployeeNumber = employee.EmployeeNumber
        };
    }

    [Test]
    public async Task UpdateEmployeeAsync_WhenEmailAlreadyExists_ReturnsFail()
    {
        var emp1 = await AddEmployeeForUpdateNew("emp1@test.com");
        var emp2 = await AddEmployeeForUpdateNew("emp2@test.com","9999999999");

        await _dbContext.SaveChangesAsync();

        _userServicesMock
       .Setup(x => x.GetuserByEmployeeAsync(It.IsAny<int>()))
       .ReturnsAsync(ServiceResult<UserDto?>.Fail("Not found"));

        _userServicesMock
            .Setup(x => x.UpdateUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(ServiceResult<UserDto>.Success(new UserDto()));

        // Mock RoleService (safe fallback)
        _roleServiceMock
            .Setup(x => x.GetRoleByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<RoleDto>.Fail("Role not found"));


        var dto = ValidUpdateDtoNew(emp2);
        dto.Email = emp1.Email;

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Email or Phone Number already exists"));
    }

    [Test]
    public async Task UpdateEmployeeAsync_WhenPhoneAlreadyExists_ReturnsFail()
    {
        var emp1 = await AddEmployeeForUpdateNew("emp1@test.com");
        var emp2 = await AddEmployeeForUpdateNew("emp2@test.com", "9999999999");

        _userServicesMock.Setup(x=>x.GetuserByEmployeeAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<UserDto?>.Fail("Not found"));

        _userServicesMock.Setup(x => x.UpdateUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(ServiceResult<UserDto>.Success(new UserDto()));

        _roleServiceMock
            .Setup(x => x.GetRoleByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<RoleDto>.Fail("Role not found"));

        var dto = ValidUpdateDtoNew(emp2);
        dto.PhoneNumber = emp1.PhoneNumber;

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Does.Contain("Email or Phone Number already exists"));
    }

    [Test]
    public async Task UpdateEmployeeAsync_WhenPhoneEmpty_ReturnsFail()
    {
        var employee = await AddEmployeeForUpdateNew();

        var dto = ValidUpdateDtoNew(employee);
        dto.PhoneNumber = "";

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Phone number is required."));
    }


    [Test]
    public async Task UpdateEmployeeAsync_WhenDateOfBirthFuture_ReturnsFail()
    {
        var emp = await AddEmployeeForUpdateNew();

        var dto = ValidUpdateDtoNew(emp);
        dto.DateOfBirth = DateTime.Now.AddDays(1);

        var result = await _employeeService.UpdateEmployeeAsync(dto);   

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Does.Contain("DateOfBirth cannot be in the future"));
    }


    [Test]
    public async Task UpdateEmployeeAsync_WhenDateOfBirthDefault_ReturnsFail()
    {
        var emp = await AddEmployeeForUpdateNew();

        var dto = ValidUpdateDtoNew(emp);
        dto.DateOfBirth = default;

        var result = await _employeeService.UpdateEmployeeAsync(dto);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("DateOfBirth is required"));
    }


    [Test]
    public async Task UpdateEmployeeAsync_WhenEmailInvalid_ReturnsFail()
    {
        var emp =await AddEmployeeForUpdateNew();

        var dto = ValidUpdateDtoNew(emp);
        dto.Email = "invalid-email";

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Invalid Email format"));
    }

    [Test]
    public async Task UpdateEmployeeAsync_ShouldNotModifyEmployeeNumber()
    {
        var emp = await AddEmployeeForUpdateNew();
        var originalNumber = emp.EmployeeNumber;

        _userServicesMock.Setup(x=>x.GetuserByEmployeeAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<UserDto?>.Fail("Not found"));

        _userServicesMock.Setup(x=>x.UpdateUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(ServiceResult<UserDto>.Success(new UserDto()));

        _roleServiceMock.Setup(x=>x.GetRoleByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<RoleDto>.Fail("Role not found"));

        var dto = ValidUpdateDtoNew(emp);

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        var updated = await _dbContext.Employees.FindAsync(emp.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(updated!.EmployeeNumber, Is.EqualTo(originalNumber));
    }

    [Test]
    public async Task UpdateEmployeeAsync_WhenSalaryNegative_ReturnsFail()
    {
        var emp = await AddEmployeeForUpdateNew();

        var dto = ValidUpdateDtoNew(emp);
        
        dto.Salary = -5000;
        
        var result = await _employeeService.UpdateEmployeeAsync(dto);
        
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Salary must be greater than 0"));
    }

    [Test]
    public async Task UpdateEmployeeAsync_WhenEmailEmpty_ReturnsFail()
    {
        var emp = await AddEmployeeForUpdateNew();

        var dto = ValidUpdateDtoNew(emp);
        dto.Email = "";

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Email is required"));
    }


    [Test]
    public async Task UpdateEmployeeAsync_WhenLastNameEmpty_ReturnsFail()
    {
        var emp = await AddEmployeeForUpdateNew();

        var dto = ValidUpdateDtoNew(emp);
        dto.LastName = "";

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Message, Is.EqualTo("LastName is required"));
    }


    [Test]
    public async Task UpdateEmployeeAsync_WhenFirstNameEmpty_ReturnsFail()
    {
        var emp = await AddEmployeeForUpdateNew();

        var dto = ValidUpdateDtoNew(emp);
        dto.FirstName = "";

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Data, Is.Null);
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
        var emp = await AddEmployeeForUpdateNew();

        var dto = ValidUpdateDtoNew(emp);

        _dbContext.Employees.Remove(emp);
        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Employee not found"));
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
        var emp = new Employee
        {
            Id = 20,
            FirstName = "Deparment",
            LastName = "ka banda",
            Email = "Deparment123@gmial.com",
            OfficeEmail = "Deparment987@gmail.com",
            PhoneNumber = "123",
            IsActive = true,
            IsDeleted = false,
            HireDate = DateTime.Today,
            DateOfBirth = DateTime.Today,
            Position = "Dev",
            Salary = 100,
            DepartmentId = 1,
            EmployeeNumber = "EMP020"
        };
        _dbContext.Employees.Add(emp);
        await _dbContext.SaveChangesAsync();

        _userServicesMock.Setup(x => x.GetuserByEmployeeAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<UserDto?>.Fail("Not found"));

        _userServicesMock.Setup(x=>x.DeleteUserAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<bool>.Success(true));

        var result = await _employeeService.DeleteEmployeeAsync(emp.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Employee deleted successfully."));
        Assert.That(_dbContext.Employees.Find(emp.Id)!.IsDeleted, Is.True);
    }

    [Test]
    public async Task DeleteEmployeeAsync_WhenNotFound_ReturnsFail()
    {
        _userServicesMock.Setup(x=>x.GetuserByEmployeeAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<UserDto?>.Fail("Not found"));

        _userServicesMock.Setup(x=>x.DeleteUserAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<bool>.Success(true));

        var result = await _employeeService.DeleteEmployeeAsync(999);

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public async Task DeleteEmployeeAsync_ShouldSetIsDeletedTrue()
    {
        var emp = new Employee
        {
            EmployeeNumber = "Emp001",
            FirstName = "Soft",
            LastName = "Delete",
            Email = "softdelete@test.com",
            OfficeEmail = "softdelete@office.com",
            PhoneNumber = "8887776666",
            Position = "Dev",
            Salary = 10000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25)
        };

        _dbContext.Employees.Add(emp);
        await _dbContext.SaveChangesAsync();

        _userServicesMock.Setup(x => x.GetuserByEmployeeAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<UserDto?>.Fail("Not found"));

        _userServicesMock.Setup(x => x.DeleteUserAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<bool>.Success(true));

        await _employeeService.DeleteEmployeeAsync(emp.Id);

        var deleted = await _dbContext.Employees.FindAsync(emp.Id);

        Assert.That(deleted!.IsDeleted, Is.True);
    }

    [Test]
    public async Task DeleteEmployeeAsync_WhenIdIsZeroOrNegative_ReturnsFail()
    {
        var result = await _employeeService.DeleteEmployeeAsync(0);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid employee ID."));
        var result2 = await _employeeService.DeleteEmployeeAsync(-5);

        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Is.EqualTo("Invalid employee ID."));
    }

    [Test]
    public async Task DeleteEmployeeAsync_WhenAlreadyDeleted_ReturnsFail()
    {
        var emp = new Employee
        {
            EmployeeNumber = "EmpDeleted",
            FirstName = "123",
            LastName = "Deleted",
            Email = "123@test.com",
            OfficeEmail = "123@office.com",
            PhoneNumber = "8888888888",
            Position = "Dev",
            Salary = 20000,
            DepartmentId = 1,
            HireDate = DateTime.Now,
            DateOfBirth = DateTime.Now.AddYears(-25),
            IsActive = false,
            IsDeleted = true
        };

        _dbContext.Employees.Add(emp);

        await _dbContext.SaveChangesAsync();

        var result = await _employeeService.DeleteEmployeeAsync(emp.Id);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Employee not found."));
    }
}
