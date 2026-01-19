using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers.Client;

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

    [HttpPost("approveRequest/{id:int}")]
    public async Task<IActionResult> ApproveAttendanceRequest(int id)
    {
        var result = await _attendanceRequestService.UpdateAttendanceRequestStatusAsync(id);

        if (!result.IsSuccess)
            return NotFound(result.Message);

        return Ok(result);
    }
}
