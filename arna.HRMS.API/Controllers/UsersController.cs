using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserServices _service;

    public UsersController(IUserServices service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        var result = await _service.GetUsersAsync();

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetUserByIdAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] UserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.CreateUserAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.Id)
            return BadRequest("Invalid user ID.");

        var result = await _service.UpdateUserAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteUserAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    [HttpPost("{id:int}/change-password")]
    public async Task<IActionResult> ChangePasswordAsync(int id, [FromBody] string newPassword)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.ChangeUserPasswordAsync(id, newPassword);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }
}