using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;

namespace arna.HRMS.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly EmployeeRepository _repository;
    private readonly IUserServices _userServices;
    private readonly IRoleService _roleService;
    private readonly IMapper _mapper;
    private readonly EmployeeValidator _validator;
    private readonly IWebHostEnvironment? _environment;

    public EmployeeService(
        EmployeeRepository repository,
        IUserServices userServices,
        IRoleService roleService,
        IMapper mapper,
        EmployeeValidator validator,
        IWebHostEnvironment? environment = null)
    {
        _repository = repository;
        _userServices = userServices;
        _roleService = roleService;
        _mapper = mapper;
        _validator = validator;
        _environment = environment;
    }

    public async Task<ServiceResult<List<EmployeeDto>>> GetEmployeesAsync()
    {
        var employees = await _repository.GetEmployeesAsync();

        return ServiceResult<List<EmployeeDto>>.Success(_mapper.Map<List<EmployeeDto>>(employees));
    }

    public async Task<ServiceResult<EmployeeDto?>> GetEmployeeByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<EmployeeDto?>.Fail("Invalid Employee ID");

        var employee = await _repository.GetEmployeeByIdAsync(id);

        if (employee == null)
            return ServiceResult<EmployeeDto?>.Fail("Employee not found");

        return ServiceResult<EmployeeDto?>.Success(_mapper.Map<EmployeeDto>(employee));
    }

    public async Task<ServiceResult<EmployeeDto>> CreateEmployeeAsync(EmployeeDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);

        if (!validation.IsValid)
            return ServiceResult<EmployeeDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var roleResult = await _roleService.GetRoleByNameAsync(UserRole.Employee.ToString());
        if (roleResult == null || !roleResult.IsSuccess || roleResult.Data == null)
            return ServiceResult<EmployeeDto>.Fail("Employee role not found");

        var employee = _mapper.Map<Employee>(dto);

        var lastNumber = await _repository.GetLastEmployeeNumberAsync();
        employee.EmployeeNumber = GenerateEmployeeNumber(lastNumber);

        var created = await _repository.CreateEmployeeAsync(employee);

        if (created == null)
            return ServiceResult<EmployeeDto>.Fail("Failed to create employee.");

        await CreateUserForEmployeeAsync(created);

        return ServiceResult<EmployeeDto>.Success(_mapper.Map<EmployeeDto>(created), "Employee created successfully.");
    }

    public async Task<ServiceResult<EmployeeDto>> UpdateEmployeeAsync(EmployeeDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);

        if (!validation.IsValid)
            return ServiceResult<EmployeeDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var existing = await _repository.GetEmployeeByIdAsync(dto.Id);
        if (existing == null)
            return ServiceResult<EmployeeDto>.Fail("Employee not found.");

        var entity = _mapper.Map<Employee>(dto);
        if (string.IsNullOrWhiteSpace(entity.EmployeeNumber))
            entity.EmployeeNumber = existing.EmployeeNumber;

        var updated = await _repository.UpdateEmployeeAsync(entity);

        if (updated == null)
            return ServiceResult<EmployeeDto>.Fail("Failed to update employee.");

        await UpdateUserForEmployeeAsync(updated);

        return ServiceResult<EmployeeDto>.Success(_mapper.Map<EmployeeDto>(updated), "Employee updated successfully.");
    }

    public async Task<ServiceResult<bool>> DeleteEmployeeAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Employee not found");

        var deleted = await _repository.DeleteEmployeeAsync(id);
        // Bug 6 FIX: Added await (was missing) and null-safety check
        var userResult = await _userServices.GetuserByEmployeeAsync(id);
        if (userResult != null && userResult.IsSuccess && userResult.Data != null)
            await _userServices.DeleteUserAsync(userResult.Data.Id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Employee deleted successfully.")
            : ServiceResult<bool>.Fail("Employee not found");
    }

    public async Task<ServiceResult<string>> UploadProfileImageAsync(int employeeId, Stream fileStream, string fileName)
    {
        if (employeeId <= 0)
            return ServiceResult<string>.Fail("Invalid employee ID.");

        if (fileStream == null || fileStream.Length == 0)
            return ServiceResult<string>.Fail("No image stream provided.");

        var employee = await _repository.GetEmployeeByIdAsync(employeeId);
        if (employee == null)
            return ServiceResult<string>.Fail("Employee not found.");

        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            return ServiceResult<string>.Fail("Only JPG, JPEG, PNG, and WEBP image files are allowed.");

        // Resolve root folder: wwwroot/profile-images
        string rootPath;
        if (_environment != null && !string.IsNullOrEmpty(_environment.WebRootPath))
        {
            rootPath = _environment.WebRootPath;
        }
        else
        {
            rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var folderPath = Path.Combine(rootPath, "profile-images");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Generate unique safe file name
        var uniqueFileName = $"emp_{employeeId}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folderPath, uniqueFileName);

        // Delete previous image file if exists
        if (!string.IsNullOrWhiteSpace(employee.ProfileImage))
        {
            var oldFileName = Path.GetFileName(employee.ProfileImage);
            var oldFullPath = Path.Combine(folderPath, oldFileName);
            if (File.Exists(oldFullPath))
            {
                try { File.Delete(oldFullPath); } catch { /* Ignore deletion errors */ }
            }
        }

        // Save new file to disk
        using (var outputStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }
            await fileStream.CopyToAsync(outputStream);
        }

        // Update database entity with relative URL
        var relativeUrl = $"/profile-images/{uniqueFileName}";
        employee.ProfileImage = relativeUrl;
        employee.UpdatedOn = DateTime.Now;

        await _repository.UpdateEmployeeAsync(employee);

        return ServiceResult<string>.Success(relativeUrl, "Profile image updated successfully.");
    }

    public async Task<ServiceResult<bool>> DeleteProfileImageAsync(int employeeId)
    {
        if (employeeId <= 0)
            return ServiceResult<bool>.Fail("Invalid employee ID.");

        var employee = await _repository.GetEmployeeByIdAsync(employeeId);
        if (employee == null)
            return ServiceResult<bool>.Fail("Employee not found.");

        if (!string.IsNullOrWhiteSpace(employee.ProfileImage))
        {
            string rootPath = _environment?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(rootPath, "profile-images");
            var oldFileName = Path.GetFileName(employee.ProfileImage);
            var oldFullPath = Path.Combine(folderPath, oldFileName);
            if (File.Exists(oldFullPath))
            {
                try { File.Delete(oldFullPath); } catch { }
            }

            employee.ProfileImage = null;
            employee.UpdatedOn = DateTime.Now;
            await _repository.UpdateEmployeeAsync(employee);
        }

        return ServiceResult<bool>.Success(true, "Profile image removed successfully.");
    }

    private async Task CreateUserForEmployeeAsync(Employee employee)
    {
        var roleResult = await _roleService.GetRoleByNameAsync(UserRole.Employee.ToString());

        if (roleResult == null || !roleResult.IsSuccess || roleResult.Data == null)
            return;

        var password = GenerateRandomPassword();

        var userDto = new UserDto
        {
            Username = employee.OfficeEmail,
            Email = employee.Email,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            RoleId = roleResult.Data.Id,
            PhoneNumber = employee.PhoneNumber,
            EmployeeId = employee.Id,
            Password = password
        };

        await _userServices.CreateUserAsync(userDto);
    }

    private async Task UpdateUserForEmployeeAsync(Employee employee)
    {
        var existingUserResult = await _userServices.GetuserByEmployeeAsync(employee.Id);

        if (existingUserResult == null || !existingUserResult.IsSuccess || existingUserResult.Data == null)
            return;

        var existingUser = existingUserResult.Data;

        var userDto = new UserDto
        {
            Id = existingUser.Id, 
            Username = employee.OfficeEmail ?? existingUser.Username,
            Email = employee.Email ?? existingUser.Email,
            FirstName = employee.FirstName ?? existingUser.FirstName,
            LastName = employee.LastName ?? existingUser.LastName,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            RoleId = existingUser.RoleId > 0 ? existingUser.RoleId : (await _roleService.GetRoleByNameAsync(UserRole.Employee.ToString()))?.Data?.Id ?? 4,
            PhoneNumber = employee.PhoneNumber ?? existingUser.PhoneNumber,
            EmployeeId = employee.Id,
            IsActive = employee.IsActive,
            Password = existingUser.Password
        };

        var updateResult = await _userServices.UpdateUserAsync(userDto);

        if (!updateResult.IsSuccess)
        {
            throw new Exception($"User update failed: {updateResult.Message}");
        }
    }

    private static string GenerateEmployeeNumber(string? lastNumber)
    {
        int next = 1;

        if (!string.IsNullOrWhiteSpace(lastNumber) && lastNumber.StartsWith("Emp", StringComparison.OrdinalIgnoreCase))
        {
            var numericPart = new string(lastNumber.Where(char.IsDigit).ToArray());

            if (int.TryParse(numericPart, out int current))
                next = current + 1;
        }

        return $"Emp{next:D3}";
    }

    private static string GenerateRandomPassword()
        => Guid.NewGuid().ToString("N")[..8];
}