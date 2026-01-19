using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common.Interfaces;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Models.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;

namespace arna.HRMS.Tests.Unit.Services;

[TestFixture]
public class AttendanceServiceTests
{
    private Mock<IBaseRepository<Attendance>> _baseRepositoryMock = null!;
    private Mock<IEmployeeService> _employeeServiceMock = null!;
    private AttendanceRepository _attendanceRepository = null!;
    private AttendanceService _attendanceService = null!;
    private Mock<IFestivalHolidayService> _festivalHolidayService = null!; 
    private IMapper _mapper = null!;

    #region Setup

    [SetUp]
    public void Setup()
    {
        _baseRepositoryMock = new Mock<IBaseRepository<Attendance>>();
        _employeeServiceMock = new Mock<IEmployeeService>();
        _festivalHolidayService = new Mock<IFestivalHolidayService>();

        _attendanceRepository = new AttendanceRepository(
            _baseRepositoryMock.Object);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AttendanceProfile>();
        });

        _mapper = mapperConfig.CreateMapper();

        _attendanceService = new AttendanceService(
            _attendanceRepository,
            _mapper,
            _employeeServiceMock.Object,
            _festivalHolidayService.Object); 
    }

    #endregion

    // --------------------------------------------------
    // GetAttendanceAsync
    // --------------------------------------------------
    [Test]
    public async Task GetAttendanceAsync_ReturnsAllAttendances()
    {
        // Arrange
        var attendances = new List<Attendance>
        {
            new Attendance { Id = 1, EmployeeId = 1, Date = DateTime.Today },
            new Attendance { Id = 2, EmployeeId = 2, Date = DateTime.Today }
        };

        var asyncQueryable = new TestAsyncEnumerable<Attendance>(attendances);

        _baseRepositoryMock
            .Setup(r => r.Query())
            .Returns(asyncQueryable);

        // Act
        var result = await _attendanceService.GetAttendanceAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data?.Count, Is.EqualTo(2));
    }

    // --------------------------------------------------
    // GetAttendenceByIdAsync - FOUND
    // --------------------------------------------------
    [Test]
    public async Task GetAttendenceByIdAsync_ReturnsDto_WhenFound()
    {
        // Arrange
        var attendance = new Attendance
        {
            Id = 10,
            EmployeeId = 3,
            Date = DateTime.Today
        };

        _baseRepositoryMock
            .Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(attendance);

        // Act
        var result = await _attendanceService.GetAttendenceByIdAsync(10);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Data?.Id, Is.EqualTo(10));
    }

    // --------------------------------------------------
    // GetAttendenceByIdAsync - NOT FOUND
    // --------------------------------------------------
    [Test]
    public async Task GetAttendenceByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _baseRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Attendance?)null);

        // Act
        var result = await _attendanceService.GetAttendenceByIdAsync(99);

        // Assert
        Assert.That(result, Is.Null);
    }

    // --------------------------------------------------
    // CreateAttendanceAsync
    // --------------------------------------------------
    [Test]
    public async Task CreateAttendanceAsync_ReturnsCreatedAttendanceDto()
    {
        // Arrange
        var dto = new AttendanceDto
        {
            EmployeeId = 5,
            Date = DateTime.Today,
            WorkingHours = TimeSpan.FromHours(8),
            Notes = "Test"
        };

        _baseRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Attendance>()))
            .ReturnsAsync((Attendance entity) =>
            {
                entity.Id = 100;
                return entity;
            });

        // Act
        var result = await _attendanceService.CreateAttendanceAsync(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data?.Id, Is.EqualTo(100));
        Assert.That(result.Data?.EmployeeId, Is.EqualTo(5));
    }

    // --------------------------------------------------
    // GetAttendanceByMonthAsync
    // --------------------------------------------------
    [Test]
    public async Task GetAttendanceByMonthAsync_ReturnsFilteredAttendances()
    {
        // Arrange
        var year = 2025;
        var month = 1;
        var empId = 7;

        var attendances = new List<Attendance>
        {
            new Attendance
            {
                EmployeeId = empId,
                Date = new DateTime(year, month, 5),
                ClockIn = new DateTime(year, month, 5, 9, 0, 0),
                ClockOut = new DateTime(year, month, 5, 18, 0, 0)
            },
            new Attendance
            {
                EmployeeId = empId,
                Date = new DateTime(year, month, 15),
                ClockIn = new DateTime(year, month, 15, 9, 30, 0),
                ClockOut = new DateTime(year, month, 15, 17, 30, 0)
            },
            new Attendance
            {
                EmployeeId = 999,
                Date = new DateTime(year, month, 10)
            }
        };

        _baseRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(attendances);

        // Act
        var result = await _attendanceService
            .GetAttendanceByMonthAsync(year, month, empId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data?.Count, Is.EqualTo(2));
        Assert.That(result.Data?.All(x => x.EmployeeId == empId), Is.True);
    }

    #region EF Core Async Helpers (DO NOT TOUCH)

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
            => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
            => _inner.Execute<TResult>(expression);

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            => new TestAsyncEnumerable<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            => Execute<TResult>(expression);
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable) { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
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

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
            => new ValueTask<bool>(_inner.MoveNext());
    }

    #endregion
}
