using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Interfaces;

public interface IUserServices
{
    Task<List<UserDto>> GetUserAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(UserDto dto);
    Task<UserDto> UpdateUserAsync(UserDto dto);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> UserExistsAsync(string username, string email);
    Task<User?> GetUserByUserNameAndEmail(string userNameOrEmail);
    Task<bool> ChangeUserPasswordAsync(int id, string newPassword);

    // Auth Services
    Task<User> CreateUserEntityAsync(UserDto dto);
}
