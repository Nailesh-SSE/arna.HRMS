using arna.HRMS.ClientServices.Common;
using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Employee;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetEmployeesAsync();
    Task<ApiResult<EmployeeDto?>> GetEmployeeByIdAsync(int id);
    Task<ApiResult<EmployeeDto>> CreateEmployeeAsync(EmployeeDto employeeDto);
    Task<ApiResult<bool>> DeleteEmployeeAsync(int id);
    Task<ApiResult<bool>> UpdateEmployeeAsync(int id, UpdateEmployeeRequest employeeDto);
}
public class EmployeeService : IEmployeeService
{
    private readonly HttpService HttpClient;
    public EmployeeService(HttpService HttpClient)
    {
        this.HttpClient = HttpClient;
    }

    public async Task<List<EmployeeDto>> GetEmployeesAsync()
    {
        var result = await HttpClient.GetAsync<List<EmployeeDto>>("api/employees");
        return result.Data ?? new List<EmployeeDto>();
    }

    public async Task<ApiResult<EmployeeDto?>> GetEmployeeByIdAsync(int id)
    {
        var response = await HttpClient.GetAsync<EmployeeDto>($"api/employees/{id}");
        return response;
    }

    public async Task<ApiResult<EmployeeDto>> CreateEmployeeAsync(EmployeeDto employeeDto)
    {
        return await HttpClient.PostAsync<EmployeeDto>("api/employees", employeeDto);
    }
    public async Task<ApiResult<bool>> DeleteEmployeeAsync(int id)
    {
        var respnce = await HttpClient.DeleteAsync<bool>($"api/employees/{id}");
        return respnce;
    }
    public async Task<ApiResult<bool>> UpdateEmployeeAsync(int id,UpdateEmployeeRequest employeeDto)
    {
        return await HttpClient.PutAsync<bool>($"api/employees/{id}", employeeDto);
    }
}
