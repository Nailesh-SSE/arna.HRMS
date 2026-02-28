using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _service;

    public RoleController(IRoleService service)
    {
        _service = service; 
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        var result = await _service.GetRolesAsync();

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetRoleByIdAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    [HttpGet("by-name")]
    public async Task<IActionResult> GetByNameAsync([FromQuery] string name)
    {
        var result = await _service.GetRoleByNameAsync(name);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] RoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.CreateRoleAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] RoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.Id)
            return BadRequest("Invalid role ID.");

        var result = await _service.UpdateRoleAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteRoleAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }
}