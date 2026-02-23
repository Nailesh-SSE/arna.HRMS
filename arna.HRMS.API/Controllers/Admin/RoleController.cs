using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers.Admin;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var result = await _roleService.GetRoleAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRoleById(int id)
    {
        var result = await _roleService.GetRoleByIdAsync(id); 
        if(!result.IsSuccess)
            return NotFound(result.Message);
        return result == null ? NotFound("Role not found") : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] RoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _roleService.CreateRoleAsync(dto); 

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleDto dto)
    {
        if(dto.Id != id)
            return BadRequest("Invalid Id");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _roleService.UpdateRoleAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result); 
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var result = await _roleService.DeleteRoleAsync(id);

        if (!result.IsSuccess)
            return NotFound(result.Message); 

        return Ok(result);
    }
}
