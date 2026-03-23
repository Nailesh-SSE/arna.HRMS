using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("daily-attendance-report")]
    public async Task<IActionResult> DailyAttendanceReportAsync(
    [FromQuery] int? year,
    [FromQuery] int? month,
    [FromQuery] int? employeeId,
    [FromQuery] AttendanceStatus? statusId,
    [FromQuery] DeviceType? device,
    [FromQuery] DateTime? FromDate,
    [FromQuery] DateTime? ToDate)
    {
        var result = await _reportService.GetDailyAttendanceReport(year, month, employeeId, statusId, device,FromDate, ToDate);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("employees-attendance-report")]
    public async Task<IActionResult> EmployeeAttendanceReportAsync(
    [FromQuery] int? year,
    [FromQuery] int? month,
    [FromQuery] int? employeeId,
    [FromQuery] DateTime? FromDate,
    [FromQuery] DateTime? ToDate)
    {
        var result = await _reportService.GetEmployeeAttendanceReport(year, month, employeeId, FromDate, ToDate);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("leave-summary-report")]
    public async Task<IActionResult> LeaveSummaryReportAsync(
        [FromQuery] int year,
        [FromQuery] int? month,
        [FromQuery] int? departmentId,
        [FromQuery] DateTime? FromDate,
        [FromQuery] DateTime? ToDate)
    {
        var result = await _reportService.GetLeaveSummaryReport(year, month, departmentId, FromDate, ToDate);

        if (!result.IsSuccess)
            return BadRequest(result.Message);
        
        return Ok(result);
    }
}
