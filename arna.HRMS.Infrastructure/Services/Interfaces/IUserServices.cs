using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;

namespace arna.HRMS.Infrastructure.Services.Interfaces;

public interface IUserServices
{
    Task<ServiceResult<List<UserDto>>> GetUserAsync();
    Task<ServiceResult<UserDto?>> GetUserByIdAsync(int id);
    Task<ServiceResult<UserDto>> CreateUserAsync(UserDto dto);
    Task<ServiceResult<UserDto>> UpdateUserAsync(UserDto dto);
    Task<ServiceResult<bool>> DeleteUserAsync(int id);
    Task<ServiceResult<bool>> ChangeUserPasswordAsync(int id, string newPassword);

    // Auth Services
    Task<bool> UserExistsAsync(string email, string phoneNumber);
    Task<User?> GetUserByUserNameAndEmail(string userNameOrEmail);
    Task<User> CreateUserEntityAsync(UserDto dto);
}
