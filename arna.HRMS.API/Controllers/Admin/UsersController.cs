using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers.Admin;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserServices _userServices;

    public UsersController(IUserServices userServices)
    {
        _userServices = userServices;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userServices.GetUserAsync();
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userServices.GetUserByIdAsync(id);
        if (!user.IsSuccess)
            return NotFound(user.Message);

        return user == null ? NotFound("User not found") : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userServices.CreateUserAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto dto)
    {
        if(dto.Id != id)
            return BadRequest("Invalid Id");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userServices.UpdateUserAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userServices.DeleteUserAsync(id);

        if (!result.IsSuccess)
            return NotFound(result.Message);

        return Ok(result);
    }

    [HttpPost("{id:int}/change-password")]
    public async Task<IActionResult> ChangeUserPassword(int id, [FromBody] string newPassword)
    {
        var result = await _userServices.ChangeUserPasswordAsync(id, newPassword);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }
}
