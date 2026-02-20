using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace arna.HRMS.Tests.Unit.Controllers;

public class LeaveControllerTest
{
    private Mock<ILeaveService> _leaveServiceMock = null!;
    private LeaveController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _leaveServiceMock = new Mock<ILeaveService>();
        _controller = new LeaveController(_leaveServiceMock.Object);
    }

    [Test]
    public async Task GetLeaveTypes_WhenDataFound_ReturnsOkResult()
    {
        // Arrange
        var leaveTypes = new List<LeaveTypeDto>
        {
            new LeaveTypeDto { Id = 1, LeaveNameId = LeaveName.SickLeave , Description = "Used for illness", MaxPerYear = 10 },
            new LeaveTypeDto { Id = 2, LeaveNameId = LeaveName.CasualLeave , Description = "Used for any personal reson", MaxPerYear = 15 }
        };

        _leaveServiceMock
            .Setup(s => s.GetLeaveTypeAsync())
            .ReturnsAsync(ServiceResult<List<LeaveTypeDto>>.Success(leaveTypes));
        // Act
        var result = await _controller.GetLeaveTypes();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var apiResult = okResult!.Value as ServiceResult<List<LeaveTypeDto>>;
        Assert.That(apiResult!.Data!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetLeaveTypes_WhenNoDataFound_ReturnsNotFoundResult()
    {
        // Arrange
        _leaveServiceMock
            .Setup(s => s.GetLeaveTypeAsync())
            .ReturnsAsync(ServiceResult<List<LeaveTypeDto>>.Success(null!));
        // Act
        var result = await _controller.GetLeaveTypes();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Null);
    }

    [Test]
    public async Task GetLeaveTypeById_shouldReturnOkResult_WhenDataFound()
    {
        // Arrange
        var leaveType = new LeaveTypeDto { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Used for illness", MaxPerYear = 10 };
        _leaveServiceMock
            .Setup(s => s.GetLeaveTypeByIdAsync(1))
            .ReturnsAsync(ServiceResult<LeaveTypeDto>.Success(leaveType));
        // Act
        var result = await _controller.GetLeaveTypesById(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var apiResult = okResult!.Value as ServiceResult<LeaveTypeDto>;
        Assert.That(apiResult!.Data!.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetLeaveTypeById_shouldReturnNotFoundResult_WhenDataNotFound()
    {
        // Arrange
        _leaveServiceMock
            .Setup(s => s.GetLeaveTypeByIdAsync(1))
            .ReturnsAsync(ServiceResult<LeaveTypeDto>.Fail(null!));
        // Act
        var result = await _controller.GetLeaveTypesById(1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetLeaveTypeById_ShouldFail_WhenIdIsZeroOrNegative()
    {
        _leaveServiceMock
            .Setup(s => s.GetLeaveTypeByIdAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<LeaveTypeDto>.Fail("Invalid Id"));

        var result = await _controller.GetLeaveTypesById(0);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

        result = await _controller.GetLeaveTypesById(-1);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreateLeaveType_ShouldReturnOkResult_WhenModelIsValid()
    {
        // Arrange
        var leaveTypeDto = new LeaveTypeDto { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Used for illness", MaxPerYear = 10 };
        _leaveServiceMock
            .Setup(s => s.CreateLeaveTypeAsync(leaveTypeDto))
            .ReturnsAsync(ServiceResult<LeaveTypeDto>.Success(leaveTypeDto));
        // Act
        var result = await _controller.CreateLeaveType(leaveTypeDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var apiResult = okResult!.Value as ServiceResult<LeaveTypeDto>;
        Assert.That(apiResult!.Data!.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateLeaveType_ShouldReturnBadRequest_WhenModelIsInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("LeaveNameId", "LeaveNameId is required");
        var leaveTypeDto = new LeaveTypeDto { Id = 1, Description = "Used for illness", MaxPerYear = 10 };
        // Act
        var result = await _controller.CreateLeaveType(leaveTypeDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateLeaveType_ShouldFail_WhenCreateFail()
    {
        var dto = new LeaveTypeDto { Id = 1, LeaveNameId = LeaveName.SickLeave };
        _leaveServiceMock
            .Setup(s => s.CreateLeaveTypeAsync(dto))
            .ReturnsAsync(ServiceResult<LeaveTypeDto>.Fail("Create failed"));
        var result = await _controller.CreateLeaveType(dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateLeaveType_ShouldReturnOkResult_WhenModelIsValid()
    {
        // Arrange
        var leaveTypeDto = new LeaveTypeDto { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Used for illness", MaxPerYear = 10 };
        _leaveServiceMock
            .Setup(s => s.UpdateLeaveTypeAsync(leaveTypeDto))
            .ReturnsAsync(ServiceResult<LeaveTypeDto>.Success(leaveTypeDto));
        // Act
        var result = await _controller.UpdateLeaveType(1, leaveTypeDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var apiResult = okResult!.Value as ServiceResult<LeaveTypeDto>;
        Assert.That(apiResult!.Data!.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateLeaveType_ShouldFail_WhenUpdateFail()
    {
        var dto = new LeaveTypeDto { Id = 1, LeaveNameId = LeaveName.SickLeave };
        _leaveServiceMock
            .Setup(s => s.UpdateLeaveTypeAsync(dto))
            .ReturnsAsync(ServiceResult<LeaveTypeDto>.Fail("Update failed"));
        var result = await _controller.UpdateLeaveType(1, dto);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateLeaveType_ShouldFail_WhenModelIsInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("LeaveNameId", "LeaveNameId is required");
        var leaveTypeDto = new LeaveTypeDto { Id = 1, Description = "Used for illness", MaxPerYear = 10 };
        // Act
        var result = await _controller.UpdateLeaveType(1, leaveTypeDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateLeaveType_ShouldFail_WhenIdMismatch()
    {
        // Arrange
        var leaveTypeDto = new LeaveTypeDto { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Used for illness", MaxPerYear = 10 };
        // Act
        var result = await _controller.UpdateLeaveType(2, leaveTypeDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateLeaveType_ShouldFail_WhenIdIsZeroOrNegative()
    {
        var dto = new LeaveTypeDto { Id = 1, LeaveNameId = LeaveName.SickLeave };
        _leaveServiceMock
            .Setup(s => s.UpdateLeaveTypeAsync(dto))
            .ReturnsAsync(ServiceResult<LeaveTypeDto>.Fail("Invalid Id"));
        var result = await _controller.UpdateLeaveType(0, dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        result = await _controller.UpdateLeaveType(-1, dto);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeleteLeaveType_ShouldReturnOkResult_WhenDeleteSuccess()
    {
        new LeaveTypeDto { Id = 1, LeaveNameId = LeaveName.SickLeave, Description = "Used for illness", MaxPerYear = 10 };
        // Arrange
        _leaveServiceMock
            .Setup(s => s.DeleteLeaveTypeAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Success(true));
        // Act
        var result = await _controller.DeleteLeaveType(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkResult>());
    }

    [Test]
    public async Task DeleteLeaveType_ShouldReturnBadRequest_WhenDeleteFail()
    {
        // Arrange
        _leaveServiceMock
            .Setup(s => s.DeleteLeaveTypeAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Fail("Delete failed"));
        // Act
        var result = await _controller.DeleteLeaveType(1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteLeaveType_ShouldFail_WhenIdIsZeroOrNegative()
    {
        _leaveServiceMock
            .Setup(s => s.DeleteLeaveTypeAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<bool>.Fail("Invalid Id"));
        var result = await _controller.DeleteLeaveType(0);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        result = await _controller.DeleteLeaveType(-1);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetLeaveRequests_ShouldReturnOkResult_WhenDataFound()
    {
        // Arrange
        var leaveRequests = new List<LeaveRequestDto>
        {
            new LeaveRequestDto { Id = 1, EmployeeId = 1, LeaveTypeId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5), StatusId = Status.Pending },
            new LeaveRequestDto { Id = 2, EmployeeId = 2, LeaveTypeId = 2, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(3), StatusId = Status.Approved }
        };
        _leaveServiceMock
            .Setup(s => s.GetLeaveRequestAsync())
            .ReturnsAsync(ServiceResult<List<LeaveRequestDto>>.Success(leaveRequests));
        // Act
        var result = await _controller.GetLeaveRequests();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var apiResult = okResult!.Value as ServiceResult<List<LeaveRequestDto>>;
        Assert.That(apiResult!.Data!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetLeaveRequests_ShouldReturnOkResult_WhenNoDataFound()
    {
        // Arrange
        _leaveServiceMock
            .Setup(s => s.GetLeaveRequestAsync())
            .ReturnsAsync(ServiceResult<List<LeaveRequestDto>>.Success(null!));
        // Act
        var result = await _controller.GetLeaveRequests();
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var apiResult = okResult!.Value as ServiceResult<List<LeaveRequestDto>>;
        Assert.That(apiResult!.Data, Is.Null);
    }

    [Test]
    public async Task GetLeaveRequestById_ShouldReturnOkResult_WhenDataFound()
    {
        // Arrange
        var leaveRequest = new LeaveRequestDto { Id = 1, EmployeeId = 1, LeaveTypeId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5), StatusId = Status.Pending };
        _leaveServiceMock
            .Setup(s => s.GetLeaveRequestByIdAsync(1))
            .ReturnsAsync(ServiceResult<LeaveRequestDto>.Success(leaveRequest));
        // Act
        var result = await _controller.GetLeaveRequestById(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var apiResult = okResult!.Value as ServiceResult<LeaveRequestDto>;
        Assert.That(apiResult!.Data!.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetLeaveRequestById_ShouldReturnNotFoundResult_WhenDataNotFound()
    {
        // Arrange
        _leaveServiceMock
            .Setup(s => s.GetLeaveRequestByIdAsync(1))
            .ReturnsAsync(ServiceResult<LeaveRequestDto>.Fail(null!));
        // Act
        var result = await _controller.GetLeaveRequestById(1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetLeaveRequestById_ShouldFail_WhenIdIsZeroOrNegative()
    {
        _leaveServiceMock
            .Setup(s => s.GetLeaveRequestByIdAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<LeaveRequestDto>.Fail("Invalid Id"));
        var result = await _controller.GetLeaveRequestById(0);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        result = await _controller.GetLeaveRequestById(-1);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreateLeaveRequest_ShouldReturnOkResult_WhenModelIsValid()
    {
        // Arrange
        var leaveRequestDto = new LeaveRequestDto { Id = 1, EmployeeId = 1, LeaveTypeId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5), StatusId = Status.Pending };
        _leaveServiceMock
            .Setup(s => s.CreateLeaveRequestAsync(leaveRequestDto))
            .ReturnsAsync(ServiceResult<LeaveRequestDto>.Success(leaveRequestDto));
        // Act
        var result = await _controller.CreateLeaveRequest(leaveRequestDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var apiResult = okResult!.Value as ServiceResult<LeaveRequestDto>;
        Assert.That(apiResult!.Data!.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateLeaveRequest_ShouldReturnBadRequest_WhenModelIsInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("EmployeeId", "EmployeeId is required");
        var leaveRequestDto = new LeaveRequestDto { Id = 1, LeaveTypeId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5), StatusId = Status.Pending };
        // Act
        var result = await _controller.CreateLeaveRequest(leaveRequestDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateLeaveRequest_ShouldFail_WhenCreateFail()
    {
        var dto = new LeaveRequestDto { Id = 1, EmployeeId = 1, LeaveTypeId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5), StatusId = Status.Pending };
        _leaveServiceMock
            .Setup(s => s.CreateLeaveRequestAsync(dto))
            .ReturnsAsync(ServiceResult<LeaveRequestDto>.Fail("Create failed"));
        var result = await _controller.CreateLeaveRequest(dto);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateLeaveRequest_ShouldReturnOkResult_WhenModelIsValid()
    {
        // Arrange
        var leaveRequestDto = new LeaveRequestDto { Id = 1, EmployeeId = 1, LeaveTypeId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5), StatusId = Status.Pending };
        _leaveServiceMock
            .Setup(s => s.UpdateLeaveRequestAsync(leaveRequestDto))
            .ReturnsAsync(ServiceResult<LeaveRequestDto>.Success(leaveRequestDto));
        // Act
        var result = await _controller.UpdateLeaveRequest(1, leaveRequestDto);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var apiResult = okResult!.Value as ServiceResult<LeaveRequestDto>;
        Assert.That(apiResult!.Data!.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateLeaveRequest_ShouldFail_WhenUpdateFail()
    {
        var dto = new LeaveRequestDto { Id = 1, EmployeeId = 1, LeaveTypeId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5), StatusId = Status.Pending };
        _leaveServiceMock
            .Setup(s => s.UpdateLeaveRequestAsync(dto))
            .ReturnsAsync(ServiceResult<LeaveRequestDto>.Fail("Update failed"));
        var result = await _controller.UpdateLeaveRequest(1, dto);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateLeaveRequest_ShouldFail_WhenModelIsInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("EmployeeId", "EmployeeId is required");
        var leaveRequestDto = new LeaveRequestDto { Id = 1, LeaveTypeId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5), StatusId = Status.Pending };
        // Act
        var result = await _controller.UpdateLeaveRequest(1, leaveRequestDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateLeaveRequest_ShouldFail_WhenIdMismatch()
    {
        // Arrange
        var leaveRequestDto = new LeaveRequestDto { Id = 1, EmployeeId = 1, LeaveTypeId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5), StatusId = Status.Pending };
        // Act
        var result = await _controller.UpdateLeaveRequest(2, leaveRequestDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateLeaveRequest_ShouldFail_WhenStatusIsNotPending()
    {
        // Arrange
        var leaveRequestDto = new LeaveRequestDto { Id = 1, EmployeeId = 1, LeaveTypeId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5), StatusId = Status.Approved };
        // Act
        var result = await _controller.UpdateLeaveRequest(1, leaveRequestDto);
        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeleteLeaveRequest_ShouldReturnOkResult_WhenDeleteSuccess()
    {
        // Arrange
        _leaveServiceMock
            .Setup(s => s.DeleteLeaveRequestAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Success(true));
        // Act
        var result = await _controller.DeleteLeaveRequest(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkResult>());
    }

    [Test]
    public async Task DeleteLeaveRequest_ShouldReturnBadRequest_WhenDeleteFail()
    {
        // Arrange
        _leaveServiceMock
            .Setup(s => s.DeleteLeaveRequestAsync(1))
            .ReturnsAsync(ServiceResult<bool>.Fail("Delete failed"));
        // Act
        var result = await _controller.DeleteLeaveRequest(1);
        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteLeaveRequest_ShouldFail_WhenIdIsZeroOrNegative()
    {
        _leaveServiceMock
            .Setup(s => s.DeleteLeaveRequestAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<bool>.Fail("Invalid Id"));
        var result = await _controller.DeleteLeaveRequest(0);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        result = await _controller.DeleteLeaveRequest(-1);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetLeaveRequestsByEmployeeId_ShouldReturnOkResult_WhenDataFound()
    {
        // Arrange
        var leaveRequests = new List<LeaveRequestDto>
        {
            new LeaveRequestDto { Id = 1, EmployeeId = 1, LeaveTypeId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5), StatusId = Status.Pending },
            new LeaveRequestDto { Id = 2, EmployeeId = 1, LeaveTypeId = 2, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(3), StatusId = Status.Approved }
        };
        _leaveServiceMock
            .Setup(s => s.GetLeaveRequestByEmployeeIdAsync(1))
            .ReturnsAsync(ServiceResult<List<LeaveRequestDto>>.Success(leaveRequests));
        // Act
        var result = await _controller.GetEmployeeLeaveRequest(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var apiResult = okResult!.Value as ServiceResult<List<LeaveRequestDto>>;
        Assert.That(apiResult!.Data!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetLeaveRequestsByEmployeeId_ShouldReturnOkResult_WhenNoDataFound()
    {
        // Arrange
        _leaveServiceMock
            .Setup(s => s.GetLeaveRequestByEmployeeIdAsync(1))
            .ReturnsAsync(ServiceResult<List<LeaveRequestDto>>.Success(null!));
        // Act
        var result = await _controller.GetEmployeeLeaveRequest(1);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var apiResult = okResult!.Value as ServiceResult<List<LeaveRequestDto>>;
        Assert.That(apiResult!.Data, Is.Null);
    }

    [Test]
    public async Task GetLeaveRequestsByEmployeeId_ShouldFail_WhenIdIsZeroOrNegative()
    {
        _leaveServiceMock
            .Setup(s => s.GetLeaveRequestByEmployeeIdAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync(ServiceResult<List<LeaveRequestDto>>.Fail("Invalid Id"));
        var result = await _controller.GetEmployeeLeaveRequest(0);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        result = await _controller.GetEmployeeLeaveRequest(-1);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetLeaveRequestsByStatus_ShouldReturnOkResult_WhenDataFound()
    {
        // Arrange
        var leaveRequests = new List<LeaveRequestDto>
        {
            new LeaveRequestDto { Id = 1, EmployeeId = 1, LeaveTypeId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5), StatusId = Status.Pending },
            new LeaveRequestDto { Id = 2, EmployeeId = 2, LeaveTypeId = 2, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(3), StatusId = Status.Pending }
        };
        _leaveServiceMock
            .Setup(s => s.GetByFilterAsync(Status.Pending, null))
            .ReturnsAsync(ServiceResult<List<LeaveRequestDto>>.Success(leaveRequests));
        // Act
        var result = await _controller.GetLeaveRequestsByFilter(Status.Pending, null);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var apiResult = okResult!.Value as ServiceResult<List<LeaveRequestDto>>;
        Assert.That(apiResult!.Data!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetLeaveRequestsByStatus_ShouldReturnOkResult_WhenNoDataFound()
    {
        // Arrange
        _leaveServiceMock
            .Setup(s => s.GetByFilterAsync(Status.Pending, null))
            .ReturnsAsync(ServiceResult<List<LeaveRequestDto>>.Success(null!));
        // Act
        var result = await _controller.GetLeaveRequestsByFilter(Status.Pending, null);
        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var apiResult = okResult!.Value as ServiceResult<List<LeaveRequestDto>>;
        Assert.That(apiResult!.Data, Is.Null);
    }

    /*[Test]
    public async Task GetLea*/
}
