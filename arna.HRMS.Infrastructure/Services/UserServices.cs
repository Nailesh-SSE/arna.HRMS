using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Models.DTOs;
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

    public async Task<List<UserDto>> GetUserAsync()
    {
        var users = await _userRepository.GetUserAsync();
        return users.Select(e => _mapper.Map<UserDto>(e)).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null) return null;
        var userDto = _mapper.Map<UserDto>(user);
        return userDto;
    }

    public async Task<UserDto> CreateUserAsync(User user)
    {
        var createdUser = await _userRepository.CreateUserAsync(user);
        return _mapper.Map<UserDto>(createdUser);
    }

    public async Task<UserDto> UpdateUserAsync(User user)
    {
        var updatedUser = await _userRepository.UpdateUserAsync(user);
        return _mapper.Map<UserDto>(updatedUser);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var userDelete = await _userRepository.DeleteUserAsync(id);
        return userDelete;
    }

    public async Task<bool> UserExistsAsync(string userName, string email)
    {
        var users = await _userRepository.GetUserAsync();
        return users.Any(u => u.Username == userName || u.Email == email);
    }

    public async Task<User?> GetUserByUserNameAndEmail(string userNameOrEmail)
    {
        var users = await _userRepository.GetUserAsync();
        var user = users.FirstOrDefault(x => x.Username.ToLower() == userNameOrEmail.ToLower() || x.Email.ToLower() == userNameOrEmail.ToLower()) ?? null; 
        if (user == null) return null;
        return user;
    }
}
