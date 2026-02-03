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
    // LEAVE MASTER
    // ============================

    [HttpGet("masters")]
    public async Task<IActionResult> GetLeaveMasters()
    {
        var result = await _leaveService.GetLeaveMasterAsync();
        return Ok(result);
    }

    [HttpPost("masters")]
    public async Task<IActionResult> CreateLeaveMaster([FromBody] LeaveMasterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _leaveService.LeaveExistsAsync(dto.LeaveName);
        if (result.Data)
            return Conflict("Leave already exists");

        var created = await _leaveService.CreateLeaveMasterAsync(dto);
        return Ok(created);
    }

    [HttpPost("masters/{id:int}")]
    public async Task<IActionResult> UpdateLeaveMaster(int id, [FromBody] LeaveMasterDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Invalid Id");

        var updated = await _leaveService.UpdateLeaveMasterAsync(dto);
        return Ok(updated);
    }

    [HttpDelete("masters/{id:int}")]
    public async Task<IActionResult> DeleteLeaveMaster(int id)
    {
        var deletedResult = await _leaveService.DeleteLeaveMasterAsync(id);
        return deletedResult.Data ? Ok() : NotFound("Leave master not found");
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

        if (dto.Status != Status.Pending)
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

    [HttpGet("requests/{status}")]
    [Authorize(Roles = UserRoleGroups.AdminRoles)]
    public async Task<IActionResult> GetLeaveRequestsByStatus(Status status)
    {
        var result = await _leaveService.GetByStatusAsync(status);
        return Ok(result);
    }

    [HttpPost("requests/status/{leaveRequestId:int}")]
    [Authorize(Roles = UserRoleGroups.AdminRoles)]
    public async Task<IActionResult> UpdateLeaveStatus(int leaveRequestId, [FromQuery] Status status)
    {
        if (status != Status.Approved && status != Status.Rejected)
            return BadRequest("Invalid status");

        int approvedBy =
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

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
        return Ok("Leave request cancelled successfully");
    }

    // ============================
    // LEAVE BALANCES
    // ============================

    [HttpGet("balances")]
    public async Task<IActionResult> GetLeaveBalances()
    {
        var result = await _leaveService.GetLeaveBalanceAsync();
        return Ok(result);
    }

    [HttpGet("balances/employee/{employeeId:int}")]
    public async Task<IActionResult> GetLeaveBalanceByEmployee(int employeeId)
    {
        var balance =
            await _leaveService.GetLeaveBalanceByEmployeeIdAsync(employeeId);

        return balance == null
            ? NotFound("Leave balance not found")
            : Ok(balance);
    }

    [HttpPost("balances/{id:int}")]
    public async Task<IActionResult> UpdateLeaveBalance(int id, [FromBody] EmployeeLeaveBalanceDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Invalid Id");

        var updated = await _leaveService.UpdateLeaveBalanceAsync(dto);

        return Ok(updated);
    }

    [HttpDelete("balances/{id:int}")]
    public async Task<IActionResult> DeleteLeaveBalance(int id)
    {
        var deleted = await _leaveService.DeleteLeaveBalanceAsync(id);

        return deleted.Data ? Ok() : NotFound("Leave balance not found");
    }
}
