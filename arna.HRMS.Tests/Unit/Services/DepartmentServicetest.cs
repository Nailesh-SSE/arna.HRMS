using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Models.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;

namespace arna.HRMS.Tests.Unit.Services;

[TestFixture]
public class DepartmentServiceTests
{
    private Mock<IBaseRepository<Department>> _baseRepositoryMock = null!;
    private DepartmentRepository _departmentRepository = null!;
    private DepartmentService _departmentService = null!;
    private IMapper _mapper = null!;

    // =========================
    // SETUP
    // =========================
    [SetUp]
    public void Setup()
    {
        _baseRepositoryMock = new Mock<IBaseRepository<Department>>();

        _departmentRepository = new DepartmentRepository(
            _baseRepositoryMock.Object);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Department, DepartmentDto>();
            cfg.CreateMap<DepartmentDto, Department>();
        });

        _mapper = mapperConfig.CreateMapper();

        _departmentService = new DepartmentService(
            _departmentRepository,
            _mapper);
    }

    // =====================================================
    // GetDepartmentAsync
    // =====================================================
    [Test]
    public async Task GetDepartmentAsync_ReturnsAllDepartments()
    {
        // Arrange
        var departments = new List<Department>
        {
            new Department { Id = 1, Name = "HR", IsActive = true, IsDeleted = false },
            new Department { Id = 2, Name = "IT", IsActive = true, IsDeleted = false }
        };

        _baseRepositoryMock
            .Setup(r => r.Query())
            .Returns(departments.AsAsyncQueryable());

        // Act
        var result = await _departmentService.GetDepartmentAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Name, Is.EqualTo("HR"));
    }

    // =====================================================
    // GetDepartmentByIdAsync - FOUND
    // =====================================================
    [Test]
    public async Task GetDepartmentByIdAsync_ReturnsDto_WhenFound()
    {
        // Arrange
        var department = new Department
        {
            Id = 1,
            Name = "Finance"
        };

        _baseRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(department);

        // Act
        var result = await _departmentService.GetDepartmentByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Finance"));
    }

    // =====================================================
    // GetDepartmentByIdAsync - NOT FOUND
    // =====================================================
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

    // =====================================================
    // CreateDepartmentAsync
    // =====================================================
    [Test]
    public async Task CreateDepartmentAsync_ReturnsCreatedDepartmentDto()
    {
        // Arrange
        var dto = new DepartmentDto
        {
            Name = "Operations"
        };

        var savedDepartment = new Department
        {
            Id = 3,
            Name = "Operations"
        };

        _baseRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Department>()))
            .ReturnsAsync(savedDepartment);

        // Act
        var result = await _departmentService.CreateDepartmentAsync(dto);

        // Assert
        Assert.That(result.Id, Is.EqualTo(3));
        Assert.That(result.Name, Is.EqualTo("Operations"));
    }

    // =====================================================
    // UpdateDepartmentAsync
    // =====================================================
    [Test]
    public async Task UpdateDepartmentAsync_ReturnsUpdatedDepartmentDto()
    {
        // Arrange
        var dto = new DepartmentDto
        {
            Id = 4,
            Name = "Marketing"
        };

        var updatedDepartment = new Department
        {
            Id = 4,
            Name = "Marketing"
        };

        _baseRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Department>()))
            .ReturnsAsync(updatedDepartment);

        // Act
        var result = await _departmentService.UpdateDepartmentAsync(dto);

        // Assert
        Assert.That(result.Name, Is.EqualTo("Marketing"));
    }

    // =====================================================
    // DeleteDepartmentAsync
    // =====================================================
    [Test]
    public async Task DeleteDepartmentAsync_ReturnsTrue_WhenDeleted()
    {
        // Arrange
        var department = new Department
        {
            Id = 1,
            Name = "HR",
            IsActive = true,
            IsDeleted = false
        };

        _baseRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(department);

        _baseRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Department>()))
            .ReturnsAsync(department);

        // Act
        var result = await _departmentService.DeleteDepartmentAsync(1);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(department.IsActive, Is.False);
        Assert.That(department.IsDeleted, Is.True);
    }
}

#region EF Core 8 Async IQueryable Helpers (CORRECT)

// -----------------------------
// THIS FIXES CS0738 (EF Core 8)
// -----------------------------

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
        => new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new TestAsyncEnumerable<TElement>(expression);

    public object Execute(Expression expression)
        => _inner.Execute(expression)!;

    public TResult Execute<TResult>(Expression expression)
        => _inner.Execute<TResult>(expression)!;

    // 🔑 EF Core 8 expects TResult (NOT Task<TResult>)
    public TResult ExecuteAsync<TResult>(
        Expression expression,
        CancellationToken cancellationToken)
        => Execute<TResult>(expression);
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(
        CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider
        => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
        => new ValueTask<bool>(_inner.MoveNext());

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}

public static class AsyncQueryableExtensions
{
    public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source)
        => new TestAsyncEnumerable<T>(source);
}

#endregion
