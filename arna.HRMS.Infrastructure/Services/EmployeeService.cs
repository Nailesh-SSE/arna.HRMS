using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly EmployeeRepository _repository;
    private readonly IUserServices _userServices;
    private readonly IRoleService _roleService;
    private readonly IMapper _mapper;
    private readonly EmployeeValidator _validator;

    public EmployeeService(
        EmployeeRepository repository,
        IUserServices userServices,
        IRoleService roleService,
        IMapper mapper,
        EmployeeValidator validator)
    {
        _repository = repository;
        _userServices = userServices;
        _roleService = roleService;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<ServiceResult<List<EmployeeDto>>> GetEmployeesAsync()
    {
        var employees = await _repository.GetEmployeesAsync();

        return ServiceResult<List<EmployeeDto>>.Success(_mapper.Map<List<EmployeeDto>>(employees));
    }

    public async Task<ServiceResult<EmployeeDto?>> GetEmployeeByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<EmployeeDto?>.Fail("Invalid employee ID.");

        var employee = await _repository.GetEmployeeByIdAsync(id);

        if (employee == null)
            return ServiceResult<EmployeeDto?>.Fail("Employee not found.");

        return ServiceResult<EmployeeDto?>.Success(_mapper.Map<EmployeeDto>(employee));
    }

    public async Task<ServiceResult<EmployeeDto>> CreateEmployeeAsync(EmployeeDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);

        if (!validation.IsValid)
            return ServiceResult<EmployeeDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

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

        var entity = _mapper.Map<Employee>(dto);

        var updated = await _repository.UpdateEmployeeAsync(entity);

        return ServiceResult<EmployeeDto>.Success(_mapper.Map<EmployeeDto>(updated), "Employee updated successfully.");
    }

    public async Task<ServiceResult<bool>> DeleteEmployeeAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid employee ID.");

        var deleted = await _repository.DeleteEmployeeAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Employee deleted successfully.")
            : ServiceResult<bool>.Fail("Employee not found.");
    }

    private async Task CreateUserForEmployeeAsync(Employee employee)
    {
        var roleResult = await _roleService.GetRoleByNameAsync(UserRole.Employee.ToString());

        if (!roleResult.IsSuccess || roleResult.Data == null)
            return;

        var password = GenerateRandomPassword();

        var userDto = new UserDto
        {
            Username = employee.FirstName,
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

    private static string GenerateEmployeeNumber(string? lastNumber)
    {
        int next = 1;

        if (!string.IsNullOrWhiteSpace(lastNumber))
        {
            var numericPart = new string(lastNumber.Where(char.IsDigit).ToArray());

            if (int.TryParse(numericPart, out int current))
                next = current + 1;
        }

        return $"EMP{next:D4}";
    }

    private static string GenerateRandomPassword()
        => Guid.NewGuid().ToString("N")[..8];
}