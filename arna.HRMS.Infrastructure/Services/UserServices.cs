using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Repositories;
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

    public async Task<ServiceResult<List<UserDto>>> GetUsersAsync()
    {
        var users = await _repository.GetUsersAsync();
        var dtos = _mapper.Map<List<UserDto>>(users);
        var response = ServiceResult<List<UserDto>>.Success(dtos);
        return response;
    }

    public async Task<ServiceResult<UserDto?>> GetUserByIdAsync(int id)
    {
        if (id <= 0)
        {
            var failId = ServiceResult<UserDto?>.Fail("Invalid User ID");
            return failId;
        }

        var user = await _repository.GetUserByIdAsync(id);
        if (user == null)
        {
            var failNotFound = ServiceResult<UserDto?>.Fail("User not found");
            return failNotFound;
        }

        var mapped = _mapper.Map<UserDto>(user);
        var response = ServiceResult<UserDto?>.Success(mapped);
        return response;
    }

    public async Task<ServiceResult<UserDto?>> GetuserByEmployeeAsync(int empid)
    {
        if (empid <= 0)
        {
            var failId = ServiceResult<UserDto?>.Fail("Invalid employee ID.");
            return failId;
        }

        var emp = await _repository.GetUserByEmployeeIdAsync(empid);
        if (emp == null)
        {
            var failNotFound = ServiceResult<UserDto?>.Fail("Employee not found.");
            return failNotFound;
        }

        var mapped = _mapper.Map<UserDto>(emp);
        var response = ServiceResult<UserDto?>.Success(mapped);
        return response;
    }

    public async Task<ServiceResult<UserDto>> CreateUserAsync(UserDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<UserDto>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        if (dto.EmployeeId.HasValue && dto.EmployeeId.Value > 0)
        {
            var existingUserWithEmp = await _repository.GetUserByEmployeeIdAsync(dto.EmployeeId.Value);
            if (existingUserWithEmp != null)
            {
                existingUserWithEmp.EmployeeId = null;
                existingUserWithEmp.UpdatedOn = DateTime.Now;
                await _repository.UpdateUserAsync(existingUserWithEmp);
            }
        }

        var entity = _mapper.Map<User>(dto);
        var created = await _repository.CreateUserAsync(entity);
        var mapped = _mapper.Map<UserDto>(created);
        var response = ServiceResult<UserDto>.Success(mapped, "User created successfully.");
        return response;
    }

    public async Task<ServiceResult<UserDto>> UpdateUserAsync(UserDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<UserDto>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var existing = await _repository.GetUserByIdAsync(dto.Id);
        if (existing == null)
        {
            var failNotFound = ServiceResult<UserDto>.Fail("User not found.");
            return failNotFound;
        }

        if (dto.EmployeeId.HasValue && dto.EmployeeId.Value > 0)
        {
            var existingUserWithEmp = await _repository.GetUserByEmployeeIdAsync(dto.EmployeeId.Value);
            if (existingUserWithEmp != null && existingUserWithEmp.Id != dto.Id)
            {
                existingUserWithEmp.EmployeeId = null;
                existingUserWithEmp.UpdatedOn = DateTime.Now;
                await _repository.UpdateUserAsync(existingUserWithEmp);
            }
        }

        existing.FirstName = dto.FirstName;
        existing.LastName = dto.LastName;
        existing.Email = dto.Email;
        existing.PhoneNumber = dto.PhoneNumber;
        existing.Username = dto.Username;
        existing.RoleId = dto.RoleId;
        existing.EmployeeId = dto.EmployeeId;
        existing.IsActive = dto.IsActive;
        existing.UpdatedOn = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            existing.Password = dto.Password;
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(dto.Password);
            var hash = sha.ComputeHash(bytes);
            existing.PasswordHash = Convert.ToBase64String(hash);
        }

        var updated = await _repository.UpdateUserAsync(existing);
        var mapped = _mapper.Map<UserDto>(updated);
        var response = ServiceResult<UserDto>.Success(mapped, "User updated successfully.");
        return response;
    }

    public async Task<ServiceResult<bool>> DeleteUserAsync(int id)
    {
        if (id <= 0)
        {
            var failId = ServiceResult<bool>.Fail("Fail to Delete User");
            return failId;
        }

        var deleted = await _repository.DeleteUserAsync(id);
        if (deleted)
        {
            var success = ServiceResult<bool>.Success(true, "User deleted successfully");
            return success;
        }

        var failNotFound = ServiceResult<bool>.Fail("Fail to Delete User");
        return failNotFound;
    }

    public async Task<ServiceResult<bool>> ChangeUserPasswordAsync(int id, string newPassword)
    {
        var validation = await _validator.ValidateChangePasswordAsync(id, newPassword);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<bool>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var changed = await _repository.ChangeUserPasswordAsync(id, newPassword);
        if (changed)
        {
            var success = ServiceResult<bool>.Success(true, "Password updated successfully.");
            return success;
        }

        var failNotFound = ServiceResult<bool>.Fail("No Data Found");
        return failNotFound;
    }

    public async Task<ServiceResult<bool>> CheckUsernameExistsAsync(string username, int? excludeUserId = null)
    {
        var exists = await _repository.UsernameExistsAsync(username, excludeUserId);
        var response = ServiceResult<bool>.Success(exists);
        return response;
    }

    public async Task<bool> UserExistsAsync(string email, string phoneNumber, int? id)
    {
        return await _repository.UserExistsAsync(email, phoneNumber, id);
    }

    public async Task<User?> GetUserByUserNameOrEmailAsync(string userNameOrEmail)
    {
        return string.IsNullOrWhiteSpace(userNameOrEmail)
            ? null
            : await _repository.GetByUsernameOrEmailAsync(userNameOrEmail);
    }

    public async Task<User> CreateUserEntityAsync(UserDto dto)
    {
        return await _repository.CreateUserAsync(_mapper.Map<User>(dto));
    }
}
