using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.ViewModels;

namespace arna.HRMS.ClientServices.Admin.Employee;

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

    public Task<ApiResult<List<EmployeeViewModel>>> GetEmployeesAsync()
        => _employees.GetAll();

    public Task<ApiResult<EmployeeViewModel>> GetEmployeeByIdAsync(int id)
        => _employees.GetById(id);

    public Task<ApiResult<EmployeeViewModel>> CreateEmployeeAsync(EmployeeViewModel model)
        => _employees.Create(model);

    public Task<ApiResult<bool>> UpdateEmployeeAsync(int id, EmployeeViewModel model)
        => _employees.Update(id, model);

    public Task<ApiResult<bool>> DeleteEmployeeAsync(int id)
        => _employees.Delete(id); 
}
