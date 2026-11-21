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
        {
            var failVal = ServiceResult<EmployeeDto>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var roleResult = await _roleService.GetRoleByNameAsync(UserRole.Employee.ToString());
        if (roleResult == null || !roleResult.IsSuccess || roleResult.Data == null)
        {
            var failRole = ServiceResult<EmployeeDto>.Fail("Employee role not found");
            return failRole;
        }

        var employee = _mapper.Map<Employee>(dto);

        if (employee.ManagerId.HasValue && employee.ManagerId.Value <= 0)
        {
            employee.ManagerId = null;
        }

        var lastNumber = await _repository.GetLastEmployeeNumberAsync();
        var candidateNumber = GenerateEmployeeNumber(lastNumber);

        var nextNum = 1;
        if (!string.IsNullOrWhiteSpace(lastNumber))
        {
            var digits = new string(lastNumber.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out int cur)) nextNum = cur + 1;
        }

        while (await _repository.EmployeeNumberExistsAsync(candidateNumber))
        {
            nextNum++;
            candidateNumber = $"Emp{nextNum:D3}";
        }

        employee.EmployeeNumber = candidateNumber;

        var created = await _repository.CreateEmployeeAsync(employee);

        if (created == null)
        {
            var failCreate = ServiceResult<EmployeeDto>.Fail("Failed to create employee.");
            return failCreate;
        }

        try
        {
            await CreateUserForEmployeeAsync(created);
        }
        catch (Exception userEx)
        {
            Console.WriteLine($"[EmployeeService] Warning: CreateUserForEmployee failed: {userEx.Message}");
        }

        var response = ServiceResult<EmployeeDto>.Success(_mapper.Map<EmployeeDto>(created), "Employee created successfully.");
        return response;
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
        var userResult = await _userServices.GetuserByEmployeeAsync(id);
        if (userResult != null && userResult.IsSuccess && userResult.Data != null)
        {
            await _userServices.DeleteUserAsync(userResult.Data.Id);
        }

        if (deleted)
        {
            var successResult = ServiceResult<bool>.Success(true, "Employee deleted successfully.");
            return successResult;
        }

        var failResult = ServiceResult<bool>.Fail("Employee not found");
        return failResult;
    }

    public async Task<ServiceResult<string>> UploadProfileImageAsync(int employeeId, Stream fileStream, string fileName)
    {
        var validation = ValidateImageUploadInput(employeeId, fileStream, fileName);
        if (!validation.IsSuccess)
        {
            var failValidation = ServiceResult<string>.Fail(validation.Message);
            return failValidation;
        }

        var employee = await _repository.GetEmployeeByIdAsync(employeeId);
        if (employee == null)
        {
            var failEmp = ServiceResult<string>.Fail("Employee not found.");
            return failEmp;
        }

        var ext = Path.GetExtension(fileName)!.ToLowerInvariant();
        var folder = GetProfileFolder();
        var savedFileName = $"emp_{employeeId}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, savedFileName);

        DeleteExistingProfileImageFile(employee.ProfileImage, folder);
        await SaveImageToDiskAsync(fileStream, fullPath);

        var relativePath = $"/profile/{savedFileName}";
        employee.ProfileImage = relativePath;
        employee.UpdatedOn = DateTime.Now;

        await _repository.UpdateEmployeeAsync(employee);

        var result = ServiceResult<string>.Success(relativePath, "Profile image updated successfully.");
        return result;
    }

    private static ServiceResult<bool> ValidateImageUploadInput(int employeeId, Stream? fileStream, string? fileName)
    {
        if (employeeId <= 0)
            return ServiceResult<bool>.Fail("Invalid employee ID.");

        if (fileStream == null || fileStream.Length == 0)
            return ServiceResult<bool>.Fail("No image stream provided.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            return ServiceResult<bool>.Fail("Only JPG, JPEG, PNG, and WEBP image files are allowed.");

        return ServiceResult<bool>.Success(true);
    }

    private string GetProfileFolder()
    {
        var rootPath = _environment?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var folder = Path.Combine(rootPath, "profile");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        return folder;
    }

    private static void DeleteExistingProfileImageFile(string? imageRelativePath, string folder)
    {
        if (string.IsNullOrWhiteSpace(imageRelativePath))
            return;

        var oldFileName = Path.GetFileName(imageRelativePath);
        var oldFullPath = Path.Combine(folder, oldFileName);
        if (File.Exists(oldFullPath))
        {
            try { File.Delete(oldFullPath); } catch { }
        }
    }

    private static async Task SaveImageToDiskAsync(Stream fileStream, string targetPath)
    {
        using var outputStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(outputStream);
    }

    public async Task<ServiceResult<bool>> DeleteProfileImageAsync(int employeeId)
    {
        if (employeeId <= 0)
        {
            var failId = ServiceResult<bool>.Fail("Invalid employee ID.");
            return failId;
        }

        var employee = await _repository.GetEmployeeByIdAsync(employeeId);
        if (employee == null)
        {
            var failEmp = ServiceResult<bool>.Fail("Employee not found.");
            return failEmp;
        }

        if (!string.IsNullOrWhiteSpace(employee.ProfileImage))
        {
            var rootPath = _environment?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(rootPath, "profile");
            var oldFileName = Path.GetFileName(employee.ProfileImage);
            var oldFullPath = Path.Combine(folder, oldFileName);
            if (File.Exists(oldFullPath))
            {
                try { File.Delete(oldFullPath); } catch { }
            }

            employee.ProfileImage = null;
            employee.UpdatedOn = DateTime.Now;
            await _repository.UpdateEmployeeAsync(employee);
        }

        var result = ServiceResult<bool>.Success(true, "Profile image removed successfully.");
        return result;
    }

    private async Task CreateUserForEmployeeAsync(Employee employee)
    {
        var roleResult = await _roleService.GetRoleByNameAsync(UserRole.Employee.ToString());

        if (roleResult == null || !roleResult.IsSuccess || roleResult.Data == null)
            return;

        var password = GenerateRandomPassword();
        var username = await GenerateUniqueUsernameAsync(employee.FirstName, employee.LastName);

        var userDto = new UserDto
        {
            Username = username,
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

        var username = existingUser.Username;
        if (string.IsNullOrWhiteSpace(username) || username.Contains('@'))
        {
            username = await GenerateUniqueUsernameAsync(employee.FirstName, employee.LastName, existingUser.Id);
        }

        var userDto = new UserDto
        {
            Id = existingUser.Id, 
            Username = username,
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

    private async Task<string> GenerateUniqueUsernameAsync(string? firstName, string? lastName, int? excludeUserId = null)
    {
        var first = new string((firstName ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLower();
        var last = new string((lastName ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLower();

        var baseUsername = string.IsNullOrWhiteSpace(last)
            ? (string.IsNullOrWhiteSpace(first) ? "user" : first)
            : $"{first}.{last}";

        var candidate = baseUsername;
        int counter = 1;

        while (true)
        {
            var existsResult = await _userServices.CheckUsernameExistsAsync(candidate, excludeUserId);
            if (existsResult == null || !existsResult.IsSuccess || !existsResult.Data)
            {
                break;
            }

            candidate = $"{baseUsername}{counter}";
            counter++;
        }

        return candidate;
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