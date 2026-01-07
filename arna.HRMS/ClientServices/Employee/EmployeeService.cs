using arna.HRMS.ClientServices.Common;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Employee;

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
    private readonly ApiClients _api;

    public EmployeeService(ApiClients api)
    {
        _api = api;
    }

    public Task<ApiResult<List<EmployeeDto>>> GetEmployeesAsync()
        => _api.Employees.GetAllAsync();

    public Task<ApiResult<EmployeeDto>> GetEmployeeByIdAsync(int id)
        => _api.Employees.GetByIdAsync(id);

    public Task<ApiResult<EmployeeDto>> CreateEmployeeAsync(EmployeeDto dto)
        => _api.Employees.CreateAsync(dto);

    public Task<ApiResult<bool>> UpdateEmployeeAsync(int id, EmployeeDto dto)
        => _api.Employees.UpdateAsync(id, dto);

    public Task<ApiResult<bool>> DeleteEmployeeAsync(int id)
        => _api.Employees.DeleteAsync(id);
}
