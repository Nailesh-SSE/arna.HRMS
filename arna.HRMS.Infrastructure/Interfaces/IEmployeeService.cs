using arna.HRMS.Models.DTOs;
namespace arna.HRMS.Infrastructure.Interfaces;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetEmployeesAsync();
    Task<EmployeeDto> GetEmployeeByIdAsync(int id);
    Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto dto);
    Task<EmployeeDto> UpdateEmployeeAsync(EmployeeDto dto);
    Task<bool> DeleteEmployeeAsync(int id);
    Task<bool> EmployeeExistsAsync(string email, string phoneNumber);
}