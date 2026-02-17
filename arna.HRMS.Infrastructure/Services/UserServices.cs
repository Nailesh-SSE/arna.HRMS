using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class UserServices : IUserServices
{
    private readonly UserRepository _repository;
    private readonly IMapper _mapper;
    private readonly UserValidator _validator;

    public UserServices(
        UserRepository repository,
        IMapper mapper,
        UserValidator validator)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<ServiceResult<List<UserDto>>> GetUserAsync()
    {
        var users = await _repository.GetUserAsync();
        return ServiceResult<List<UserDto>>.Success(_mapper.Map<List<UserDto>>(users));
    }

    public async Task<ServiceResult<UserDto?>> GetUserByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<UserDto?>.Fail("Invalid User ID");

        var user = await _repository.GetUserByIdAsync(id);

        if (user == null)
            return ServiceResult<UserDto?>.Fail("User not found");

        return ServiceResult<UserDto?>.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<ServiceResult<UserDto>> CreateUserAsync(UserDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<UserDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var entity = _mapper.Map<User>(dto);
        var created = await _repository.CreateUserAsync(entity);

        return ServiceResult<UserDto>.Success(_mapper.Map<UserDto>(created), "User created successfully");
    }

    public async Task<ServiceResult<UserDto>> UpdateUserAsync(UserDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<UserDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var updated = await _repository.UpdateUserAsync(_mapper.Map<User>(dto));

        return ServiceResult<UserDto>.Success(_mapper.Map<UserDto>(updated), "User updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteUserAsync(int id)
    {
        var deleted = await _repository.DeleteUserAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(deleted, "User deleted successfully")
            : ServiceResult<bool>.Fail("Fail to Delete User");
    }

    public async Task<ServiceResult<bool>> ChangeUserPasswordAsync(int id, string newPassword)
    {
        var validation = _validator.ValidateChangePasswordAsync(id, newPassword);
        if (!validation.IsValid)
            return ServiceResult<bool>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var changed = await _repository.ChangeUserPasswordAsync(id, newPassword);

        return ServiceResult<bool>.Success(changed, "Password updated successfully");
    }

    // Auth helpers remain unchanged
    public async Task<bool> UserExistsAsync(string email, string phoneNumber, int? Id)
        => await _repository.UserExistsAsync(email, phoneNumber, Id);

    public async Task<User?> GetUserByUserNameAndEmail(string userNameOrEmail)
        => string.IsNullOrWhiteSpace(userNameOrEmail) 
        ? null : await _repository.GetByUsernameOrEmailAsync(userNameOrEmail);

    public async Task<User> CreateUserEntityAsync(UserDto dto)
        => await _repository.CreateUserAsync(_mapper.Map<User>(dto));
}
