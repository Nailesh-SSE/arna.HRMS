using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Services.Http;

namespace arna.HRMS.Services;

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

    public async Task<ApiResult<List<DepartmentViewModel>>> GetDepartmentsAsync()
    {
        return await _departments.GetAllAsync();
    }

    public async Task<ApiResult<DepartmentViewModel>> GetDepartmentByIdAsync(int id)
    {
        return await _departments.GetByIdAsync(id);
    }

    public async Task<ApiResult<DepartmentViewModel>> CreateDepartmentAsync(DepartmentViewModel model)
    {
        return await _departments.CreateAsync(model);
    }

    public async Task<ApiResult<bool>> UpdateDepartmentAsync(int id, DepartmentViewModel model)
    {
        return await _departments.UpdateAsync(id, model);
    }

    public async Task<ApiResult<bool>> DeleteDepartmentAsync(int id)
    {
        return await _departments.DeleteAsync(id);
    }
}