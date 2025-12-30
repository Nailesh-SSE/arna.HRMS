using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity.Data;
using RegisterRequest = arna.HRMS.Core.DTOs.Requests.RegisterRequest;

namespace arna.HRMS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;
    private readonly IUserServices _userServices; 

    public AuthService(IJwtService jwtService, IUserServices userServices)
    {
        _jwtService = jwtService;
        _userServices = userServices;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userServices.GetUserByUserNameAndEmail(request.Email);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "Invalid credentials"
            };
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken);

        return new AuthResponse
        {
            IsSuccess = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(2),
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            Password = user.Password,
            Email = user.Username,
            EmployeeId = user.EmployeeId,
            Message = "Login successful"
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Check if user already exists
            if (await _userServices.UserExistsAsync(request.Username, request.Email))
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Username or email already exists"
                };
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmployeeId = request.EmployeeId
            };

            await _userServices.CreateUserAsync(user);

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken);

            return new AuthResponse
            {
                IsSuccess = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(2),
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                Password = user.Password,
                Email = user.Username,
                EmployeeId = user.EmployeeId,
                Message = "Login successful"
            };
        }
        catch (Exception)
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "An error occurred during registration"
            };
        }
    }

    public async Task LogoutAsync(int userId)
    {
        await _jwtService.RevokeRefreshTokenAsync(userId);
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        return HashPassword(password) == passwordHash;
    }

    private string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
