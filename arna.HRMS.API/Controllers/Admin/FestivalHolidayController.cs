using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers.Admin;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FestivalHolidayController : ControllerBase
{
    private readonly IFestivalHolidayService _holidayService;

    public FestivalHolidayController(IFestivalHolidayService holidayService)
    {
        _holidayService = holidayService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFestivalHoliday()
    {
        var result = await _holidayService.GetFestivalHolidayAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetFestivalHolidayById(int id)
    {
        var result = await _holidayService.GetFestivalHolidayByIdAsync(id);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetHolidaysByMonth([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _holidayService.GetFestivalHolidayByMonthAsync(year, month);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFestivalHoliday([FromBody] FestivalHolidayDto festivalHolidayDto)
    {
        var result = await _holidayService.CreateFestivalHolidayAsync(festivalHolidayDto);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateFestivalHoliday(int id, [FromBody] FestivalHolidayDto festivalHolidayDto)
    {
        festivalHolidayDto.Id = id;

        var result = await _holidayService.UpdateFestivalHolidayAsync(festivalHolidayDto);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteFestivalHoliday(int id)
    {
        var result = await _holidayService.DeleteFestivalHolidayAsync(id);

        if (!result.IsSuccess)
            return NotFound(result.Message);

        return Ok(result);
    }

    [HttpGet("by-name")]
    public async Task<IActionResult> CheckFestivalHolidayName([FromQuery] string name)
    {
        var result = await _holidayService.GetFestivalHolidayByNameAsync(name);
        if (!result.IsSuccess)
            return BadRequest(result.Message);
        return Ok(result);
    }
}
