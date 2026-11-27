using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAttendanceByStatusAndEmployeeIdAsync([FromQuery] AttendanceStatus? status, [FromQuery] int? employeeId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _attendanceService.GetAttendanceByStatusAndEmployeeIdAsync(status, employeeId, fromDate, toDate);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        try
        {
            var result = await _attendanceService.GetAttendanceByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] AttendanceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _attendanceService.CreateAttendanceAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyAsync([FromQuery] int year, [FromQuery] int month, [FromQuery] int? employeeId, [FromQuery] DateTime? date, [FromQuery] AttendanceStatus? statusId)
    {
        try
        {
            var result = await _attendanceService.GetAttendanceByMonthAsync(year, month, employeeId, date, statusId);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{employeeId:int}/last-today")]
    public async Task<IActionResult> GetLastTodayAsync(int employeeId)
    {
        try
        {
            var result = await _attendanceService.GetTodayLastEntryAsync(employeeId);
            return result.IsSuccess ? Ok(result) : NotFound(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("employees/{employeeId:int}/today/first-clock-in")]
    public async Task<IActionResult> GetTodayFirstClockIn(int employeeId)
    {
        try
        {
            var result = await _attendanceService.GetEmployeeTodayFirstClockInAsync(employeeId);
            return result.IsSuccess ? Ok(result) : NotFound(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
