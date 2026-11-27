using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _service;

    public LeaveController(ILeaveService service)
    {
        _service = service; 
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetTypesAsync()
    {
        try
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value;
            int.TryParse(employeeIdClaim, out var employeeId);

            ServiceResult<List<LeaveTypeDto>> result;

            if (role == UserRoleGroups.Employee && employeeId > 0)
            {
                result = await _service.GetEmployeeLeaveTypesAsync();
            }
            else
            {
                result = await _service.GetLeaveTypesAsync();
            }

            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpGet("types/{id:int}")]
    public async Task<IActionResult> GetTypeByIdAsync(int id)
    {
        try
        {
            var result = await _service.GetLeaveTypeByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpPost("types")]
    public async Task<IActionResult> CreateTypeAsync([FromBody] LeaveTypeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateLeaveTypeAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpPost("types/{id:int}")]
    public async Task<IActionResult> UpdateTypeAsync(int id, [FromBody] LeaveTypeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest("Invalid leave type ID.");

            var result = await _service.UpdateLeaveTypeAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpPost("types/{id:int}/delete")]
    public async Task<IActionResult> DeleteTypeAsync(int id)
    {
        try
        {
            var result = await _service.DeleteLeaveTypeAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequestsAsync()
    {
        try
        {
            var result = await _service.GetLeaveRequestsAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpGet("requests/{id:int}")]
    public async Task<IActionResult> GetRequestByIdAsync(int id)
    {
        try
        {
            var result = await _service.GetLeaveRequestByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpGet("requests/filter")]
    public async Task<IActionResult> FilterAsync([FromQuery] Status? status, [FromQuery] int? employeeId)
    {
        try
        {
            var result = await _service.GetLeaveRequestsByFilterAsync(status, employeeId);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpGet("requests/employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployeeAsync(int employeeId)
    {
        try
        {
            var result = await _service.GetLeaveRequestsByEmployeeIdAsync(employeeId);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequestAsync([FromBody] LeaveRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateLeaveRequestAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpPost("requests/{id:int}")]
    public async Task<IActionResult> UpdateRequestAsync(int id, [FromBody] LeaveRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest("Invalid leave request ID.");

            var result = await _service.UpdateLeaveRequestAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpPost("requests/{id:int}/delete")]
    public async Task<IActionResult> DeleteRequestAsync(int id)
    {
        try
        {
            var result = await _service.DeleteLeaveRequestAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpPost("requests/{id:int}/status")]
    public async Task<IActionResult> UpdateStatusAsync(int id, [FromQuery] Status status, [FromQuery] int approvedBy)
    {
        try
        {
            if (status is not (Status.Approved or Status.Rejected))
                return BadRequest("Invalid status.");

            var result = await _service.UpdateLeaveRequestStatusAsync(id, status, approvedBy);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpPost("requests/bulk-approve")]
    public async Task<IActionResult> BulkApproveAsync([FromBody] BulkApproveRequestDto dto)
    {
        try
        {
            if (dto == null || dto.RequestIds == null || !dto.RequestIds.Any())
                return BadRequest("No leave request IDs provided.");

            if (dto.ApprovedBy <= 0)
                return BadRequest("Invalid approver ID.");

            var result = await _service.BulkApproveLeaveRequestsAsync(dto.RequestIds, dto.ApprovedBy);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }

    [HttpPost("requests/{id:int}/cancel")]
    public async Task<IActionResult> CancelAsync(int id, [FromQuery] int employeeId)
    {
        try
        {
            var result = await _service.CancelLeaveRequestAsync(id, employeeId);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, msg);
        }
    }
}
