using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;

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
    Task<bool> UserExistsAsync(string email);
    Task<User?> GetUserByUserNameAndEmail(string userNameOrEmail);
    Task<User> CreateUserEntityAsync(UserDto dto);
}
