using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{

    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployeeAndAdminDashboardAsync([FromQuery] Status? status, [FromQuery] int? employeeId)
    {
        var dashboard = await _dashboardService.GetDashboardAsync(status, employeeId);

        return Ok(dashboard);
    }
}
