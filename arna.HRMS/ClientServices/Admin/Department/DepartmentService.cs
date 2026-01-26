using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.ViewModels;

namespace arna.HRMS.ClientServices.Admin.Department;

public interface IDepartmentService
{
    Task<ApiResult<List<DepartmentViewModel>>> GetDepartmentsAsync();
    Task<ApiResult<DepartmentViewModel>> GetDepartmentByIdAsync(int id);
    Task<ApiResult<DepartmentViewModel>> CreateDepartmentAsync(DepartmentViewModel model);
    Task<ApiResult<bool>> UpdateDepartmentAsync(int id, DepartmentViewModel model);
    Task<ApiResult<bool>> DeleteDepartmentAsync(int id);
}

public class DepartmentService : IDepartmentService
{
    private readonly ApiClients.DepartmentApi _departments;

    public DepartmentService(ApiClients api)
    {
        _departments = api.Departments;
    }

    public Task<ApiResult<List<DepartmentViewModel>>> GetDepartmentsAsync()
        => _departments.GetAll();

    public Task<ApiResult<DepartmentViewModel>> GetDepartmentByIdAsync(int id)
        => _departments.GetById(id);

    public Task<ApiResult<DepartmentViewModel>> CreateDepartmentAsync(DepartmentViewModel model)
        => _departments.Create(model);

    public Task<ApiResult<bool>> UpdateDepartmentAsync(int id, DepartmentViewModel model)
        => _departments.Update(id, model); 

    public Task<ApiResult<bool>> DeleteDepartmentAsync(int id)
        => _departments.Delete(id);
}
