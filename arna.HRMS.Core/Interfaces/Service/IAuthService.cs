using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.DTOs.Auth;

namespace arna.HRMS.Core.Interfaces.Service;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginDto request);
    Task<ServiceResult<AuthResponse>> RegisterAsync(UserDto dto);
    Task<ServiceResult<AuthResponse>> RefreshTokenAsync(RefreshTokenDto request);
    Task<ServiceResult<bool>> LogoutAsync(int userId);
}