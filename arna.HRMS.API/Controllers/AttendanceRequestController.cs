using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceRequestController : ControllerBase
{
    private readonly IAttendanceRequestService _AttendanceRequestService;
    public AttendanceRequestController(IAttendanceRequestService AttendanceRequestService)
    {
        _AttendanceRequestService = AttendanceRequestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAttendanceRequest()
    {
        var attendanceRequest = await _AttendanceRequestService.GetAttendanceRequestAsync();
        return Ok(attendanceRequest);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAttendanceRequestById(int id)
    {
        var attendance = await _AttendanceRequestService.GetAttendenceRequestByIdAsync(id);
        return attendance == null ? NotFound("not found") : Ok(attendance);
    }

    [HttpPost]
    public async Task<ActionResult<AttendanceDto>> CreateAttendanceRequest([FromBody] AttendanceRequestDto attendanceRequestDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var createdAttendance = await _AttendanceRequestService.CreateAttendanceRequestAsync(attendanceRequestDto);

        return Ok(createdAttendance);
    }

    [HttpPut("approveRequest/{id:int}")]
    public async Task<IActionResult> ApproveAttendanceRequest(int id)
    {
        var result = await _AttendanceRequestService.UpdateAttendanceRequestStatusAsync(id);

        if (!result)
            return NotFound(new { message = "Attendance request not found" });

        return Ok(new { message = "Attendance request approved successfully" });
    }
}
