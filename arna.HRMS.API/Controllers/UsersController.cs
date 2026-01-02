using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/users")]
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
        return user == null
            ? NotFound("User not found")
            : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _userServices.UserExistsAsync(dto.Username, dto.Email))
            return Conflict("Username or Email already exists");

        var createdUser = await _userServices.CreateUserAsync(dto);
        return Ok(createdUser);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Invalid User ID");

        var updated = await _userServices.UpdateUserAsync(dto);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var deleted = await _userServices.DeleteUserAsync(id);
        return deleted
            ? Ok()
            : NotFound("User not found");
    }

    [HttpPut("{id:int}/changepassword")]
    public async Task<IActionResult> ChangeUserPassword(int id, [FromBody] string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            return BadRequest("Password is required");
        var changed = await _userServices.ChangeUserPasswordAsync(id, newPassword);
        return changed  
            ? Ok()
            : NotFound("User not found");
    }
}
