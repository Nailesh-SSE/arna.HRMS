using arna.HRMS.ClientServices.Common;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Department;

public interface IDepartmentService
{
    Task<ApiResult<List<DepartmentDto>>>  GetDepartmentsAsync();
    Task<ApiResult<DepartmentDto>> GetDepartmentByIdAsync(int id);
    Task<ApiResult<DepartmentDto>> CreateDepartmentAsync(DepartmentDto DepartmentDto);
    Task<ApiResult<bool>> DeleteDepartmentAsync(int id);
    Task<ApiResult<bool>> UpdateDepartmentAsync(int id, DepartmentDto departmentDto);
}
public class DepartmentService : IDepartmentService
{
    private readonly HttpService _http;

    public DepartmentService(HttpService http)
    {
        _http = http;
    }

    public async Task<ApiResult<List<DepartmentDto>>> GetDepartmentsAsync()
    {
        return await _http.GetAsync<List<DepartmentDto>>("api/department");
    }

    public async Task<ApiResult<DepartmentDto>> GetDepartmentByIdAsync(int id)
    {
        return await _http.GetAsync<DepartmentDto>($"api/department/{id}");
    }

    public async Task<ApiResult<DepartmentDto>> CreateDepartmentAsync(DepartmentDto DepartmentDto)
    {
        return await _http.PostAsync<DepartmentDto>("api/department", DepartmentDto);
    }
    public async Task<ApiResult<bool>> UpdateDepartmentAsync(int id, DepartmentDto departmentDto)
    {
        return await _http.PutAsync<bool>($"api/department/{id}", departmentDto);
    }

    public async Task<ApiResult<bool>> DeleteDepartmentAsync(int id)
    {
        return await _http.DeleteAsync<bool>($"api/department/{id}");
    }
}