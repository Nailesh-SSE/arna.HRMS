using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers.Client;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAttendance()
    {
        var result = await _attendanceService.GetAttendanceAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAttendanceById(int id)
    {
        var result = await _attendanceService.GetAttendenceByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(result.Message);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAttendance([FromBody] AttendanceDto attendanceDto)
    {
        var result = await _attendanceService.CreateAttendanceAsync(attendanceDto);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetAttendanceByMonth(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int empId)
    {
        var result = await _attendanceService.GetAttendanceByMonthAsync(year, month, empId);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("clockStatus/{id:int}")]
    public async Task<IActionResult> GetLastToday(int id)
    {
        var result = await _attendanceService.GetTodayLastEntryAsync(id);

        if (!result.IsSuccess)
            return NotFound(result.Message);

        return Ok(result);
    }
}
