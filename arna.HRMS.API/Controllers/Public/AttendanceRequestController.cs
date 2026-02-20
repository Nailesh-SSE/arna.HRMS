using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> GetAttendanceRequests([FromQuery] int? employeeId, [FromQuery] Status? status)
    {
        var result = await _attendanceRequestService.GetAttendanceRequests(employeeId, status);

        if (!result.IsSuccess)
            return BadRequest(result.Message); 

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
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _attendanceRequestService.CreateAttendanceRequestAsync(attendanceRequestDto);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateAttendanceRequest(int id, [FromBody] AttendanceRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.Id)
            return BadRequest("Invalid Id");

        var result = await _attendanceRequestService.UpdateAttendanceRequestAsync(dto); 

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAttendanceRequest(int id)
    {
        var result = await _attendanceRequestService.DeleteAttendanceRequestAsync(id);  

        if (!result.IsSuccess)
            return NotFound(result.Message);

        return Ok(result);
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
        var result = await _attendanceRequestService.GetPendingAttendanceRequestesAsync();
        return Ok(result);
    }

    [HttpPost("status/{id:int}")]
    [Authorize(Roles = UserRoleGroups.AdminRoles)]
    public async Task<IActionResult> ApproveAttendanceRequest(int id, [FromQuery] Status status)
    {
        if (status != Status.Approved && status != Status.Rejected)
            return BadRequest("Invalid status");

        var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value;

        if (string.IsNullOrWhiteSpace(employeeIdClaim))
            return Unauthorized("EmployeeId claim missing");

        if (!int.TryParse(employeeIdClaim, out var approvedBy))
            return Unauthorized("Invalid EmployeeId claim");

        var result = await _attendanceRequestService.UpdateAttendanceRequestStatusAsync(id, status, approvedBy);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }
}
