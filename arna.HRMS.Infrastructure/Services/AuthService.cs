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
        try
        {
            // Find user by username
            var user = await _userServices.GetUserByUserNameAndEmail(request.Email);

            if (user == null)
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Invalid username or password"
                };
            }

            // Verify password (assuming PasswordHash contains hashed password)
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Invalid username or password"
                };
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken);

            return new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddDays(8),
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Password = user.Password,
                Role = user.Role,
                IsSuccess = true,
                Message = "Login successful"
            };
        }
        catch (Exception)
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "An error occurred during login"
            };
        }
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
                CreatedAt = DateTime.UtcNow
            };

            await _userServices.CreateUserAsync(user);

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken);

            return new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddDays(8),
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                IsSuccess = true,
                Message = "Registration successful"
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
