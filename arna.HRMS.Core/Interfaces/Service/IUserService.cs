using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;

namespace arna.HRMS.Core.Interfaces.Service;

public interface IUserServices
{
    Task<ServiceResult<List<UserDto>>> GetUsersAsync();
    Task<ServiceResult<UserDto?>> GetUserByIdAsync(int id);
    Task<ServiceResult<UserDto>> CreateUserAsync(UserDto dto);
    Task<ServiceResult<UserDto>> UpdateUserAsync(UserDto dto);
    Task<ServiceResult<bool>> DeleteUserAsync(int id);
    Task<ServiceResult<bool>> ChangeUserPasswordAsync(int id, string newPassword);
    Task<bool> UserExistsAsync(string email, string phoneNumber, int? id);
    Task<User?> GetUserByUserNameOrEmailAsync(string userNameOrEmail);
    Task<User> CreateUserEntityAsync(UserDto dto);
}