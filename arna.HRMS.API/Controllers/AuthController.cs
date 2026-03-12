// ============================================================
// FILE: arna.HRMS.API/Controllers/AuthController.cs
// ============================================================
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

    // ──────────────────────────────────────────────────────────
    // POST api/auth/login
    // Anyone can call this — no [Authorize] needed
    // Returns: AccessToken + RefreshToken + user info
    // ──────────────────────────────────────────────────────────
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var result = await _authService.LoginAsync(request);
        return result.IsSuccess ? Ok(result) : Unauthorized(result.Message);
    }

    // ──────────────────────────────────────────────────────────
    // POST api/auth/register
    // ──────────────────────────────────────────────────────────
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    // ──────────────────────────────────────────────────────────
    // POST api/auth/refresh
    // Called by Web app when AccessToken expires (401 received)
    // Body: { "userId": 1, "refreshToken": "..." }
    // Returns: new AccessToken + RefreshToken
    // ──────────────────────────────────────────────────────────
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return result.IsSuccess ? Ok(result) : Unauthorized(result.Message);
    }

    // ──────────────────────────────────────────────────────────
    // POST api/auth/logout
    // Requires valid AccessToken — reads UserId from JWT claims
    // Revokes the refresh token in DB
    // ──────────────────────────────────────────────────────────
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized("Invalid token.");

        var result = await _authService.LogoutAsync(userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    // ──────────────────────────────────────────────────────────
    // Helper — reads UserId from JWT claim
    // ──────────────────────────────────────────────────────────
    private bool TryGetUserId(out int userId)
    {
        userId = 0;
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out userId);
    }
}