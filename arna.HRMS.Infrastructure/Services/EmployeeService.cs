using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Models.DTOs;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly EmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;

    public EmployeeService(EmployeeRepository employeeRepository, IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }

    public async Task<List<EmployeeDto>> GetEmployeesAsync()
    {
        var employees = await _employeeRepository.GetEmployeesAsync();
        return _mapper.Map<List<EmployeeDto>>(employees);
    }

    public async Task<EmployeeDto> GetEmployeeByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
        return employee == null ? new EmployeeDto() : _mapper.Map<EmployeeDto>(employee);
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto dto)
    {
        var employee = _mapper.Map<Employee>(dto);

        var lastEmployeeNumber = await _employeeRepository.GetLastEmployeeNumberAsync();
        employee.EmployeeNumber = GenerateEmployeeNumber(lastEmployeeNumber);

        var created = await _employeeRepository.CreateEmployeeAsync(employee);
        return _mapper.Map<EmployeeDto>(created);
    }

    public async Task<EmployeeDto> UpdateEmployeeAsync(EmployeeDto dto)
    {
        var employee = _mapper.Map<Employee>(dto);
        var updated = await _employeeRepository.UpdateEmployeeAsync(employee);
        return _mapper.Map<EmployeeDto>(updated);
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        return await _employeeRepository.DeleteEmployeeAsync(id);
    }

    public async Task<bool> EmployeeExistsAsync(string email, string phoneNumber)
    {
        return await _employeeRepository.EmployeeExistsAsync(email, phoneNumber);
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
