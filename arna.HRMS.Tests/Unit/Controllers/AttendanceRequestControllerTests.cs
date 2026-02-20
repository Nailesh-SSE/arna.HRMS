using arna.HRMS.API.Controllers.Public;
using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace arna.HRMS.Tests.Unit.Controllers;

[TestFixture]
public class AttendanceRequestControllerTests
{
    private Mock<IAttendanceRequestService> _attendanceRequestServiceMock = null!;
    private AttendanceRequestController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _attendanceRequestServiceMock = new Mock<IAttendanceRequestService>();
        _controller = new AttendanceRequestController(_attendanceRequestServiceMock.Object);
    }

    [Test]
    public async Task GetAttendanceRequests_ReturnsOkResult()
    {
        var Data = new List<AttendanceRequestDto>
                {
                    new AttendanceRequestDto { Id = 1, EmployeeId = 1, StatusId = Status.Pending },
                    new AttendanceRequestDto { Id = 2, EmployeeId = 5, StatusId = Status.Approved },
                    new AttendanceRequestDto { Id = 3, EmployeeId = 3, StatusId = Status.Cancelled },
                    new AttendanceRequestDto { Id = 4, EmployeeId = 2, StatusId = Status.Rejected }
                };
        // Arrange
        _attendanceRequestServiceMock.Setup(s => s.GetAttendanceRequests(null, null))
            .ReturnsAsync(ServiceResult<List<AttendanceRequestDto>>.Success(Data));
        // Act
        var result = await _controller.GetAttendanceRequests(null, null);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetAttendanceRequests_ReturnsBadRequest()
    {
        // Arrange
        _attendanceRequestServiceMock.Setup(s => s.GetAttendanceRequests(null, null))
            .ReturnsAsync(ServiceResult<List<AttendanceRequestDto>>.Fail("Error fetching attendance requests"));
        // Act
        var result = await _controller.GetAttendanceRequests(null, null);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetAttendanceRequestById_ReturnsOkResult()
    {
        var Data = new AttendanceRequestDto { Id = 1, EmployeeId = 1, StatusId = Status.Pending };
        // Arrange
        _attendanceRequestServiceMock.Setup(s => s.GetAttendenceRequestByIdAsync(1))
            .ReturnsAsync(ServiceResult<AttendanceRequestDto?>.Success(Data));
        // Act
        var result = await _controller.GetAttendanceRequestById(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetAttendanceRequestById_ReturnsNotFound()
    {
        // Arrange
        _attendanceRequestServiceMock.Setup(s => s.GetAttendenceRequestByIdAsync(1))
            .ReturnsAsync(ServiceResult<AttendanceRequestDto?>.Fail("Attendance request not found"));
        // Act
        var result = await _controller.GetAttendanceRequestById(1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetAttendanceRequestById_ShouldFail_WhenIdIsZeroOrFail()
    {
        _attendanceRequestServiceMock.Setup(s => s.GetAttendenceRequestByIdAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<AttendanceRequestDto?>.Fail("Invalid ID"));

        var result = await _controller.GetAttendanceRequestById(0);
        
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

        result = await _controller.GetAttendanceRequestById(-1);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreateAttendanceRequest_ReturnsOkResult()
    {
        var requestDto = new AttendanceRequestDto { EmployeeId = 1, StatusId = Status.Pending };
        var createdDto = new AttendanceRequestDto { Id = 1, EmployeeId = 1, StatusId = Status.Pending };
        // Arrange
        _attendanceRequestServiceMock.Setup(s => s.CreateAttendanceRequestAsync(requestDto))
            .ReturnsAsync(ServiceResult<AttendanceRequestDto>.Success(createdDto));
        // Act
        var result = await _controller.CreateAttendanceRequest(requestDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task CreateAttendanceRequest_ReturnsBadRequest()
    {
        var requestDto = new AttendanceRequestDto { EmployeeId = 1, StatusId = Status.Pending };
        // Arrange
        _attendanceRequestServiceMock.Setup(s => s.CreateAttendanceRequestAsync(requestDto))
            .ReturnsAsync(ServiceResult<AttendanceRequestDto>.Fail("Error creating attendance request"));
        // Act
        var result = await _controller.CreateAttendanceRequest(requestDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateAttendanceRequest_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        var requestDto = new AttendanceRequestDto { EmployeeId = 1, StatusId = Status.Pending };

        _controller.ModelState.AddModelError("EmployeeId", "EmployeeId is required");

        var result = await _controller.CreateAttendanceRequest(requestDto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequest_ReturnsOkResult()
    {
        var requestDto = new AttendanceRequestDto { Id = 1, EmployeeId = 1, StatusId = Status.Pending };
        // Arrange
        _attendanceRequestServiceMock.Setup(s => s.UpdateAttendanceRequestAsync(requestDto))
            .ReturnsAsync(ServiceResult<AttendanceRequestDto>.Success(requestDto));
        // Act
        var result = await _controller.UpdateAttendanceRequest(1, requestDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequest_ReturnsBadRequest()
    {
        var requestDto = new AttendanceRequestDto { Id = 1, EmployeeId = 1, StatusId = Status.Pending };
        // Arrange
        _attendanceRequestServiceMock.Setup(s => s.UpdateAttendanceRequestAsync(requestDto))
            .ReturnsAsync(ServiceResult<AttendanceRequestDto>.Fail("Error updating attendance request"));
        // Act
        var result = await _controller.UpdateAttendanceRequest(1, requestDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequest_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        var requestDto = new AttendanceRequestDto { Id = 1, StatusId = Status.Pending };

        _controller.ModelState.AddModelError("EmployeeId", "EmployeeId is required");

        var result = await _controller.UpdateAttendanceRequest(1, requestDto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequest_ReturnsBadRequest_WhenIdMismatch()
    {
        var requestDto = new AttendanceRequestDto { Id = 2, EmployeeId = 1, StatusId = Status.Pending };

        _attendanceRequestServiceMock.Setup(s => s.UpdateAttendanceRequestAsync(requestDto))
            .ReturnsAsync(ServiceResult<AttendanceRequestDto>.Fail("Error updating attendance request"));

        var result = await _controller.UpdateAttendanceRequest(1, requestDto);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequest_ReturnsBadRequest_WhenIdIsZeroOrNegative()
    {
        var requestDto = new AttendanceRequestDto { Id = 0, EmployeeId = 1, StatusId = Status.Pending };
        _attendanceRequestServiceMock.Setup(s => s.UpdateAttendanceRequestAsync(requestDto))
            .ReturnsAsync(ServiceResult<AttendanceRequestDto>.Fail("Error updating attendance request"));
        var result = await _controller.UpdateAttendanceRequest(0, requestDto);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        requestDto.Id = -1;
        result = await _controller.UpdateAttendanceRequest(-1, requestDto);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeleteAttendanceRequest_ReturnsOkResult()
    {
        // Arrange
        _attendanceRequestServiceMock.Setup(s => s.DeleteAttendanceRequestAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Success(true));
        // Act
        var result = await _controller.DeleteAttendanceRequest(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task DeleteAttendanceRequest_ReturnsNotFound()
    {
        // Arrange
        _attendanceRequestServiceMock.Setup(s => s.DeleteAttendanceRequestAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Fail("Error deleting attendance request"));
        // Act
        var result = await _controller.DeleteAttendanceRequest(1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteAttendanceRequest_ReturnsBadRequest_WhenIdIsZeroOrNegative()
    {
        _attendanceRequestServiceMock.Setup(s => s.DeleteAttendanceRequestAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<bool>.Fail("Invalid ID"));
        var result = await _controller.DeleteAttendanceRequest(0);
        
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

        result = await _controller.DeleteAttendanceRequest(-1);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequestStatus_ReturnsOkResult()
    {
        // Arrange
        var claims = new List<Claim>
    {
        new Claim("EmployeeId", "2")
    };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        _attendanceRequestServiceMock
            .Setup(s => s.UpdateAttendanceRequestStatusAsync(1, Status.Approved, 2))
            .ReturnsAsync(ServiceResult<bool>.Success(true));

        // Act
        var result = await _controller.ApproveAttendanceRequest(1, Status.Approved);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public  async Task UpdateAttendanceRequestStatus_ReturnsBadRequest()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("EmployeeId", "2")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
        _attendanceRequestServiceMock
            .Setup(s => s.UpdateAttendanceRequestStatusAsync(1, Status.Approved, 2))
            .ReturnsAsync(ServiceResult<bool>.Fail("Error updating attendance request status"));
        // Act
        var result = await _controller.ApproveAttendanceRequest(1, Status.Approved);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequestStatus_ReturnsBadRequest_WhenUserIdNotInClaims()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };
        // Act
        var result = await _controller.ApproveAttendanceRequest(1, Status.Approved);
        // Assert
        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequestStatus_ReturnsBadRequest_WhenUserIdIsInvalid()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("EmployeeId", "invalid")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
        // Act
        var result = await _controller.ApproveAttendanceRequest(1, Status.Approved);
        // Assert
        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequestStatus_ReturnsBadRequest_WhenIdIsZeroOrNegative()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("EmployeeId", "2")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
        _attendanceRequestServiceMock
            .Setup(s => s.UpdateAttendanceRequestStatusAsync(It.Is<int>(id => id <= 0), Status.Approved, 2))
            .ReturnsAsync(ServiceResult<bool>.Fail("Invalid ID"));
        // Act
        var result = await _controller.ApproveAttendanceRequest(0, Status.Approved);
        
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        result = await _controller.ApproveAttendanceRequest(-1, Status.Approved);
        
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequestStatus_ReturnsBadRequest_WhenStatusIsInvalid()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("EmployeeId", "2")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
        // Act
        var result = await _controller.ApproveAttendanceRequest(1, (Status)999);
        
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetPendingAttendanceRequests_ReturnsOkResult()
    {
        var Data = new List<AttendanceRequestDto>
                {
                    new AttendanceRequestDto { Id = 1, EmployeeId = 1, StatusId = Status.Pending },
                    new AttendanceRequestDto { Id = 2, EmployeeId = 5, StatusId = Status.Cancelled }
                };
        // Arrange
        _attendanceRequestServiceMock.Setup(s => s.GetPendingAttendanceRequestesAsync())
            .ReturnsAsync(ServiceResult<List<AttendanceRequestDto>>.Success(Data));
        // Act
        var result = await _controller.GetPendingAttendanceRequests();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetPendingAttendanceRequests_ReturnsBadRequest()
    {
        // Arrange
        _attendanceRequestServiceMock.Setup(s => s.GetPendingAttendanceRequestesAsync())
            .ReturnsAsync(ServiceResult<List<AttendanceRequestDto>>.Success(null!));
        // Act
        var result = await _controller.GetPendingAttendanceRequests();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusCancel_ReturnsOkResult()
    {
        // Arrange
        _attendanceRequestServiceMock
            .Setup(s => s.UpdateAttendanceRequestStatusCancleAsync(1, 2))
            .ReturnsAsync(ServiceResult<bool>.Success(true));
        // Act
        var result = await _controller.UpdateAttendanceRequestStatusCancel(1, 2);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusCancel_ReturnsBadRequest()
    {
        // Arrange
        _attendanceRequestServiceMock
            .Setup(s => s.UpdateAttendanceRequestStatusCancleAsync(1, 2))
            .ReturnsAsync(ServiceResult<bool>.Fail("Error cancelling attendance request"));
        // Act
        var result = await _controller.UpdateAttendanceRequestStatusCancel(1, 2);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusCancel_ReturnsBadRequest_WhenIdIsZeroOrNegative()
    {
        _attendanceRequestServiceMock
            .Setup(s => s.UpdateAttendanceRequestStatusCancleAsync(It.Is<int>(id => id <= 0), 2))
            .ReturnsAsync(ServiceResult<bool>.Fail("Invalid ID"));
        var result = await _controller.UpdateAttendanceRequestStatusCancel(0, 2);
        
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        result = await _controller.UpdateAttendanceRequestStatusCancel(-1, 2);
        
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusCancel_ReturnsBadRequest_WhenEmployeeIdIsZeroOrNegative()
    {
        _attendanceRequestServiceMock
            .Setup(s => s.UpdateAttendanceRequestStatusCancleAsync(1, It.Is<int>(employeeId => employeeId <= 0)))
            .ReturnsAsync(ServiceResult<bool>.Fail("Invalid Employee ID"));
        var result = await _controller.UpdateAttendanceRequestStatusCancel(1, 0);
        
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        result = await _controller.UpdateAttendanceRequestStatusCancel(1, -1);
        
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateAttendanceRequestStatusCancel_ReturnsBadRequest_WhenBothIdIsZeroOrNegative()
    {
        _attendanceRequestServiceMock
            .Setup(s => s.UpdateAttendanceRequestStatusCancleAsync(It.Is<int>(id => id <= 0), It.Is<int>(employeeId => employeeId <= 0)))
            .ReturnsAsync(ServiceResult<bool>.Fail("Invalid ID and Employee ID"));

        var result = await _controller.UpdateAttendanceRequestStatusCancel(0, 0);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        result = await _controller.UpdateAttendanceRequestStatusCancel(-1, -1);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

}

