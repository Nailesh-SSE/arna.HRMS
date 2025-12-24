using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;
namespace arna.HRMS.Infrastructure.Interfaces;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    Task<EmployeeDto> CreateEmployeeAsync(Employee employee);
    Task<bool> DeleteEmployeeAsync(int id);
    Task<EmployeeDto> UpdateEmployeeAsync(Employee employee);
}