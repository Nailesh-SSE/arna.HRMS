using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Repositories;

[TestFixture]
public class DepartmentRepositoryTests
{
    private ApplicationDbContext _dbContext = null!;
    private DepartmentRepository _departmentRepository = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var baseRepository = new BaseRepository<Department>(_dbContext);
        _departmentRepository = new DepartmentRepository(baseRepository);
    }

    [Test]
    public async Task GetDepartmentAsync_ShouldReturnOnlyActiveDepartments()
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
                Name = "InactiveDept",
                Code = "I1",
                Description = "Inactive",
                IsActive = false,
                IsDeleted = false
            },
            new Department
            {
                Name = "DeletedDept",
                Code = "D1",
                Description = "Deleted",
                IsActive = true,
                IsDeleted = true
            }
        );

        await _dbContext.SaveChangesAsync();

        var result = await _departmentRepository.GetDepartmentAsync();

        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("ActiveDept"));
    }

    [Test]
    public async Task GetDepartmentAsync_WhenNoDepartments_ReturnsEmpty()
    {
        var result = await _departmentRepository.GetDepartmentAsync();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetDepartmentAsync_ShouldOrderByIdDescending()
    {
        var dept1 = new Department
        {
            Name = "Dept1",
            Code = "D1",
            Description = "First",
            IsActive = true,
            IsDeleted = false
        };

        var dept2 = new Department
        {
            Name = "Dept2",
            Code = "D2",
            Description = "Second",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.AddRange(dept1, dept2);
        await _dbContext.SaveChangesAsync();

        var result = (await _departmentRepository.GetDepartmentAsync()).ToList();

        Assert.That(result[0].Id, Is.GreaterThan(result[1].Id));
    }

    [Test]
    public async Task GetDepartmentAsync_ShouldIncludeParentDepartment()
    {
        var parent = new Department
        {
            Name = "ParentDept",
            Code = "P1",
            Description = "Parent",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.Add(parent);
        await _dbContext.SaveChangesAsync();

        var child = new Department
        {
            Name = "ChildDept",
            Code = "C1",
            Description = "Child",
            ParentDepartmentId = parent.Id,
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.Add(child);
        await _dbContext.SaveChangesAsync();

        var result = (await _departmentRepository.GetDepartmentAsync()).ToList();

        var childFromDb = result.First(d => d.Name == "ChildDept");

        Assert.That(childFromDb.ParentDepartment, Is.Not.Null);
        Assert.That(childFromDb.ParentDepartment.Name, Is.EqualTo("ParentDept"));
    }

    [Test]
    public async Task GetDepartmentAsync_ShouldExcludeInactiveWithParent()
    {
        var parent = new Department
        {
            Name = "Parent2",
            Code = "P2",
            Description = "Parent",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.Add(parent);
        await _dbContext.SaveChangesAsync();

        var inactiveChild = new Department
        {
            Name = "InactiveChild",
            Code = "IC",
            Description = "Inactive",
            ParentDepartmentId = parent.Id,
            IsActive = false,
            IsDeleted = false
        };

        _dbContext.Departments.Add(inactiveChild);
        await _dbContext.SaveChangesAsync();

        var result = await _departmentRepository.GetDepartmentAsync();

        Assert.That(result.Any(d => d.Name == "InactiveChild"), Is.False);
    }

    [Test]
    public async Task GetDepartmentByIdAsync_WhenExists_ReturnsDepartment()
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

        var result = await _departmentRepository.GetDepartmentByIdAsync(department.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("IT"));
    }

    [Test]
    public async Task GetDepartmentByIdAsync_WhenNotFound_ReturnsNull()
    {
        var result = await _departmentRepository.GetDepartmentByIdAsync(999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetDepartmentByIdAsync_WhenInactive_ReturnsNull()
    {
        var department = new Department
        {
            Name = "HR",
            Code = "HR01",
            Description = "HR Dept",
            IsActive = false,
            IsDeleted = false
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        var result = await _departmentRepository.GetDepartmentByIdAsync(department.Id);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetDepartmentByIdAsync_WhenDeleted_ReturnsNull()
    {
        var department = new Department
        {
            Name = "Finance",
            Code = "FN01",
            Description = "Finance Dept",
            IsActive = true,
            IsDeleted = true
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        var result = await _departmentRepository.GetDepartmentByIdAsync(department.Id);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetDepartmentByIdAsync_WhenIdZeroAndNegative_ReturnsNull()
    {
        var result = await _departmentRepository.GetDepartmentByIdAsync(0);
        Assert.That(result, Is.Null);

        var result2 = await _departmentRepository.GetDepartmentByIdAsync(-22);
        Assert.That(result2, Is.Null);
    }

    [Test]
    public async Task GetDepartmentByIdAsync_WhenMultipleDepartmentsExist_ReturnsCorrectOne()
    {
        var dept1 = new Department
        {
            Name = "Dept1",
            Code = "D1",
            Description = "First",
            IsActive = true,
            IsDeleted = false
        };

        var dept2 = new Department
        {
            Name = "Dept2",
            Code = "D2",
            Description = "Second",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.AddRange(dept1, dept2);
        await _dbContext.SaveChangesAsync();

        var result = await _departmentRepository.GetDepartmentByIdAsync(dept2.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Dept2"));
    }

    [Test]
    public async Task CreateDepartmentAsync_ShouldAddDepartment()
    {
        var department = new Department
        {
            Name = "IT",
            Code = "IT01",
            Description = "Information Technology",
            IsActive = true,
            IsDeleted = false
        };

        var result = await _departmentRepository.CreateDepartmentAsync(department);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(await _dbContext.Departments.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task CreateDepartmentAsync_ShouldPersistCorrectData()
    {
        var department = new Department
        {
            Name = "HR",
            Code = "HR01",
            Description = "Human Resources",
            IsActive = true,
            IsDeleted = false
        };

        var result = await _departmentRepository.CreateDepartmentAsync(department);

        var saved = await _dbContext.Departments.FindAsync(result.Id);

        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Name, Is.EqualTo("HR"));
        Assert.That(saved.Code, Is.EqualTo("HR01"));
    }

    [Test]
    public async Task CreateDepartmentAsync_ShouldCreateWithParentDepartment()
    {
        var parent = new Department
        {
            Name = "ParentDept",
            Code = "P1",
            Description = "Parent",
            IsActive = true,
            IsDeleted = false
        };

        await _departmentRepository.CreateDepartmentAsync(parent);

        var child = new Department
        {
            Name = "ChildDept",
            Code = "C1",
            Description = "Child",
            ParentDepartmentId = parent.Id,
            IsActive = true,
            IsDeleted = false
        };

        var result = await _departmentRepository.CreateDepartmentAsync(child);

        var saved = await _dbContext.Departments
            .Include(d => d.ParentDepartment)
            .FirstOrDefaultAsync(d => d.Id == result.Id);

        Assert.That(saved!.ParentDepartmentId, Is.EqualTo(parent.Id));
    }

    [Test]
    public void CreateDepartmentAsync_WhenNull_ShouldThrowException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _departmentRepository.CreateDepartmentAsync(null!);
        });
    }

    [Test]
    public async Task UpdateDepartmentAsync_ShouldUpdateDepartment()
    {
        var department = new Department
        {
            Name = "OldName",
            Code = "OLD",
            Description = "Old Description",
            IsActive = true,
            IsDeleted = false
        };

        await _departmentRepository.CreateDepartmentAsync(department);

        department.Name = "NewName";
        department.Code = "NEW";

        var result = await _departmentRepository.UpdateDepartmentAsync(department);

        Assert.That(result.Name, Is.EqualTo("NewName"));

        var updated = await _dbContext.Departments.FindAsync(department.Id);
        Assert.That(updated!.Code, Is.EqualTo("NEW"));
    }

    [Test]
    public async Task UpdateDepartmentAsync_ShouldPersistAllFields()
    {
        var department = new Department
        {
            Name = "Finance",
            Code = "FN01",
            Description = "Finance Dept",
            IsActive = true,
            IsDeleted = false
        };

        await _departmentRepository.CreateDepartmentAsync(department);

        department.Description = "Updated Description";
        department.IsActive = false;

        await _departmentRepository.UpdateDepartmentAsync(department);

        var updated = await _dbContext.Departments.FindAsync(department.Id);

        Assert.That(updated!.Description, Is.EqualTo("Updated Description"));
        Assert.That(updated.IsActive, Is.False);
    }

    [Test]
    public async Task UpdateDepartmentAsync_ShouldUpdateParentDepartment()
    {
        var parent1 = new Department
        {
            Name = "Parent1",
            Code = "P1",
            Description = "Parent1",
            IsActive = true,
            IsDeleted = false
        };

        var parent2 = new Department
        {
            Name = "Parent2",
            Code = "P2",
            Description = "Parent2",
            IsActive = true,
            IsDeleted = false
        };

        await _departmentRepository.CreateDepartmentAsync(parent1);
        await _departmentRepository.CreateDepartmentAsync(parent2);

        var child = new Department
        {
            Name = "Child",
            Code = "C1",
            Description = "Child",
            ParentDepartmentId = parent1.Id,
            IsActive = true,
            IsDeleted = false
        };

        await _departmentRepository.CreateDepartmentAsync(child);

        child.ParentDepartmentId = parent2.Id;

        await _departmentRepository.UpdateDepartmentAsync(child);

        var updated = await _dbContext.Departments.FindAsync(child.Id);

        Assert.That(updated!.ParentDepartmentId, Is.EqualTo(parent2.Id));
    }

    [Test]
    public void UpdateDepartmentAsync_WhenDepartmentDoesNotExist_ShouldThrowConcurrencyException()
    {
        var department = new Department
        {
            Id = 999,
            Name = "Ghost",
            Code = "GH",
            Description = "Not in DB",
            IsActive = true,
            IsDeleted = false
        };

        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await _departmentRepository.UpdateDepartmentAsync(department);
        });
    }

    [Test]
    public void UpdateDepartmentAsync_WhenNull_ShouldThrowNullReferenceException()
    {
        Assert.ThrowsAsync<NullReferenceException>(async () =>
        {
            await _departmentRepository.UpdateDepartmentAsync(null!);
        });
    }

    [Test]
    public async Task DeleteDepartmentAsync_ShouldSoftDeleteDepartment()
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

        var result = await _departmentRepository.DeleteDepartmentAsync(department.Id);

        var deleted = await _dbContext.Departments.FindAsync(department.Id);

        Assert.That(result, Is.True);
        Assert.That(deleted!.IsActive, Is.False);
        Assert.That(deleted.IsDeleted, Is.True);
        Assert.That(deleted.UpdatedOn, Is.Not.Null);
    }

    [Test]
    public async Task DeleteDepartmentAsync_WhenNotFound_ReturnsFalse()
    {
        var result = await _departmentRepository.DeleteDepartmentAsync(999);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteDepartmentAsync_WhenAlreadyDeleted_ReturnsFalse()
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

        var result = await _departmentRepository.DeleteDepartmentAsync(department.Id);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteDepartmentAsync_WhenIdZero_ReturnsFalse()
    {
        var result = await _departmentRepository.DeleteDepartmentAsync(0);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteDepartmentAsync_ShouldSetUpdatedOnCorrectly()
    {
        var department = new Department
        {
            Name = "TimeDept",
            Code = "TD",
            Description = "Time Test",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        var beforeDelete = DateTime.Now;

        await _departmentRepository.DeleteDepartmentAsync(department.Id);

        var deleted = await _dbContext.Departments.FindAsync(department.Id);

        Assert.That(deleted!.UpdatedOn, Is.GreaterThanOrEqualTo(beforeDelete));
    }

    [Test]
    public async Task DepartmentExistsAsync_WhenExists_ReturnsTrue()
    {
        var department = new Department
        {
            Id=1,
            Name = "IT",
            Code = "IT01",
            Description = "IT Dept",
            IsActive = true,
            IsDeleted = false
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        var result = await _departmentRepository.DepartmentExistsAsync("IT", 0);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DepartmentExistsAsync_ShouldIgnoreCase()
    {
        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "Finance",
            Code = "FN",
            Description = "Finance Dept",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        var result = await _departmentRepository.DepartmentExistsAsync("finance", null);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DepartmentExistsAsync_ShouldIgnoreWhitespace()
    {
        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "HR",
            Code = "HR01",
            Description = "HR Dept",
            IsActive = true,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        var result = await _departmentRepository.DepartmentExistsAsync("   HR   ", 0);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DepartmentExistsAsync_WhenNotFound_ReturnsFalse()
    {
        var result = await _departmentRepository.DepartmentExistsAsync("Unknown", 1);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DepartmentExistsAsync_WhenInactive_ReturnsFalse()
    {
        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "InactiveDept",
            Code = "IN",
            Description = "Inactive",
            IsActive = false,
            IsDeleted = false
        });

        await _dbContext.SaveChangesAsync();

        var result = await _departmentRepository.DepartmentExistsAsync("InactiveDept", 1);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DepartmentExistsAsync_WhenDeleted_ReturnsFalse()
    {
        _dbContext.Departments.Add(new Department
        {
            Id = 1,
            Name = "DeletedDept",
            Code = "DD",
            Description = "Deleted",
            IsActive = true,
            IsDeleted = true
        });

        await _dbContext.SaveChangesAsync();

        var result = await _departmentRepository.DepartmentExistsAsync("DeletedDept", 1);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DepartmentExistsAsync_WhenNameIsNull_ReturnsFalse()
    {

        var result = await _departmentRepository.DepartmentExistsAsync(null!, 0);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DepartmentExistsAsync_WhenNameIsEmpty_ReturnsFalse()
    {
        var result = await _departmentRepository.DepartmentExistsAsync("", 0);

        Assert.That(result, Is.False);
    }

}
