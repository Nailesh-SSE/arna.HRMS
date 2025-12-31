using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Models.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Identity.Data;
using RegisterRequest = arna.HRMS.Core.DTOs.Requests.RegisterRequest;

namespace arna.HRMS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;
    private readonly IUserServices _userServices;
    private readonly IMapper _mapper;

    public AuthService(IJwtService jwtService, IUserServices userServices, IMapper mapper)
    {
        _jwtService = jwtService;
        _userServices = userServices;
        _mapper = mapper;
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

        var userDto = _mapper.Map<UserDto>(user);
        return await GenerateAuthResponseAsync(userDto, "Login successful");
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userServices.UserExistsAsync(request.Username, request.Email))
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "Invalid credentials"
            };
        }

        var model = _mapper.Map<UserDto>(request);
        var userDto =  await _userServices.CreateUserAsync(model); 
        return await GenerateAuthResponseAsync(userDto, "Registration successful");
    }

    public async Task LogoutAsync(int userId)
    {
        await _jwtService.RevokeRefreshTokenAsync(userId);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(UserDto userDto, string message)
    {
        var user = _mapper.Map<User>(userDto);
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken);

        return new AuthResponse
        {
            IsSuccess = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(10),
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            Email = user.Email,
            EmployeeId = user.EmployeeId,
            Message = message
        };
    }

    private static bool VerifyPassword(string password, string hash)
        => HashPassword(password) == hash;

    private static string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }
}