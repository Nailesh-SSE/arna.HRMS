using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.DTOs.Auth;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Dependency.Identity;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace arna.HRMS.Infrastructure.Services;

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

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ServiceResult<AuthResponse>.Fail("Username and password are required.");
        }

        var user = await _userServices.GetUserByUserNameOrEmailAsync(request.UserName);

        if (user == null || !VerifyPassword(request.Password, user.Password))
            return ServiceResult<AuthResponse>.Fail("Invalid username or password.");

        var authResponse = await GenerateAuthResponseAsync(user);

        return ServiceResult<AuthResponse>.Success(authResponse, "Login successful.");
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(UserDto dto)
    {
        if (dto == null)
            return ServiceResult<AuthResponse>.Fail("Invalid request.");

        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return ServiceResult<AuthResponse>.Fail("Username, email and password are required.");
        }

        if (dto.Password.Length < 6)
            return ServiceResult<AuthResponse>.Fail("Password must be at least 6 characters.");

        if (await _userServices.UserExistsAsync(dto.Email, dto.PhoneNumber, dto.Id))
            return ServiceResult<AuthResponse>.Fail("Email or phone number already exists.");

        dto.Password = HashPassword(dto.Password);

        var user = await _userServices.CreateUserEntityAsync(dto);

        if (user == null)
            return ServiceResult<AuthResponse>.Fail("User creation failed.");

        var authResponse = await GenerateAuthResponseAsync(user);

        return ServiceResult<AuthResponse>.Success(authResponse, "Registration successful.");
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(RefreshTokenDto request)
    {
        if (request == null || request.UserId <= 0 || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return ServiceResult<AuthResponse>.Fail("Invalid request.");
        }

        var user = await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == request.UserId);

        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ServiceResult<AuthResponse>.Fail("Invalid or expired refresh token.");
        }

        var authResponse = await GenerateAuthResponseAsync(user);

        return ServiceResult<AuthResponse>.Success(authResponse, "Token refreshed successfully.");
    }

    public async Task<ServiceResult<bool>> LogoutAsync(int userId)
    {
        if (userId <= 0)
            return ServiceResult<bool>.Fail("Invalid user id.");

        await _jwtService.RevokeRefreshTokenAsync(userId);

        return ServiceResult<bool>.Success(true, "Logged out successfully.");
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken);

        return new AuthResponse
        {
            IsSuccess = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationInMinutes),
            UserId = user.Id,
            Username = user.Username ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmployeeId = user.EmployeeId,
            Role = user.Role?.Name ?? string.Empty
        };
    }

    private static string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string inputPassword, string userPassword)
    {
        return inputPassword == userPassword; 
    }
}