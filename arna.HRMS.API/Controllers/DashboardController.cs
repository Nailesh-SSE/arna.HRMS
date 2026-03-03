using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{

    private readonly IEmployeeService _employeeService;
    private readonly ILeaveService _leaveService;

    public DashboardController(IEmployeeService employeeService, ILeaveService leaveService)
    {
        _employeeService = employeeService;
        _leaveService = leaveService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardAsync([FromQuery] int? employeeId,[FromQuery] Status? status)
    {
        var employeesResult = await _employeeService.GetEmployeesAsync();
        var typesResult = await _leaveService.GetLeaveTypesAsync();

        ServiceResult<List<LeaveRequestDto>> requestsResult;

        if (status.HasValue || employeeId.HasValue)
        {
            requestsResult = await _leaveService.GetLeaveRequestsByFilterAsync(status, employeeId);
        }
        else
        {
            requestsResult = await _leaveService.GetLeaveRequestsAsync();
        }

        if (!employeesResult.IsSuccess ||
            !typesResult.IsSuccess ||
            !requestsResult.IsSuccess
        )
        {
            return BadRequest("Failed to load dashboard data.");
        }

        var dashboard = new DashboardDto
        {
            Employees = employeesResult.Data ?? new(),
            LeaveTypes = typesResult.Data ?? new(),
            Requests = requestsResult.Data ?? new()
        };

        return Ok(ServiceResult<DashboardDto>.Success(dashboard));
    }
}
