using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        try
        {
            var result = await _service.GetUsersAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        try
        {
            var result = await _service.GetUserByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] UserDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateUserAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UserDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest("Invalid user ID.");

            var result = await _service.UpdateUserAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("{id:int}/delete")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        try
        {
            var result = await _service.DeleteUserAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("{id:int}/change-password")]
    public async Task<IActionResult> ChangePasswordAsync(int id, [FromBody] string newPassword)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.ChangeUserPasswordAsync(id, newPassword);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
