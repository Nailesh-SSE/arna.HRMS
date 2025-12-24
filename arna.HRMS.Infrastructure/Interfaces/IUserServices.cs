using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Interfaces;

public interface IUserServices
{
    Task<List<UserDto>> GetUserAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(User user);
    Task<UserDto> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> UserExistsAsync(string userName, string email);
    Task<User?> GetUserByUserNameAndEmail(string userNameOrEmail); 
}
