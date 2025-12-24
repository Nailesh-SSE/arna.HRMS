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
        return employees.Select(e => _mapper.Map<EmployeeDto>(e)).ToList();
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
        if (employee == null) return null;
        //var employeedto = new EmployeeDto();
        var employeedto = _mapper.Map<EmployeeDto>(employee);

        return employeedto;
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(Employee employee)
    {
        var createdEmployee = await _employeeRepository.CreateEmployeeAsync(employee);
        return _mapper.Map<EmployeeDto>(createdEmployee);
    }
    public async Task<bool> DeleteEmployeeAsync(int id) 
    {
        var employeeDelete = await _employeeRepository.DeleteEmployeeAsync(id);
        return employeeDelete;
    }

    public async Task<EmployeeDto> UpdateEmployeeAsync(Employee employee)
    {
        var updatedEmployee = await _employeeRepository.UpdateEmployeeAsync(employee);
        return _mapper.Map<EmployeeDto>(updatedEmployee);
    }
}
