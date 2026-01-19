using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Admin.Department;

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
    private readonly ApiClients.DepartmentApi _departments;

    public DepartmentService(ApiClients api)
    {
        _departments = api.Departments;
    }

    public Task<ApiResult<List<DepartmentDto>>> GetDepartmentsAsync()
        => _departments.GetAll();

    public Task<ApiResult<DepartmentDto>> GetDepartmentByIdAsync(int id)
        => _departments.GetById(id);

    public Task<ApiResult<DepartmentDto>> CreateDepartmentAsync(DepartmentDto dto)
        => _departments.Create(dto);

    public Task<ApiResult<bool>> UpdateDepartmentAsync(int id, DepartmentDto dto)
        => _departments.Update(id, dto);

    public Task<ApiResult<bool>> DeleteDepartmentAsync(int id)
        => _departments.Delete(id);
}
