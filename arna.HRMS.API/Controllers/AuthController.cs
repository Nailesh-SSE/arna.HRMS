using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);

            if (result.IsSuccess)
            {
                TestTokenStore.Token = result.Token;
                _logger.LogInformation($"User '{request.Email}' logged in successfully");
                return Ok(result);
            }
            else
            {
                _logger.LogWarning($"Login failed for user '{request.Email}': {result.Message}");
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during login for user '{request.Email}'");
            return StatusCode(500, new AuthResponse
            {
                IsSuccess = false,
                Message = "An internal error occurred during login"
            });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);

            if (result.IsSuccess)
            {
                TestTokenStore.Token = result.Token;
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

    [HttpPost("logout")]
    public ActionResult LogOut()
    {
        try
        {
            TestTokenStore.Token = null;
            _logger.LogInformation("User logged out successfully");
            return Ok(new { IsSuccess = true, Message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { IsSuccess = false, Message = "An internal error occurred during logout" });
        }
    }
}
