using arna.HRMS.ClientServices.Common;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Department;

public interface IDepartmentService
{
    Task<ApiResult<List<DepartmentDto>>> GetDepartmentsAsync();
    Task<ApiResult<DepartmentDto>> GetDepartmentByIdAsync(int id);
    Task<ApiResult<DepartmentDto>> CreateDepartmentAsync(DepartmentDto dto);
    Task<ApiResult<bool>> UpdateDepartmentAsync(int id, DepartmentDto dto);
    Task<ApiResult<bool>> DeleteDepartmentAsync(int id);
}

public class DepartmentService : IDepartmentService
{
    private readonly ApiClients _api;

    public DepartmentService(ApiClients api)
    {
        _api = api;
    }

    public Task<ApiResult<List<DepartmentDto>>> GetDepartmentsAsync()
        => _api.Departments.GetAllAsync();

    public Task<ApiResult<DepartmentDto>> GetDepartmentByIdAsync(int id)
        => _api.Departments.GetByIdAsync(id);

    public Task<ApiResult<DepartmentDto>> CreateDepartmentAsync(DepartmentDto dto)
        => _api.Departments.CreateAsync(dto);

    public Task<ApiResult<bool>> UpdateDepartmentAsync(int id, DepartmentDto dto)
        => _api.Departments.UpdateAsync(id, dto);

    public Task<ApiResult<bool>> DeleteDepartmentAsync(int id)
        => _api.Departments.DeleteAsync(id);
}
