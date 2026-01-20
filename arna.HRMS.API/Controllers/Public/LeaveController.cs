using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Models.DTOs;
using arna.HRMS.Models.Enums;
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

        if (await _leaveService.LeaveExistsAsync(dto.LeaveName))
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
        var deleted = await _leaveService.DeleteLeaveMasterAsync(id);
        return deleted ? Ok() : NotFound("Leave master not found");
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
        return leave == null ? NotFound("Leave request not found") : Ok(leave);
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

        if (dto.Status != LeaveStatusList.Pending)
            return BadRequest("Only pending leave requests can be updated");

        var updated = await _leaveService.UpdateLeaveRequestAsync(dto);
        return Ok(updated);
    }

    [HttpDelete("requests/{id:int}")]
    public async Task<IActionResult> DeleteLeaveRequest(int id)
    {
        var deleted = await _leaveService.DeleteLeaveRequestAsync(id);
        return deleted ? Ok() : NotFound("Leave request not found");
    }

    [HttpGet("requests/pending")]
    public async Task<IActionResult> GetPendingLeaveRequests()
    {
        var result = await _leaveService.GetLeaveRequestAsync();
        var pending = result.Where(x => x.Status == LeaveStatusList.Pending);
        return Ok(pending);
    }

    [HttpPost("requests/status/{id:int}")]
    [Authorize(Roles = "SuperAdmin,Admin,HR,Manager")]
    public async Task<IActionResult> UpdateLeaveStatus(int id, [FromQuery] LeaveStatus status)
    {
        if (status != LeaveStatus.Approved && status != LeaveStatus.Rejected)
            return BadRequest("Invalid status");

        int approvedBy =
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var result =
            await _leaveService.UpdateStatusLeaveAsync(id, status, approvedBy);

        if (!result)
            return BadRequest("Invalid leave request");

        return Ok($"Leave {status} successfully");
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

        return deleted ? Ok() : NotFound("Leave balance not found");
    }
}
