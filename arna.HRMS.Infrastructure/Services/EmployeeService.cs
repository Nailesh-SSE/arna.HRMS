using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Models.DTOs;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly EmployeeRepository _employeeRepository;
    private readonly IUserServices _userServices;
    private readonly IMapper _mapper;

    public EmployeeService(
        EmployeeRepository employeeRepository,
        IMapper mapper,
        IUserServices userServices)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
        _userServices = userServices;
    }

    public async Task<ServiceResult<List<EmployeeDto>>> GetEmployeesAsync()
    {
        var employees = await _employeeRepository.GetEmployeesAsync();
        var employeesList = _mapper.Map<List<EmployeeDto>>(employees);

        return ServiceResult<List<EmployeeDto>>.Success(employeesList);
    }

    public async Task<ServiceResult<EmployeeDto?>> GetEmployeeByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<EmployeeDto?>.Fail("Invalid Employee ID");

        var employee = await _employeeRepository.GetEmployeeByIdAsync(id);

        if (employee == null)
            return ServiceResult<EmployeeDto?>.Fail("Employee not found");

        var employeeDto = _mapper.Map<EmployeeDto>(employee);
        return ServiceResult<EmployeeDto?>.Success(employeeDto);
    }

    public async Task<ServiceResult<EmployeeDto>> CreateEmployeeAsync(EmployeeDto dto)
    {
        if (dto == null)
            return ServiceResult<EmployeeDto>.Fail("Invalid request");

        if (string.IsNullOrWhiteSpace(dto.FirstName))
            return ServiceResult<EmployeeDto>.Fail("FirstName is required");

        if (string.IsNullOrWhiteSpace(dto.LastName))
            return ServiceResult<EmployeeDto>.Fail("LastName is required");

        if (string.IsNullOrWhiteSpace(dto.Email))
            return ServiceResult<EmployeeDto>.Fail("Email is required");

        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return ServiceResult<EmployeeDto>.Fail("PhoneNumber is required");

        if (await _employeeRepository.EmployeeExistsAsync(dto.Email, dto.PhoneNumber))
            return ServiceResult<EmployeeDto>.Fail("Email or Phone Number already exists");

        var employee = _mapper.Map<Employee>(dto);

        var lastEmployeeNumber = await _employeeRepository.GetLastEmployeeNumberAsync();
        employee.EmployeeNumber = GenerateEmployeeNumber(lastEmployeeNumber);

        var createdEmployee = await _employeeRepository.CreateEmployeeAsync(employee);

        if (createdEmployee != null)
        {
            var userDto = new UserDto
            {
                Username = createdEmployee.FirstName,
                Email = createdEmployee.Email,
                FirstName = createdEmployee.FirstName,
                LastName = createdEmployee.LastName,
                EmployeeName = createdEmployee.FirstName + " " + createdEmployee.LastName,
                Role = UserRole.Employee,
                PhoneNumber = createdEmployee.PhoneNumber,
                EmployeeId = createdEmployee.Id,
                Password = $"{Guid.NewGuid().ToString("N")[..6]}"
            };

            await _userServices.CreateUserAsync(userDto);
        }

        var resultDto = _mapper.Map<EmployeeDto>(createdEmployee);

        return ServiceResult<EmployeeDto>.Success(resultDto, "Employee created successfully");
    }

    public async Task<ServiceResult<EmployeeDto>> UpdateEmployeeAsync(EmployeeDto dto)
    {
        if (dto == null)
            return ServiceResult<EmployeeDto>.Fail("Invalid request");

        if (dto.Id <= 0)
            return ServiceResult<EmployeeDto>.Fail("Invalid Employee ID");

        var employee = _mapper.Map<Employee>(dto);
        var updated = await _employeeRepository.UpdateEmployeeAsync(employee);
        var resultDto = _mapper.Map<EmployeeDto>(updated);

        return ServiceResult<EmployeeDto>.Success(resultDto, "Employee updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteEmployeeAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid Employee ID");

        var deleted = await _employeeRepository.DeleteEmployeeAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Employee deleted successfully")
            : ServiceResult<bool>.Fail("Employee not found");
    }

    public async Task<ServiceResult<bool>> EmployeeExistsAsync(string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phoneNumber))
            return ServiceResult<bool>.Fail("Email or PhoneNumber is required");

        var exists = await _employeeRepository.EmployeeExistsAsync(email, phoneNumber);
        if (!exists)
            return ServiceResult<bool>.Fail("Employee does not exist");

        return ServiceResult<bool>.Success(exists);
    }

    private static string GenerateEmployeeNumber(string? lastEmployeeNumber)
    {
        int next = 1;

        if (!string.IsNullOrWhiteSpace(lastEmployeeNumber))
        {
            var numberPart = lastEmployeeNumber
                .Replace("EMP", "", StringComparison.OrdinalIgnoreCase);

            if (int.TryParse(numberPart, out int current))
                next = current + 1;
        }

        return $"Emp{next:D3}";
    }
}
