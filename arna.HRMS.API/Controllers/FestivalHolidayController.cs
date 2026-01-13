using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FestivalHolidayController : ControllerBase
{
    private readonly IFestivalHolidayService _holidayService;
    public FestivalHolidayController(IFestivalHolidayService HolidayService)
    {
        _holidayService = HolidayService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFestivalHoliday()
    {
        var holiday= await _holidayService.GetFestivalHolidayAsync();
        return Ok(holiday);
    }
    [HttpGet("monthly")]
    public async Task<ActionResult<IEnumerable<FestivalHolidayDto>>> GetHolidaysByMonth(int year, int month)
    {
        var attendance = await _holidayService.GetFestivalHolidayByMonthAsync(year, month);
        return Ok(attendance);
    }
}
