using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FestivalHolidayController : ControllerBase
{
    private readonly IFestivalHolidayService _service;

    public FestivalHolidayController(IFestivalHolidayService service)
    {
        _service = service; 
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        var result = await _service.GetFestivalHolidaysAsync();

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetFestivalHolidayByIdAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetByMonthAsync([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _service.GetFestivalHolidaysByMonthAsync(year, month);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("by-name")]
    public async Task<IActionResult> GetByNameAsync([FromQuery] string name)
    {
        var result = await _service.GetFestivalHolidaysByNameAsync(name);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] FestivalHolidayDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.CreateFestivalHolidayAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] FestivalHolidayDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.Id)
            return BadRequest("Invalid festival holiday ID.");

        var result = await _service.UpdateFestivalHolidayAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteFestivalHolidayAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }
}