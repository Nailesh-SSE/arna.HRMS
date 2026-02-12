using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly EmployeeRepository _employeeRepository;
    private readonly IUserServices _userServices;
    private readonly IMapper _mapper;
    private readonly IRoleService _roleService;
    private readonly EmployeeValidator _validator;

    public EmployeeService(
        EmployeeRepository employeeRepository,
        IMapper mapper,
        IUserServices userServices,
        IRoleService roleService,
        EmployeeValidator validator)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
        _userServices = userServices;
        _roleService = roleService;
        _validator = validator;
    }

    public async Task<ServiceResult<List<EmployeeDto>>> GetEmployeesAsync()
    {
        var employees = await _employeeRepository.GetEmployeesAsync();
        return ServiceResult<List<EmployeeDto>>.Success(_mapper.Map<List<EmployeeDto>>(employees));
    }

    public async Task<ServiceResult<EmployeeDto?>> GetEmployeeByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<EmployeeDto?>.Fail("Invalid Employee ID");

        var employee = await _employeeRepository.GetEmployeeByIdAsync(id);

        if (employee == null)
            return ServiceResult<EmployeeDto?>.Fail("Employee not found");

        var dto = _mapper.Map<EmployeeDto>(employee);
        return dto != null
            ? ServiceResult<EmployeeDto?>.Success(dto)
            : ServiceResult<EmployeeDto?>.Fail("Fail to Find Employee");
    }

    public async Task<ServiceResult<EmployeeDto>> CreateEmployeeAsync(EmployeeDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<EmployeeDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var employee = _mapper.Map<Employee>(dto);

        var lastEmployeeNumber = await _employeeRepository.GetLastEmployeeNumberAsync();
        employee.EmployeeNumber = GenerateEmployeeNumber(lastEmployeeNumber);

        var createdEmployee = await _employeeRepository.CreateEmployeeAsync(employee);

        if (createdEmployee != null)
        {
            var role = await _roleService.GetRoleByNameAsync(UserRole.Employee.ToString());
            if (role == null || !role.IsSuccess || role.Data == null)
                return ServiceResult<EmployeeDto>.Fail("Employee role not found");

            var userDto = new UserDto
            {
                Username = createdEmployee.FirstName,
                Email = createdEmployee.Email,
                FirstName = createdEmployee.FirstName,
                LastName = createdEmployee.LastName,
                EmployeeName = createdEmployee.FirstName + " " + createdEmployee.LastName,
                RoleId = role.Data.Id,
                PhoneNumber = createdEmployee.PhoneNumber,
                EmployeeId = createdEmployee.Id,
                Password = $"{Guid.NewGuid().ToString("N")[..6]}"
            };

            await _userServices.CreateUserAsync(userDto);
        }
        var Data = _mapper.Map<EmployeeDto>(createdEmployee);
        return Data != null
            ? ServiceResult<EmployeeDto>.Success(Data, "Employee created successfully")
            : ServiceResult<EmployeeDto>.Fail("Fail to create employee") ;
    }

    public async Task<ServiceResult<EmployeeDto>> UpdateEmployeeAsync(EmployeeDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<EmployeeDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var employee = _mapper.Map<Employee>(dto);
        var updated = await _employeeRepository.UpdateEmployeeAsync(employee);
        var Data = _mapper.Map<EmployeeDto>(updated);
        return Data!=null 
            ? ServiceResult<EmployeeDto>.Success(Data, "Employee updated successfully")
            : ServiceResult<EmployeeDto>.Fail("Fail to Update employee");
    }

    public async Task<ServiceResult<bool>> DeleteEmployeeAsync(int id)
    {
        var employee = await GetEmployeeByIdAsync(id);
        if (!employee.IsSuccess || employee.Data == null)
            return ServiceResult<bool>.Fail("Employee not found");

        var deleted = await _employeeRepository.DeleteEmployeeAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(deleted, "Employee deleted successfully")
            : ServiceResult<bool>.Fail("Fail to Delete employee");
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
