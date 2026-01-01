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
        var data = await _employeeRepository.GetEmployeesAsync();
        var lastEmployeeNumber = data.Where(e => e.EmployeeNumber != null).OrderByDescending(e=>e.EmployeeNumber).Select(e=>e.EmployeeNumber).FirstOrDefault();
        int nextNumber = 1;

        if (lastEmployeeNumber != null)
        {
            string numberPart = lastEmployeeNumber.Replace("Emp", "");
            int currentNumber = int.Parse(numberPart);
            nextNumber = currentNumber + 1;
        }
        employee.EmployeeNumber = "Emp" + nextNumber.ToString("D3");
        var createdEmployee = await _employeeRepository.CreateEmployeeAsync(employee);
        return _mapper.Map<EmployeeDto>(createdEmployee);
       
    }
    public async Task<bool> DeleteEmployeeAsync(int id) 
    {
        return await _employeeRepository.DeleteEmployeeAsync(id);
    }

    public async Task<EmployeeDto> UpdateEmployeeAsync(Employee employee)
    {
        var updatedEmployee = await _employeeRepository.UpdateEmployeeAsync(employee);
        return _mapper.Map<EmployeeDto>(updatedEmployee);
    }
    public async Task<bool> EmailAndPhoneNumberExistAsync(string email, string phoneNumber)
    {
        return await _employeeRepository.EmployeeExistsAsync(email, phoneNumber);
    }
}
