/*using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Repositories.Common.Interfaces;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Repositories;

[TestFixture]
public class AttendanceRepositoryTests
{
    private Mock<IBaseRepository<Attendance>> _baseRepositoryMock;
    private Mock<FestivalHolidayRepository> _festivalHolidayRepositoryMock;
    private AttendanceRepository _attendanceRepository;

    [SetUp]
    public void Setup()
    {
        _baseRepositoryMock = new Mock<IBaseRepository<Attendance>>();
        _festivalHolidayRepositoryMock = new Mock<FestivalHolidayRepository>(null); // Pass required constructor args if any
        _attendanceRepository = new AttendanceRepository(
            _baseRepositoryMock.Object,
            _festivalHolidayRepositoryMock.Object);
    }

    // --------------------------------------------------
    // GetAttendenceAsync
    // --------------------------------------------------
    [Test]
    public async Task GetAttendenceAsync_ReturnsAllAttendances()
    {
        // Arrange
        var attendances = new List<Attendance>
        {
            new Attendance { Id = 1, EmployeeId = 1 },
            new Attendance { Id = 2, EmployeeId = 2 }
        };

        _baseRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(attendances);

        // Act
        var result = await _attendanceRepository.GetAttendenceAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    // --------------------------------------------------
    // GetAttendanceByIdAsync - FOUND
    // --------------------------------------------------
    [Test]
    public async Task GetAttendanceByIdAsync_ReturnsAttendance_WhenFound()
    {
        // Arrange
        var attendance = new Attendance { Id = 1, EmployeeId = 5 };

        _baseRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(attendance);

        // Act
        var result = await _attendanceRepository.GetAttendanceByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
    }

    // --------------------------------------------------
    // GetAttendanceByIdAsync - NOT FOUND
    // --------------------------------------------------
    [Test]
    public async Task GetAttendanceByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _baseRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Attendance?)null);

        // Act
        var result = await _attendanceRepository.GetAttendanceByIdAsync(99);

        // Assert
        Assert.That(result, Is.Null);
    }

    // --------------------------------------------------
    // CreateAttendanceDtoAsync
    // --------------------------------------------------
    [Test]
    public async Task CreateAttendanceDtoAsync_CallsAddAsync()
    {
        // Arrange
        var attendance = new Attendance { Id = 3, EmployeeId = 7 };

        _baseRepositoryMock
            .Setup(r => r.AddAsync(attendance))
            .ReturnsAsync(attendance);

        // Act
        var result = await _attendanceRepository
            .CreateAttendanceAsync(attendance);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(3));

        _baseRepositoryMock.Verify(
            r => r.AddAsync(attendance),
            Times.Once);
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
        var empId = 10;

        var attendances = new List<Attendance>
        {
            new Attendance
            {
                Id = 1,
                EmployeeId = empId,
                Date = new DateTime(year, month, 5)
            },
            new Attendance
            {
                Id = 2,
                EmployeeId = empId,
                Date = new DateTime(year, month, 15)
            },
            // Should be filtered out
            new Attendance
            {
                Id = 3,
                EmployeeId = 99,
                Date = new DateTime(year, month, 5)
            }
        };

        _baseRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(attendances);

        // Act
        var result = await _attendanceRepository
            .GetAttendanceByMonthAsync(year, month, empId);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.All(a => a.EmployeeId == empId), Is.True);
    }
}
*/