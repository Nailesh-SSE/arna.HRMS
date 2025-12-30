using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Models.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserServices _userServices; 
    private readonly IMapper _mapper;

    public UsersController(IUserServices userServices, IMapper mapper)
    {
        _userServices = userServices;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<UserDto>> GetUsers()
    {
        var users = await _userServices.GetUserAsync();
        if (users == null || !users.Any()) return Ok(new List<UserDto>());
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        var user = await _userServices.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserDto userDto)
    {
        var user = _mapper.Map<Core.Entities.User>(userDto);
        var createdUser = await _userServices.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto userDto)
    {
        if (id != userDto.Id)
        {
            return BadRequest();
        }
        var user = _mapper.Map<Core.Entities.User>(userDto);
        await _userServices.UpdateUserAsync(user);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var deleted = await _userServices.DeleteUserAsync(id);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}
