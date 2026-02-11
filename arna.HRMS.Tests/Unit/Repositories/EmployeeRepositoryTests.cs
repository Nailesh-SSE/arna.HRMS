using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Repositories;

[TestFixture]
public class EmployeeRepositoryTests
{
    private ApplicationDbContext _dbContext = null!;
    private EmployeeRepository _employeeRepository = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var baseRepository = new BaseRepository<Employee>(_dbContext);
        _employeeRepository = new EmployeeRepository(baseRepository);
    }

    [Test]
    public async Task GetEmployeesAsync_ShouldReturnOnlyActiveAndNotDeletedEmployees()
    {
        // Given
        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "IT",
            Code = "CIT",
            Description = "Information Technology"
        });

        _dbContext.Employees.AddRange(
            new Employee
            {
                Id = 1,
                FirstName = "Inactive",
                LastName = "Doe1",
                IsActive = false,
                IsDeleted = false,
                DepartmentId = 1,
                Email="doe1@gmail.com",
                PhoneNumber="1234567891",
                Position= "Developer",
                EmployeeNumber = "Emp001"
            },
            new Employee
            {
                Id = 2,
                FirstName = "Deleted",
                LastName = "Doe2",
                IsActive = true,
                IsDeleted = true,
                DepartmentId = 1,
                Email = "doe2@gmail.com",
                PhoneNumber = "1234567845",
                Position = "Developer",
                EmployeeNumber = "Emp002"
            },
            new Employee
            {
                Id = 3,
                FirstName = "Valid",
                LastName = "Doe3",
                IsActive = true,
                IsDeleted = false,
                DepartmentId = 1,
                Email = "doe3@gmail.com",
                PhoneNumber = "1234561234",
                Position = "Developer",
                EmployeeNumber = "Emp003"
            }
        );

        await _dbContext.SaveChangesAsync();

        // When
        var result = await _employeeRepository.GetEmployeesAsync();

        // Then
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].FirstName, Is.EqualTo("Valid"));
    }

    [Test]
    public async Task GetEmployeesAsync_ShouldReturnEmployeesOrderedByIdDescending()
    {
        // Given
        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "IT",
            Code = "CIT",
            Description = "Information Technology"
        });

        _dbContext.Employees.AddRange(
            new Employee { Id = 1, FirstName = "John", LastName="Doe2",IsActive = true, IsDeleted = false, DepartmentId = 1,
                Email = "doe1@gmail.com",
                PhoneNumber = "1234567891",
                Position = "Developer",
                EmployeeNumber = "Emp001"
            },
            new Employee { Id = 3, FirstName = "Jane", LastName = "Doe3", IsActive = true, IsDeleted = false, DepartmentId = 1,
                Email = "doe2@gmail.com",
                PhoneNumber = "1234567811",
                Position = "Developer",
                EmployeeNumber = "Emp002"
            },
            new Employee { Id = 2, FirstName = "Alex", LastName = "Doe1", IsActive = true, IsDeleted = false, DepartmentId = 1,
                Email = "doe1@gmail.com",
                PhoneNumber = "12345678456",
                Position = "Developer",
                EmployeeNumber = "Emp003"
            }
        );

        await _dbContext.SaveChangesAsync();

        // When
        var result = await _employeeRepository.GetEmployeesAsync();

        // Then
        Assert.That(result[0].Id, Is.EqualTo(3));
        Assert.That(result[1].Id, Is.EqualTo(2));
        Assert.That(result[2].Id, Is.EqualTo(1));
    }


    [Test]
    public async Task GetEmployee_ShouldSuccess_WhenDataFound()
    {
        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "IT",
            Code = "CIT",
            Description = "Information Technology Department",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();
        _dbContext.Employees.AddRange(
            new Employee
            {
                Id = 1,
                EmployeeNumber = "EMP001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@gmail.com",
                PhoneNumber = "1234567895",
                DateOfBirth = DateTime.Now.AddYears(-20),
                HireDate = DateTime.Now.AddYears(-1),
                DepartmentId = 1,
                Position = "Developer",
                Salary = 50000,
                IsActive = true,
                IsDeleted = false
            },
            new Employee
            {
                Id = 2,
                EmployeeNumber = "EMP002",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "janagmail.com",
                PhoneNumber = "9876543210",
                DateOfBirth = DateTime.Now.AddYears(-25),
                HireDate = DateTime.Now.AddYears(-2),
                DepartmentId = 1,
                Position = "Designer",
                Salary = 60000,
                IsActive = true,
                IsDeleted = false
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = _employeeRepository.GetEmployeesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Result.Count, Is.EqualTo(2));
        Assert.That(result.Result[0].FirstName, Is.EqualTo("Jane"));
        Assert.That(result.Result[1].FirstName, Is.EqualTo("John"));
        Assert.That(result.Result[0].FullName, Is.EqualTo("Jane Smith"));
        Assert.That(result.Result[1].FullName, Is.EqualTo("John Doe"));
    }

    [Test]
    public async Task GetEmployee_ShouldReturnNull_WhenDataNotFound()
    {
        var result = await _employeeRepository.GetEmployeeByIdAsync(999);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateEmployee_ShouldSuccess()
    {
        var employee = new Employee
        {
            EmployeeNumber = "EMP003",
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice@gmail.com",
            PhoneNumber = "5555555555",
            DateOfBirth = DateTime.Now.AddYears(-30),
            HireDate = DateTime.Now.AddYears(-3),
            DepartmentId = 1,
            Position = "Manager",
            Salary = 80000
        };

        var result = await _employeeRepository.CreateEmployeeAsync(employee);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(result.FirstName, Is.EqualTo("Alice"));
        Assert.That(result.FullName, Is.EqualTo("Alice Johnson"));
        Assert.That(result.IsActive, Is.True);
        Assert.That(result.IsDeleted, Is.False);
        Assert.That(result.CreatedOn, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public async Task CreateEmployee_ShouldNullFail()
    {
        Employee? employee = null;
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _employeeRepository.CreateEmployeeAsync(employee!);
        });
    }

    [Test]
    public async Task DeleteEmployee_ShouldSuccess()
    {
        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "IT",
            Code = "CIT",
            Description = "Information Technology Department",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            EmployeeNumber = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@gmail.com",
            PhoneNumber = "1234567895",
            DateOfBirth = DateTime.Now.AddYears(-20),
            HireDate = DateTime.Now.AddYears(-1),
            DepartmentId = 1,
            Position = "Developer",
            Salary = 50000,
            IsActive = true,
            CreatedOn = DateTime.Now,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        var result = await _employeeRepository.DeleteEmployeeAsync(1);
        Assert.That(result, Is.True);
        Assert.That(_dbContext.Employees.First().IsActive, Is.False);
        Assert.That(_dbContext.Employees.First().IsDeleted, Is.True);
    }

    [Test]
    public async Task DeleteEmployee_shouldFail_whenIdIsZeroOrNegative()
    {
        
        var result= await _employeeRepository.DeleteEmployeeAsync(1);
        Assert.That(result, Is.False);
        
        var result2= await _employeeRepository.DeleteEmployeeAsync(0);
        Assert.That(result2, Is.False);

        var result3 = await _employeeRepository.DeleteEmployeeAsync(-1);
        Assert.That(result3, Is.False);
    }

    [Test]
    public async Task DeleteEmployee_shouldFail_WhenAlreadyDeleted()
    {
        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "IT",
            Code = "CIT",
            Description = "Information Technology Department",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            EmployeeNumber = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@gmail.com",
            PhoneNumber = "1234567895",
            DateOfBirth = DateTime.Now.AddYears(-20),
            HireDate = DateTime.Now.AddYears(-1),
            DepartmentId = 1,
            Position = "Developer",
            Salary = 50000,
            IsActive = false,
            CreatedOn = DateTime.Now,
            IsDeleted = true
        });
        await _dbContext.SaveChangesAsync();

        var result = await _employeeRepository.DeleteEmployeeAsync(1);
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdateEmployee_ShouldSuccess_WhenFound()
    {
        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "IT",
            Code = "CIT",
            Description = "Information Technology Department",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        _dbContext.Add(new Employee
        {
            Id = 1,
            EmployeeNumber = "Emp002",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "1231456789",
            Email = "john@gmail.com",
            DateOfBirth = DateTime.Now.AddYears(-20),
            HireDate = DateTime.Now,
            Position = "Developer",
            DepartmentId = 1,
            Salary = 5000000,
        });
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var Emp = new Employee
        {
            Id = 1,
            EmployeeNumber = "Emp002",
            FirstName = "John22",
            LastName = "Doe22",
            PhoneNumber = "1231456722",
            Email = "john2@gmail.com",
            DateOfBirth = DateTime.Now.AddYears(-22),
            HireDate = DateTime.Now,
            Position = "Designer",
            DepartmentId = 1,
            Salary = 5000000
        };

        var result = await _employeeRepository.UpdateEmployeeAsync(Emp);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.DateOfBirth, Is.EqualTo(Emp.DateOfBirth));
        Assert.That(result.HireDate, Is.EqualTo(Emp.HireDate));
        Assert.That(result.Id, Is.EqualTo(Emp.Id));
        Assert.That(result.EmployeeNumber, Is.EqualTo(Emp.EmployeeNumber));
    }

    [Test]
    public async Task UpdateEmployee_ShouldThrowNullReferenceException_WhenEntityIsNull()
    {
        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await _employeeRepository.UpdateEmployeeAsync(null!)
        );
    }

    [Test]
    public async Task GetEmployeeById_ShouldReturnEmployee_WhenEmployeeIsActiveAndNotDeleted()
    {
        // Given
        _dbContext.Departments.Add(new Department { Id = 1, Name = "IT", Code="CIT", Description="Information Technology" });

        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DepartmentId = 1,
            IsActive = true,
            IsDeleted = false,
            Email= "john@gmail.com",
            EmployeeNumber = "Emp001",
            PhoneNumber = "1234567892",
            Position = "Developer"
        });

        await _dbContext.SaveChangesAsync();

        // When
        var result = await _employeeRepository.GetEmployeeByIdAsync(1);

        // Then
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.FirstName, Is.EqualTo("John"));
        Assert.That(result.Department, Is.Not.Null);
    }

    [Test]
    public async Task GetEmployeeById_ShouldReturnNull_WhenEmployeeIsInactive()
    {
        _dbContext.Departments.Add(new Department {Id = 1, Name = "IT", Code = "CIT", Description = "Information Technology" });

        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DepartmentId = 1,
            IsActive = true,
            IsDeleted = true,
            Email = "john@gmail.com",
            EmployeeNumber = "Emp001",
            PhoneNumber = "1234567892",
            Position = "Developer"
        });

        await _dbContext.SaveChangesAsync();

        var result = await _employeeRepository.GetEmployeeByIdAsync(1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetEmployeeById_ShouldReturnNull_WhenEmployeeIsDeleted()
    {
        _dbContext.Departments.Add(new Department { Id = 1, Name = "IT", Code = "CIT", Description = "Information Technology" });

        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DepartmentId = 1,
            IsActive = true,
            IsDeleted = true,
            Email = "john@gmail.com",
            EmployeeNumber = "Emp001",
            PhoneNumber = "1234567892",
            Position = "Developer"
        });

        await _dbContext.SaveChangesAsync();

        var result = await _employeeRepository.GetEmployeeByIdAsync(1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetEmployeeById_ShouldReturnNull_WhenEmployeeDoesNotExist()
    {
        var result = await _employeeRepository.GetEmployeeByIdAsync(999);

        Assert.That(result, Is.Null);
    }
    private async Task SeedDepartmentAndEmployee()
    {
        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "IT",
            Code = "CIT",
            Description = "Information Technology",
            IsActive = true,
            IsDeleted = false
        });

        _dbContext.Employees.Add(new Employee
        {
            Id = 3,
            FirstName = "Valid",
            LastName = "Doe3",
            IsActive = true,
            IsDeleted = false,
            DepartmentId = 1,
            Email = "doe3@gmail.com",
            PhoneNumber = "1234561234",
            Position = "Developer",
            EmployeeNumber = "Emp003"
        });

        await _dbContext.SaveChangesAsync();
    }
    [Test]
    public async Task EmployeeExistsAsync_ShouldReturnFalse_WhenEmailAndPhoneAreEmpty()
    {
        var result = await _employeeRepository.EmployeeExistsAsync("", "");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task EmployeeExistsAsync_ShouldReturnTrue_WhenActiveEmployeeExistsByEmail()
    {
        await SeedDepartmentAndEmployee();

        var result = await _employeeRepository.EmployeeExistsAsync("doe3@gmail.com", null!);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task EmployeeExistsAsync_ShouldReturnTrue_WhenActiveEmployeeExistsByPhoneNumber()
    {
        await SeedDepartmentAndEmployee();

        var result = await _employeeRepository.EmployeeExistsAsync(null!, "1234561234");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task EmployeeExistsAsync_ShouldIgnoreCaseAndWhitespace_InEmail()
    {
        await SeedDepartmentAndEmployee();

        var result = await _employeeRepository.EmployeeExistsAsync("  DOE3@GMAIL.COM  ", null!);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task EmployeeExistsAsync_ShouldReturnFalse_WhenEmployeeIsInactive()
    {
        await SeedDepartmentAndEmployee();

        var employee = _dbContext.Employees.First();
        employee.IsActive = false;
        await _dbContext.SaveChangesAsync();

        var result = await _employeeRepository.EmployeeExistsAsync("doe3@gmail.com", null!);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task EmployeeExistsAsync_ShouldReturnFalse_WhenEmployeeIsDeleted()
    {
        await SeedDepartmentAndEmployee();

        var employee = _dbContext.Employees.First();
        employee.IsDeleted = true;
        await _dbContext.SaveChangesAsync();

        var result = await _employeeRepository.EmployeeExistsAsync(null!, "1234561234");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task EmployeeExistsAsync_ShouldReturnFalse_WhenEmployeeDoesNotExist()
    {
        await SeedDepartmentAndEmployee();

        var result = await _employeeRepository.EmployeeExistsAsync("unknown@gmail.com", "9999999999");

        Assert.That(result, Is.False);
    }

    private async Task SeedDepartment()
    {
        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "IT",
            Code = "CIT",
            Description = "Information Technology",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();
    }

    [Test]
    public async Task GetLastEmployeeNumberAsync_ShouldReturnNull_WhenNoEmployeesExist()
    {
        // When
        var result = await _employeeRepository.GetLastEmployeeNumberAsync();

        // Then
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetLastEmployeeNumberAsync_ShouldReturnEmployeeNumber_WhenOnlyOneExists()
    {
        await SeedDepartment();

        _dbContext.Employees.Add(new Employee
        {
            Id = 1,
            DepartmentId = 1,
            FirstName = "John",
            LastName = "Doe",
            EmployeeNumber = "Emp001",
            Email = "doe1@gmail.com",
            PhoneNumber = "1234567891",
            Position = "Developer",
        });

        await _dbContext.SaveChangesAsync();

        var result = await _employeeRepository.GetLastEmployeeNumberAsync();

        Assert.That(result, Is.EqualTo("Emp001"));
    }

    [Test]
    public async Task GetLastEmployeeNumberAsync_ShouldReturnHighestEmployeeNumber()
    {
        await SeedDepartment();

        _dbContext.Employees.AddRange(
            new Employee { Id = 1, DepartmentId = 1, EmployeeNumber = "Emp001",FirstName="John", LastName="Doe",
                Email = "doe1@gmail.com",
                PhoneNumber = "1234567891",
                Position = "Developer",
            },
            new Employee { Id = 2, DepartmentId = 1, EmployeeNumber = "Emp010",
                FirstName = "John2",
                LastName = "Doe2",
                Email = "doe2@gmail.com",
                PhoneNumber = "1234567894",
                Position = "Developer",
            },
            new Employee {Id = 3, DepartmentId = 1, EmployeeNumber = "Emp005", FirstName = "John3", LastName = "Doe3", Email = "doe3@gmail.com", PhoneNumber = "1234567444", Position = "Developer",}
        );

        await _dbContext.SaveChangesAsync();

        var result = await _employeeRepository.GetLastEmployeeNumberAsync();

        Assert.That(result, Is.EqualTo("Emp010"));
    }

    [Test]
    public async Task GetLastEmployeeNumberAsync_ShouldIgnoreNullEmployeeNumbers()
    {
        await SeedDepartment();

        _dbContext.Employees.AddRange(
            new Employee
            {
                Id = 1,
                FirstName = "Null",
                LastName = "Employee1",
                Email = "null1@gmail.com",
                PhoneNumber = "1111111111",
                Position = "Developer",
                EmployeeNumber = "Emp001",
                DepartmentId = 1,
                IsActive = true,
                IsDeleted = false
            },
            new Employee
            {
                Id = 2,
                FirstName = "Valid",
                LastName = "Employee2",
                Email = "valid@gmail.com",
                PhoneNumber = "2222222222",
                Position = "Developer",
                EmployeeNumber = "Emp002",
                DepartmentId = 1,
                IsActive = true,
                IsDeleted = false
            },
            new Employee
            {
                Id = 3,
                FirstName = "Null",
                LastName = "Employee3",
                Email = "null3@gmail.com",
                PhoneNumber = "3333333333",
                Position = "Developer",
                EmployeeNumber = "Emp003",
                DepartmentId = 1,
                IsActive = true,
                IsDeleted = false
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _employeeRepository.GetLastEmployeeNumberAsync();

        Assert.That(result, Is.EqualTo("Emp003"));
    }



}