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
        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> CreateUserAsync(UserDto dto)
    {
        var user = _mapper.Map<User>(dto);
        var created = await _userRepository.CreateUserAsync(user);
        return _mapper.Map<UserDto>(created);
    }

    public async Task<UserDto> UpdateUserAsync(UserDto dto)
    {
        var user = _mapper.Map<User>(dto);
        var updated = await _userRepository.UpdateUserAsync(user);
        return _mapper.Map<UserDto>(updated);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        return await _userRepository.DeleteUserAsync(id);
    }

    public async Task<bool> UserExistsAsync(string username, string email)
    {
        return await _userRepository.UserExistsAsync(username, email);
    }

    public async Task<User?> GetUserByUserNameAndEmail(string usernameOrEmail)
    {
        return await _userRepository.GetByUsernameOrEmailAsync(usernameOrEmail);
    }
}
