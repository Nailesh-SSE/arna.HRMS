using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Admin.Employee;

public interface IEmployeeService
{
    Task<ApiResult<List<EmployeeDto>>> GetEmployeesAsync();
    Task<ApiResult<EmployeeDto>> GetEmployeeByIdAsync(int id);
    Task<ApiResult<EmployeeDto>> CreateEmployeeAsync(EmployeeDto dto);
    Task<ApiResult<bool>> UpdateEmployeeAsync(int id, EmployeeDto dto);
    Task<ApiResult<bool>> DeleteEmployeeAsync(int id);
}

public class EmployeeService : IEmployeeService
{
    private readonly ApiClients.EmployeeApi _employees;

    public EmployeeService(ApiClients api)
    {
        _employees = api.Employees;
    }

    public Task<ApiResult<List<EmployeeDto>>> GetEmployeesAsync()
        => _employees.GetAll();

    public Task<ApiResult<EmployeeDto>> GetEmployeeByIdAsync(int id)
        => _employees.GetById(id);

    public Task<ApiResult<EmployeeDto>> CreateEmployeeAsync(EmployeeDto dto)
        => _employees.Create(dto);

    public Task<ApiResult<bool>> UpdateEmployeeAsync(int id, EmployeeDto dto)
        => _employees.Update(id, dto);

    public Task<ApiResult<bool>> DeleteEmployeeAsync(int id)
        => _employees.Delete(id);
}
