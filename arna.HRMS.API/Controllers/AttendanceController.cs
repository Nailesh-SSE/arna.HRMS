using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

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
    public async Task<IActionResult> GetAsync([FromQuery] AttendanceStatus? status, [FromQuery] int? employeeId)
    {
        var result = await _attendanceService.GetEmployeeAttendanceByStatusAsync(status, employeeId);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _attendanceService.GetAttendanceByIdAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] AttendanceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _attendanceService.CreateAttendanceAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyAsync([FromQuery] int year, [FromQuery] int month, [FromQuery] int? employeeId, [FromQuery] DateTime? date, [FromQuery] AttendanceStatus? status)
    {
        var result = await _attendanceService.GetAttendanceByMonthAsync(year, month, employeeId, date, status);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("{employeeId:int}/last-today")]
    public async Task<IActionResult> GetLastTodayAsync(int employeeId)
    {
        var result = await _attendanceService.GetTodayLastEntryAsync(employeeId);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }
}