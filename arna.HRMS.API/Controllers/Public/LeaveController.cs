using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeaveController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    // ============================
    // LEAVE TYPE
    // ============================

    [HttpGet("Types")]
    public async Task<IActionResult> GetLeaveTypes()
    {
        var result = await _leaveService.GetLeaveTypeAsync();
        return Ok(result);
    }

    [HttpGet("Types/{id:int}")]
    public async Task<IActionResult> GetLeaveTypesById(int id)
    {
        var leave = await _leaveService.GetLeaveTypeByIdAsync(id);
        return leave.Data == null ? NotFound("Leave Type not found") : Ok(leave);
    }

    [HttpPost("Types")]
    public async Task<IActionResult> CreateLeaveType([FromBody] LeaveTypeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _leaveService.CreateLeaveTypeAsync(dto);
        return Ok(created);
    }

    [HttpPost("Types/{id:int}")]
    public async Task<IActionResult> UpdateLeaveType(int id, [FromBody] LeaveTypeDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Invalid Id");

        var updated = await _leaveService.UpdateLeaveTypeAsync(dto);
        return Ok(updated);
    }

    [HttpDelete("Types/{id:int}")]
    public async Task<IActionResult> DeleteLeaveType(int id)
    {
        var deletedResult = await _leaveService.DeleteLeaveTypeAsync(id);
        return deletedResult.Data ? Ok() : NotFound("Leave Type not found");
    }

    // ============================
    // LEAVE REQUESTS
    // ============================

    [HttpGet("requests")]
    public async Task<IActionResult> GetLeaveRequests()
    {
        var result = await _leaveService.GetLeaveRequestAsync();
        return Ok(result);
    }

    [HttpGet("requests/{id:int}")]
    public async Task<IActionResult> GetLeaveRequestById(int id)
    {
        var leave = await _leaveService.GetLeaveRequestByIdAsync(id);
        return leave.Data == null ? NotFound("Leave request not found") : Ok(leave);
    }

    [HttpPost("requests")]
    public async Task<IActionResult> CreateLeaveRequest([FromBody] LeaveRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _leaveService.CreateLeaveRequestAsync(dto);
        return Ok(created);
    }

    [HttpPost("requests/{id:int}")]
    public async Task<IActionResult> UpdateLeaveRequest(int id, [FromBody] LeaveRequestDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Invalid Id");

        if (dto.StatusId != Status.Pending)
            return BadRequest("Only pending leave requests can be updated");

        var updated = await _leaveService.UpdateLeaveRequestAsync(dto);
        return Ok(updated);
    }

    [HttpDelete("requests/{id:int}")]
    public async Task<IActionResult> DeleteLeaveRequest(int id)
    {
        var deleted = await _leaveService.DeleteLeaveRequestAsync(id);
        return deleted.Data ? Ok() : NotFound("Leave request not found");
    }

    [HttpGet("requests/filter")]
    public async Task<IActionResult> GetLeaveRequestsByFilter(Status? status, int? empId)
    {
        var result = await _leaveService.GetByFilterAsync(status, empId);
        return Ok(result);
    }

    [HttpPost("requests/status/{leaveRequestId:int}")]
    [Authorize(Roles = UserRoleGroups.AdminRoles)]
    public async Task<IActionResult> UpdateLeaveStatus(int leaveRequestId, [FromQuery] Status status)
    {
        if (status != Status.Approved && status != Status.Rejected)
            return BadRequest("Invalid status");

        var employeeIdClaim = User.Claims
            .FirstOrDefault(c => c.Type == "EmployeeId")
            ?.Value;

        if (string.IsNullOrWhiteSpace(employeeIdClaim))
            return Unauthorized("EmployeeId claim missing");

        if (!int.TryParse(employeeIdClaim, out var approvedBy))
            return Unauthorized("Invalid EmployeeId claim");


        var result = await _leaveService.UpdateStatusLeaveAsync(leaveRequestId, status, approvedBy);

        if (!result.Data)
            return BadRequest("Invalid leave request");

        return Ok($"Leave {status} successfully");
    }

    [HttpGet("requests/employee/{employeeid:int}")]
    public async Task<IActionResult> GetEmployeeLeaveRequest(int employeeid)
    {
        var data = await _leaveService.GetLeaveRequestByEmployeeIdAsync(employeeid);
        return Ok(data);
    }

    [HttpPost("requests/cancel/{leaveRequestId:int}")]
    public async Task<IActionResult> CancleLeaveRequest(int leaveRequestId, int employeeid)
    {
        var result = await _leaveService.UpdateLeaveRequestStatusCancelAsync(leaveRequestId, employeeid);
        if (!result.Data)
            return BadRequest("Invalid leave request");

        return Ok(result);
    }
}
