using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;

namespace arna.HRMS.Core.Interfaces.Service;

public interface IEmployeeService
{
    Task<ServiceResult<List<EmployeeDto>>> GetEmployeesAsync();
    Task<ServiceResult<EmployeeDto?>> GetEmployeeByIdAsync(int id);
    Task<ServiceResult<EmployeeDto>> CreateEmployeeAsync(EmployeeDto dto);
    Task<ServiceResult<EmployeeDto>> UpdateEmployeeAsync(EmployeeDto dto);
    Task<ServiceResult<bool>> DeleteEmployeeAsync(int id);
}
