using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpPost("{id:int}/delete")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAttendanceRequestAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelAsync(int id, [FromQuery] int employeeId)
    {
        var result = await _service.CancelAttendanceRequestAsync(id, employeeId);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> UpdateStatusAsync(
        int id,
        [FromQuery] Status status,
        [FromQuery] int approvedBy)
    {
        if (id <= 0)
            return BadRequest("Invalid request ID.");

        if (status != Status.Approved && status != Status.Rejected)
            return BadRequest("Invalid status.");

        var result = await _service.UpdateAttendanceRequestStatusAsync(id, status, approvedBy);

        if (result == null)
            return BadRequest("Failed to update attendance request status.");

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("bulk-approve")]
    public async Task<IActionResult> BulkApproveAsync([FromBody] BulkApproveRequestDto dto)
    {
        if (dto == null || dto.RequestIds == null || !dto.RequestIds.Any())
            return BadRequest("No attendance request IDs provided.");

        if (dto.ApprovedBy <= 0)
            return BadRequest("Invalid approver ID.");

        var result = await _service.BulkApproveAttendanceRequestsAsync(dto.RequestIds, dto.ApprovedBy);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }
}
