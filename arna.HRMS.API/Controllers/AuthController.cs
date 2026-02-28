using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.DTOs.Auth;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var result = await _authService.LoginAsync(request);

        return result.IsSuccess ? Ok(result) : Unauthorized(result.Message);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] UserDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);

        return result.IsSuccess ? Ok(result) : Unauthorized(result.Message);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized("Invalid token user id.");

        var result = await _authService.LogoutAsync(userId);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    private bool TryGetUserId(out int userId)
    {
        userId = 0;
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out userId);
    }
}