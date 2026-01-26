using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class UserServices : IUserServices
{
    private readonly UserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserServices(UserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResult<List<UserDto>>> GetUserAsync()
    {
        var users = await _userRepository.GetUserAsync();
        var usersList = _mapper.Map<List<UserDto>>(users);
        return ServiceResult<List<UserDto>>.Success(usersList);
    }

    public async Task<ServiceResult<UserDto?>> GetUserByIdAsync(int id)
    {
        if(id <= 0)
            return ServiceResult<UserDto?>.Fail("Invalid User ID");

        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
            return ServiceResult<UserDto?>.Fail("User not found");

        var userDto = _mapper.Map<UserDto>(user);
        return ServiceResult<UserDto?>.Success(userDto);
    }

    public async Task<ServiceResult<UserDto>> CreateUserAsync(UserDto dto)
    {
        if (dto == null)
            return ServiceResult<UserDto>.Fail("Invalid request");

        if (string.IsNullOrWhiteSpace(dto.Username))
            return ServiceResult<UserDto>.Fail("Username is required");

        if (string.IsNullOrWhiteSpace(dto.Email))
            return ServiceResult<UserDto>.Fail("Email is required");

        if (string.IsNullOrWhiteSpace(dto.Password))
            return ServiceResult<UserDto>.Fail("Password is required");

        if (dto.Password.Length < 6)
            return ServiceResult<UserDto>.Fail("Password must be at least 6 characters");

        if (await _userRepository.UserExistsAsync(dto.Email))
            return ServiceResult<UserDto>.Fail("Email already exists");

        var user = _mapper.Map<User>(dto);
        var created = await _userRepository.CreateUserAsync(user);
        var resultDto = _mapper.Map<UserDto>(created);

        return ServiceResult<UserDto>.Success(resultDto, "User created successfully");
    }

    public async Task<ServiceResult<UserDto>> UpdateUserAsync(UserDto dto)
    {
        if (dto == null)
            return ServiceResult<UserDto>.Fail("Invalid request");

        if (dto.Id <= 0)
            return ServiceResult<UserDto>.Fail("Invalid User ID");

        var user = _mapper.Map<User>(dto);
        var updated = await _userRepository.UpdateUserAsync(user);
        var resultDto = _mapper.Map<UserDto>(updated);

        return ServiceResult<UserDto>.Success(resultDto, "User updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteUserAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid User ID");

        var deleted = await _userRepository.DeleteUserAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "User deleted successfully")
            : ServiceResult<bool>.Fail("User not found");
    }

    public async Task<ServiceResult<bool>> ChangeUserPasswordAsync(int id, string newPassword)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid User ID");

        if (string.IsNullOrWhiteSpace(newPassword))
            return ServiceResult<bool>.Fail("NewPassword is required");

        if (newPassword.Length < 6)
            return ServiceResult<bool>.Fail("Password must be at least 6 characters");

        var changed = await _userRepository.ChangeUserPasswordAsync(id, newPassword);

        return changed
            ? ServiceResult<bool>.Success(true, "Password updated successfully")
            : ServiceResult<bool>.Fail("User not found");
    }

    // Auth Service related method

    public async Task<bool> UserExistsAsync(string email)
    {
        var exists = await _userRepository.UserExistsAsync(email);
        if(!exists)
            return false;

        return true;
    }

    public async Task<User?> GetUserByUserNameAndEmail(string userNameOrEmail)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail))
            return null;

        var user = await _userRepository.GetByUsernameOrEmailAsync(userNameOrEmail);
        if (user == null)
            return null;

        return user;
    }

    public async Task<User> CreateUserEntityAsync(UserDto dto)
    {
        var user = _mapper.Map<User>(dto);
        var createdUser = await _userRepository.CreateUserAsync(user);
        if (createdUser == null)
            return null;
        return createdUser;
    }
}
