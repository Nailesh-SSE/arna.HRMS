using arna.HRMS.ClientServices.Common;
using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Employee;

public interface IEmployeeService
{
    Task<ApiResult<List<EmployeeDto>>> GetEmployeesAsync();
    Task<ApiResult<EmployeeDto>> GetEmployeeByIdAsync(int id);
    Task<ApiResult<EmployeeDto>> CreateEmployeeAsync(EmployeeDto employeeDto);
    Task<ApiResult<bool>> UpdateEmployeeAsync(int id, EmployeeDto employeeDto);
    Task<ApiResult<bool>> DeleteEmployeeAsync(int id);
}

public class EmployeeService : IEmployeeService
{
    private readonly HttpService _http;

    public EmployeeService(HttpService http)
    {
        _http = http;
    }

    public async Task<ApiResult<List<EmployeeDto>>> GetEmployeesAsync()
    {
        return await _http.GetAsync<List<EmployeeDto>>("api/employees");
    }

    public async Task<ApiResult<EmployeeDto>> GetEmployeeByIdAsync(int id)
    {
        return await _http.GetAsync<EmployeeDto>($"api/employees/{id}");
    }

    public async Task<ApiResult<EmployeeDto>> CreateEmployeeAsync(EmployeeDto employeeDto)
    {
        return await _http.PostAsync<EmployeeDto>("api/employees", employeeDto);
    }

    public async Task<ApiResult<bool>> UpdateEmployeeAsync(int id, EmployeeDto employeeDto)
    {
        return await _http.PutAsync<bool>($"api/employees/{id}", employeeDto);
    }

    public async Task<ApiResult<bool>> DeleteEmployeeAsync(int id)
    {
        return await _http.DeleteAsync<bool>($"api/employees/{id}");
    }
}
