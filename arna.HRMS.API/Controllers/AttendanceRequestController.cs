using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceRequestController : ControllerBase
{
    private readonly IAttendanceRequestService _service;

    public AttendanceRequestController(IAttendanceRequestService service)
    {
        _service = service; 
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] int? employeeId, [FromQuery] Status? status)
    {
        var result = await _service.GetAttendanceRequestsAsync(employeeId, status);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetAttendanceRequestByIdAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] AttendanceRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.CreateAttendanceRequestAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] AttendanceRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.Id)
            return BadRequest("Invalid ID.");

        var result = await _service
            .UpdateAttendanceRequestAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAttendanceRequestAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelAsync(int id)
    {
        if (!TryGetEmployeeId(out var employeeId))
            return Unauthorized("Invalid EmployeeId claim.");

        var result = await _service.CancelAttendanceRequestAsync(id, employeeId);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("pending")]
    [Authorize(Roles = UserRoleGroups.AdminRoles)]
    public async Task<IActionResult> GetPendingAsync()
    {
        var result = await _service.GetPendingAttendanceRequestsAsync();

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("{id:int}/status")]
    [Authorize(Roles = UserRoleGroups.AdminRoles)]
    public async Task<IActionResult> UpdateStatusAsync(int id, [FromQuery] Status status)
    {
        if (status != Status.Approved && status != Status.Rejected)
            return BadRequest("Invalid status.");

        if (!TryGetEmployeeId(out var approvedBy))
            return Unauthorized("Invalid EmployeeId claim.");

        var result = await _service.UpdateAttendanceRequestStatusAsync(id, status, approvedBy);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    private bool TryGetEmployeeId(out int employeeId)
    {
        employeeId = 0;
        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value;
        return int.TryParse(claim, out employeeId);
    }
}