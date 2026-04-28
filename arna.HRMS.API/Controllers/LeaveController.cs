using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _service;

    public LeaveController(ILeaveService service)
    {
        _service = service; 
    }

    // ============================
    // LEAVE TYPES
    // ============================

    [HttpGet("types")]
    public async Task<IActionResult> GetTypesAsync()
    {
        // Read role from JWT
        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        // Read employeeId from JWT
        var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value;
        int.TryParse(employeeIdClaim, out var employeeId);

        ServiceResult<List<LeaveTypeDto>> result;

        // If logged user is Employee → use employee logic
        if (role == UserRoleGroups.Employee && employeeId > 0)
        {
            result = await _service.GetEmployeeLeaveTypesAsync();
        }
        else
        {
            // Admin / HR / Manager / SuperAdmin
            result = await _service.GetLeaveTypesAsync();
        }

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("types/{id:int}")]
    public async Task<IActionResult> GetTypeByIdAsync(int id)
    {
        var result = await _service.GetLeaveTypeByIdAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    [HttpPost("types")]
    public async Task<IActionResult> CreateTypeAsync([FromBody] LeaveTypeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.CreateLeaveTypeAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("types/{id:int}")]
    public async Task<IActionResult> UpdateTypeAsync(int id, [FromBody] LeaveTypeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.Id)
            return BadRequest("Invalid leave type ID.");

        var result = await _service.UpdateLeaveTypeAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("types/{id:int}/delete")]
    public async Task<IActionResult> DeleteTypeAsync(int id)
    {
        var result = await _service.DeleteLeaveTypeAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    // ============================
    // LEAVE REQUESTS
    // ============================

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequestsAsync()
    {
        var result = await _service.GetLeaveRequestsAsync();

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("requests/{id:int}")]
    public async Task<IActionResult> GetRequestByIdAsync(int id)
    {
        var result = await _service.GetLeaveRequestByIdAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    [HttpGet("requests/filter")]
    public async Task<IActionResult> FilterAsync([FromQuery] Status? status, [FromQuery] int? employeeId)
    {
        var result = await _service.GetLeaveRequestsByFilterAsync(status, employeeId);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("requests/employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployeeAsync(int employeeId)
    {
        var result = await _service.GetLeaveRequestsByEmployeeIdAsync(employeeId);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequestAsync([FromBody] LeaveRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.CreateLeaveRequestAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("requests/{id:int}")]
    public async Task<IActionResult> UpdateRequestAsync(int id, [FromBody] LeaveRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.Id)
            return BadRequest("Invalid leave request ID.");

        var result = await _service.UpdateLeaveRequestAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("requests/{id:int}/delete")]
    public async Task<IActionResult> DeleteRequestAsync(int id)
    {
        var result = await _service.DeleteLeaveRequestAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    [HttpPost("requests/{id:int}/status")]
    public async Task<IActionResult> UpdateStatusAsync(int id, [FromQuery] Status status, [FromQuery] int approvedBy)
    {
        if (status is not (Status.Approved or Status.Rejected))
            return BadRequest("Invalid status.");

        var result = await _service.UpdateLeaveRequestStatusAsync(id, status, approvedBy);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("requests/{id:int}/cancel")]
    public async Task<IActionResult> CancelAsync(int id, [FromQuery] int employeeId)
    {
        var result = await _service.CancelLeaveRequestAsync(id, employeeId);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }
}