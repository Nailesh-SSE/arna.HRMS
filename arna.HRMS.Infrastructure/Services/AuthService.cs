// ============================================================
// FILE: arna.HRMS.Infrastructure/Services/AuthService.cs
// ============================================================
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
using System.Security.Cryptography;
using System.Text;

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

    // ──────────────────────────────────────────────────────────
    // LOGIN
    // Flow: validate input → find user → verify hashed password
    //       → generate access + refresh token → return AuthResponse
    // ──────────────────────────────────────────────────────────
    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginDto request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return ServiceResult<AuthResponse>.Fail("Username and password are required.");
        }

        var user = await _userServices.GetUserByUserNameOrEmailAsync(request.UserName);

        // FIX: Hash the input password before comparing — stored password is hashed
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            return ServiceResult<AuthResponse>.Fail("Invalid username or password.");

        var authResponse = await GenerateAuthResponseAsync(user);
        return ServiceResult<AuthResponse>.Success(authResponse, "Login successful.");
    }

    // ──────────────────────────────────────────────────────────
    // REGISTER
    // Flow: validate → check duplicate → hash password → create user
    //       → generate tokens → return AuthResponse
    // ──────────────────────────────────────────────────────────
    public async Task<ServiceResult<AuthResponse>> RegisterAsync(UserDto dto)
    {
        if (dto == null)
            return ServiceResult<AuthResponse>.Fail("Invalid request.");

        if (string.IsNullOrWhiteSpace(dto.Username) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return ServiceResult<AuthResponse>.Fail("Username, email and password are required.");
        }

        if (dto.Password.Length < 6)
            return ServiceResult<AuthResponse>.Fail("Password must be at least 6 characters.");

        if (await _userServices.UserExistsAsync(dto.Email, dto.PhoneNumber, dto.Id))
            return ServiceResult<AuthResponse>.Fail("Email or phone number already exists.");

        // Always hash password before saving to DB
        dto.Password = HashPassword(dto.Password);

        var user = await _userServices.CreateUserEntityAsync(dto);
        if (user == null)
            return ServiceResult<AuthResponse>.Fail("User creation failed.");

        var authResponse = await GenerateAuthResponseAsync(user);
        return ServiceResult<AuthResponse>.Success(authResponse, "Registration successful.");
    }

    // ──────────────────────────────────────────────────────────
    // REFRESH TOKEN
    // Flow: validate request → find user → check refresh token
    //       matches DB and not expired → generate new tokens
    // ──────────────────────────────────────────────────────────
    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(RefreshTokenDto request)
    {
        if (request == null || request.UserId <= 0 || string.IsNullOrWhiteSpace(request.RefreshToken))
            return ServiceResult<AuthResponse>.Fail("Invalid request.");

        var user = await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == request.UserId);

        // Validate: user exists, token matches, token not expired
        if (user == null ||
            user.RefreshToken != request.RefreshToken ||
            user.RefreshTokenExpiryTime == null ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ServiceResult<AuthResponse>.Fail("Invalid or expired refresh token.");
        }

        var authResponse = await GenerateAuthResponseAsync(user);
        return ServiceResult<AuthResponse>.Success(authResponse, "Token refreshed successfully.");
    }

    // ──────────────────────────────────────────────────────────
    // LOGOUT
    // Revokes the refresh token in DB so it cannot be reused
    // ──────────────────────────────────────────────────────────
    public async Task<ServiceResult<bool>> LogoutAsync(int userId)
    {
        if (userId <= 0)
            return ServiceResult<bool>.Fail("Invalid user id.");

        await _jwtService.RevokeRefreshTokenAsync(userId);
        return ServiceResult<bool>.Success(true, "Logged out successfully.");
    }

    // ──────────────────────────────────────────────────────────
    // PRIVATE HELPERS
    // ──────────────────────────────────────────────────────────

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Persist new refresh token to DB
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

    // Hash using SHA-256 — same algorithm used in Register and here in Login
    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    // Hash the input then compare to stored hash
    private static bool VerifyPassword(string inputPassword, string storedHashedPassword)
    {
        var inputHash = HashPassword(inputPassword);
        return inputHash == storedHashedPassword;
    }
}