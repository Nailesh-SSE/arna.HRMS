using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RegisterRequest = arna.HRMS.Core.DTOs.Requests.RegisterRequest;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return result.IsSuccess ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);

            if (result.IsSuccess)
            {
                TestTokenStore.Token = result.AccessToken;
                _logger.LogInformation($"New user '{request.Username}' registered successfully");
                return Ok(result);
            }
            else
            {
                _logger.LogWarning($"Registration failed for user '{request.Username}': {result.Message}");
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during registration for user '{request.Username}'");
            return StatusCode(500, new AuthResponse
            {
                IsSuccess = false,
                Message = "An internal error occurred during registration"
            });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authService.LogoutAsync(userId);
        return Ok();
    }
}
