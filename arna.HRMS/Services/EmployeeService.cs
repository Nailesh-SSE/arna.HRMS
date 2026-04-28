using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Services.Http;

namespace arna.HRMS.Services;

public interface IEmployeeService
{
    Task<ApiResult<List<EmployeeViewModel>>> GetEmployeesAsync();
    Task<ApiResult<EmployeeViewModel>> GetEmployeeByIdAsync(int id);
    Task<ApiResult<EmployeeViewModel>> CreateEmployeeAsync(EmployeeViewModel model);
    Task<ApiResult<bool>> UpdateEmployeeAsync(int id, EmployeeViewModel model);
    Task<ApiResult<bool>> DeleteEmployeeAsync(int id);
}

public class EmployeeService : IEmployeeService
{
    private readonly ApiClients.EmployeeApi _employees;

    public EmployeeService(ApiClients api)
    {
        _employees = api.Employees;
    }

    public async Task<ApiResult<List<EmployeeViewModel>>> GetEmployeesAsync()
    {
        return await _employees.GetAllAsync();
    }

    public async Task<ApiResult<EmployeeViewModel>> GetEmployeeByIdAsync(int id)
    {
        return await _employees.GetByIdAsync(id);
    }

    public async Task<ApiResult<EmployeeViewModel>> CreateEmployeeAsync(EmployeeViewModel model)
    {
        return await _employees.CreateAsync(model);
    }

    public async Task<ApiResult<bool>> UpdateEmployeeAsync(int id, EmployeeViewModel model)
    {
        return await _employees.UpdateAsync(id, model);
    }

    public async Task<ApiResult<bool>> DeleteEmployeeAsync(int id)
    {
        return await _employees.DeleteAsync(id);
    }
}