using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _AttendanceService;
    public AttendanceController(IAttendanceService AttendanceService)
    {
        _AttendanceService = AttendanceService;
    }
 
    [HttpGet]
    public async Task<IActionResult> GetAttendance()
    {
        var attendance = await _AttendanceService.GetAttendanceAsync();
        return Ok(attendance);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAttendanceById(int id)
    {
        var attendance = await _AttendanceService.GetAttendenceByIdAsync(id);
        return attendance == null? NotFound("Attendance not found") : Ok(attendance);
    }

    [HttpPost]
    public async Task<ActionResult<AttendanceDto>> CreateAttendance([FromBody] AttendanceDto attendanceDto)
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);
        var createdAttendance = await _AttendanceService.CreateAttendanceAsync(attendanceDto);

        return Ok(createdAttendance);
    }

    [HttpGet("monthly")]
    public async Task<ActionResult<IEnumerable<MonthlyAttendanceDto>>> GetAttendanceByMonth(int year,int month, int EmpId)
    {
        var attendance = await _AttendanceService.GetAttendanceByMonthAsync(year, month, EmpId);
        return Ok(attendance);
    }

    [HttpGet("clockStatus/{id:int}")]
    public async Task<ActionResult<AttendanceDto?>> GetLastToday(int id)
    {
        var attendance = await _AttendanceService.GetTodayLastEntryAsync(id);
        return Ok(attendance);
    }


}
