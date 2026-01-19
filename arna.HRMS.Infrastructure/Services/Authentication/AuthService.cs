using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Configuration;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Services.Authentication.Interfaces;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Models.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RegisterRequest = arna.HRMS.Core.DTOs.Requests.RegisterRequest;

namespace arna.HRMS.Infrastructure.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;
    private readonly IUserServices _userServices;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;
    private readonly ApplicationDbContext _context;

    public AuthService(
        IJwtService jwtService,
        IUserServices userServices,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings,
        ApplicationDbContext context)
    {
        _jwtService = jwtService;
        _userServices = userServices;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
        _context = context;
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        if (request == null)
            return ServiceResult<AuthResponse>.Fail("Invalid request");

        if (string.IsNullOrWhiteSpace(request.Email))
            return ServiceResult<AuthResponse>.Fail("Email is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            return ServiceResult<AuthResponse>.Fail("Password is required");

        var user = await _userServices.GetUserByUserNameAndEmail(request.Email);

        if (user == null || user.Password != request.Password)
            return ServiceResult<AuthResponse>.Fail("Invalid email or password");

        var authData = await GenerateAuthResponseAsync(user, "Login successful");

        return ServiceResult<AuthResponse>.Success(authData, "Login successful");
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        if (request == null)
            return ServiceResult<AuthResponse>.Fail("Invalid request");

        if (string.IsNullOrWhiteSpace(request.Username))
            return ServiceResult<AuthResponse>.Fail("Username is required");

        if (string.IsNullOrWhiteSpace(request.Email))
            return ServiceResult<AuthResponse>.Fail("Email is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            return ServiceResult<AuthResponse>.Fail("Password is required");

        if (request.Password.Length < 6)
            return ServiceResult<AuthResponse>.Fail("Password must be at least 6 characters");

        if (await _userServices.UserExistsAsync(request.Email))
            return ServiceResult<AuthResponse>.Fail("Email already exists");

        var model = _mapper.Map<UserDto>(request);
        var user = await _userServices.CreateUserEntityAsync(model);

        if (user == null)
            return ServiceResult<AuthResponse>.Fail("User creation failed");

        var authData = await GenerateAuthResponseAsync(user, "Registration successful");

        return ServiceResult<AuthResponse>.Success(authData, "Registration successful");
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        if (request == null)
            return ServiceResult<AuthResponse>.Fail("Invalid request");

        if (request.UserId <= 0)
            return ServiceResult<AuthResponse>.Fail("Invalid UserId");

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return ServiceResult<AuthResponse>.Fail("RefreshToken is required");

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId);

        if (user == null)
            return ServiceResult<AuthResponse>.Fail("User not found");

        if (string.IsNullOrWhiteSpace(user.RefreshToken) || user.RefreshTokenExpiryTime == null)
            return ServiceResult<AuthResponse>.Fail("No refresh token found. Please login again.");

        if (user.RefreshToken != request.RefreshToken)
            return ServiceResult<AuthResponse>.Fail("Invalid refresh token. Please login again.");

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return ServiceResult<AuthResponse>.Fail("Refresh token has expired. Please login again.");

        var authData = await GenerateAuthResponseAsync(user, "Token refreshed successfully");

        return ServiceResult<AuthResponse>.Success(authData, "Token refreshed successfully");
    }

    public async Task<ServiceResult<bool>> LogoutAsync(int userId)
    {
        if (userId <= 0)
            return ServiceResult<bool>.Fail("Invalid UserId");

        await _jwtService.RevokeRefreshTokenAsync(userId);

        return ServiceResult<bool>.Success(true, "Logged out successfully");
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user, string message)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken);

        return new AuthResponse
        {
            IsSuccess = true,
            Message = message,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationInMinutes),
            UserId = user.Id,
            Username = user.Username ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmployeeId = user.EmployeeId,
            Role = user.Role
        };
    }
}
