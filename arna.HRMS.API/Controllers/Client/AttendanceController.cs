using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Services.Interfaces;
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

    [HttpGet("employee-attendance")]
    public async Task<IActionResult> GetEmployeeAttendance(
      AttendanceStatus? status,
      int? employeeId)
    {
        var result = await _attendanceService
            .GetEmployeeAttendanceByStatusAsync(status, employeeId);

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
        if(!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _attendanceService.CreateAttendanceAsync(attendanceDto);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("monthly-attendance")]
    public async Task<IActionResult> GetAttendanceByMonth([FromQuery] int year, [FromQuery] int month, [FromQuery] int? empId, [FromQuery] DateTime? date, [FromQuery] AttendanceStatus? statusId)
    {
        var result = await _attendanceService.GetAttendanceByMonthAsync(year, month, empId, date, statusId);

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
