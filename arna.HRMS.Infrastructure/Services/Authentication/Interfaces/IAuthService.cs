using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.Common.Token;
using arna.HRMS.Core.DTOs;
using Microsoft.AspNetCore.Identity.Data;

namespace arna.HRMS.Infrastructure.Services.Authentication.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult<AuthResponse>> RegisterAsync(UserDto dto);
    Task<ServiceResult<AuthResponse>> RefreshTokenAsync(RefreshTokenDto request);
    Task<ServiceResult<bool>> LogoutAsync(int userId);
}
