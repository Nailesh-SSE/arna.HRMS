using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace arna.HRMS.API.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceRequestController : ControllerBase
{
    private readonly IAttendanceRequestService _attendanceRequestService;

    public AttendanceRequestController(IAttendanceRequestService attendanceRequestService)
    {
        _attendanceRequestService = attendanceRequestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAttendanceRequest()
    {
        var result = await _attendanceRequestService.GetAttendanceRequestAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAttendanceRequestById(int id)
    {
        var result = await _attendanceRequestService.GetAttendenceRequestByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(result.Message);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAttendanceRequest([FromBody] AttendanceRequestDto attendanceRequestDto)
    {
        var result = await _attendanceRequestService.CreateAttendanceRequestAsync(attendanceRequestDto);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] AttendanceRequestDto dto)
    {
        dto.Id = id;
        var result = await _attendanceRequestService.UpdateAttendanceRequestAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetAttendanceRequestsByEmployee(int employeeId)
    {
        var result = await _attendanceRequestService.GetAttendanceRequestAsync();
        var employeeRequests = result.Data?.Where(x => x.EmployeeId == employeeId).OrderByDescending(d => d.Id);
        return Ok(employeeRequests);
    }

    [HttpPost("cancel/{id:int}/{employeeId:int}")]
    public async Task<IActionResult> UpdateAttendanceRequestStatusCancel(int id, int employeeId)
    {
        var result = await _attendanceRequestService.UpdateAttendanceRequestStatusCancleAsync(id, employeeId);
        if (!result.IsSuccess)
            return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpGet("pending")]
    [Authorize(Roles = UserRoleGroups.AdminRoles)]
    public async Task<IActionResult> GetPendingAttendanceRequests()
    {
        var result = await _attendanceRequestService.GetAttendanceRequestAsync();
        var pending = result.Data?.Where(x => x.Status == Status.Pending).OrderByDescending(d => d.Id);
        return Ok(pending);
    }

    [HttpPost("status/{id:int}")]
    [Authorize(Roles = UserRoleGroups.AdminRoles)]
    public async Task<IActionResult> ApproveAttendanceRequest(int id, [FromQuery] Status status)
    {
        if (status != Status.Approved && status != Status.Rejected)
            return BadRequest("Invalid status");

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var approvedBy))
            return Unauthorized("Invalid user claim");

        var result = await _attendanceRequestService.UpdateAttendanceRequestStatusAsync(id, status, approvedBy);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok($"Leave {status} successfully");
    }
}
