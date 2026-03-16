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

    [HttpGet("attendance-report")]
    public async Task<IActionResult> AttendanceReportAsync(
    [FromQuery] int? year,
    [FromQuery] int? month,
    [FromQuery] int? employeeId,
    [FromQuery] AttendanceStatus? statusId,
    [FromQuery] DeviceType? device)
    {
        var result = await _reportService.GetAttendanceReport(year, month, employeeId, statusId, device);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }
}
