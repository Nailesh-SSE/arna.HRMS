using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Models.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _AttendanceService;
    private readonly IMapper _mapper;
    public AttendanceController(IAttendanceService AttendanceService, IMapper mapper)
    {
        _AttendanceService = AttendanceService;
        _mapper = mapper;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendance()
    {
        var attendance = await _AttendanceService.GetAttendanceAsync();
        return Ok(_mapper.Map<IEnumerable<AttendanceDto>>(attendance));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AttendanceDto>> GetAttendance(int id)
    {
        var attendance = await _AttendanceService.GetAttendenceByIdAsync(id);
        if (attendance == null) return NotFound();
        return Ok(attendance);
    }

    [HttpPost]
    //[Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<AttendanceDto>> CreateAttendance(CreateAttendanceRequest attendanceDto)
    {
        var attendance = _mapper.Map<Attendance>(attendanceDto);
        var createdAttendance = await _AttendanceService.CreateAttendanceAsync(attendance);

        return CreatedAtAction(
            nameof(GetAttendance),
            new { id = createdAttendance.Id },
            _mapper.Map<AttendanceDto>(createdAttendance));
    }
    [HttpGet("by-month")]
    public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendanceByMonth(int year,int month)
    {
        var attendance = await _AttendanceService.GetAttendanceByMonthAsync(year, month);
        return Ok(attendance);
    }
}
